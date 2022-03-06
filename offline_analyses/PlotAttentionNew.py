import time

# controls working directory for relative paths depending on whether script is run from shell
import BuildControl

import os
import importlib

import pandas as pd
import numpy as np
from pyarrow import feather

import matplotlib.pyplot as plt
import seaborn as sns

from astropy import coordinates
from PIL import Image
import json
from subprocess import check_output
from fpdf import FPDF  # fpdf class

import DataStructures
import MergeImages
import ThreeD
import LoadData
import Common
from PyPDF2 import PdfFileMerger

## read command line arguments

if not BuildControl.isRunningInPyCharm:
    import argparse

    # Initialize parser
    parser = argparse.ArgumentParser()

    # Adding optional argument
    parser.add_argument("-s", "--showResultsBoolean")
    parser.add_argument("-a", "--analyseGameBoolean")

    # Read arguments from command line
    args = parser.parse_args()

    def str2bool(v):
        return v.lower() in "true"

    if args.showResultsBoolean:
        print("showResultsBoolean: % s" % args.showResultsBoolean)
        isShowResults = str2bool(args.showResultsBoolean)
    else:
        isShowResults = False

    if args.analyseGameBoolean:
        print("showResultsBoolean: % s" % args.analyseGameBoolean)
        isAnalyseGameAsWhole = str2bool(args.analyseGameBoolean)
    else:
        isAnalyseGameAsWhole = False
else:
    isShowResults = False
    isAnalyseGameAsWhole = False

isAnalyseGameAsWhole = True
print(str.format("isShowResults: {}", isShowResults))
print(str.format("isAnalyseGameAsWhole: {}", isAnalyseGameAsWhole))

## ----- settings

scatterSize = 5
elementSize = 50
scatterAlpha = .25
plotLimit = 60
histogramSpacing = 1
latitudeEdges = np.arange(-plotLimit, plotLimit + 1, histogramSpacing)
longitudeEdges = np.arange(-plotLimit, plotLimit + 1, histogramSpacing)
nSim = 100
dpi = 150
fontSize = 18
gridOn = False


# ----- methods

def CartesianToSphericalDataFrame(df):
    df = df.reset_index()
    [r, lon, lat] = coordinates.cartesian_to_spherical(df.x, df.z, df.y)  # different order than contained in main file
    lat = np.rad2deg(np.array(lat)) - 180
    lat = lat * -1  # hack solution? be very wary - reverses data left and right
    lon = np.rad2deg(np.array(lon))
    df['r'] = r
    df['lat'] = lat
    df['lon'] = lon
    df = df[(~df.lat.isnull() & ~df.lon.isnull())]
    return df


def DrawCrossHairs(color):
    plt.axhline(linewidth=1, color=color)
    plt.axvline(linewidth=1, color=color)


def PlotRayCastHeatMap(lat, lon, latEdges, lonEdges, cmap, title="", vertices=None, filename=""):
    plt.close('all')
    H, latEdges, lonEdges = np.histogram2d(x=lon, y=lat, bins=(latEdges, lonEdges))  # reversed?
    H = H / H.sum() * 100  # convert to percentage
    H = H.astype(np.float32)

    vmin = 0
    vmax = np.percentile(H, 99)

    if vmax == 0:
        vmax = np.max(H)

    print("vmin:{}, vmax:{}".format(vmin, vmax))

    figure, axis = plt.subplots(1, 1)
    im = plt.imshow(H, interpolation='none',
                    origin='lower',
                    extent=[latEdges[0], latEdges[-1], lonEdges[0], lonEdges[-1]],
                    cmap=cmap,
                    vmin=vmin,
                    vmax=vmax)

    if title != "":
        plt.title(title, fontdict={'fontsize': fontSize})

    axis.set_xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    axis.set_ylabel("Longitude (°)", fontdict={'fontsize': fontSize})

    colorBar = plt.colorbar(im)
    colorBar.ax.set_title('Freq. (%)', fontdict={'fontsize': fontSize})
    plt.gca().set_aspect('equal', adjustable='box')

    if gridOn:
        plt.grid(color=[1, 1, 1], linewidth=2)

    # DrawCrossHairs('b')

    if vertices is not None:
        plt.scatter(vertices.lat, vertices.lon, color="b")

    # plt.tight_layout()

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)

    return H, latEdges, lonEdges


def OneSamplePermutationTest(x, nsim=500):
    # https://stats.stackexchange.com/questions/65831/permutation-test-comparing-a-single-sample-against-a-mean
    n = len(x)
    dbar = np.nanmean(x)
    z = np.empty(shape=(nsim, 1))
    z[:] = np.nan

    # Run the simulation
    for i in range(0, nsim):
        mn = np.random.choice([-1, +1], n) # 1. take n random draws from {-1, 1}, where n is the length of the data to be tested
        dbardash = np.mean(mn * abs(x))  # 2. assign the signs to the data and put them in a temporary variable
        z[i] = dbardash  # 3. save the new data in an array

    # Return the p value
    # p = the fraction of fake data that is: larger than |sample mean of x|, or smaller than -|sample mean of x|
    pval = (np.sum(z >= abs(dbar)) + np.sum(z <= -abs(dbar)))/nsim
    return z, pval


def LatitudePlot(lat, title="", filename=""):
    plt.close('all')
    figure, axes = plt.subplots(2, 1, sharex='all', gridspec_kw={'height_ratios': [3, 1]})
    plt.sca(axes[0])
    sns.violinplot(x=lat)
    plt.xlabel("")
    axes[0].set_yticks([])

    if gridOn:
        plt.grid(linewidth=2)

    plt.sca(axes[1])
    z, pval = OneSamplePermutationTest(lat, nSim)
    sns.violinplot(x=z)
    sns.despine()
    sns.despine(left=True)

    if title != "":
        axes[0].set_title(title, fontdict={'fontsize': fontSize})

    plt.xlim([-plotLimit, +plotLimit])
    plt.axvline(np.nanmean(lat), color='red')
    sns.despine()
    sns.despine(left=True)
    axes[1].set_xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    axes[1].set_yticks([])

    if gridOn:
        plt.grid(linewidth=2)

    plt.title("M = {:.2f}, SD = {:.2f}, p = {:.3f}".format(np.nanmean(lat), np.nanstd(lat), pval), fontdict={'fontsize': fontSize})
    plt.tight_layout()
    #
    # figure.subplots_adjust(
    #     top=0.939,
    #     bottom=0.121,
    #     left=0.195,
    #     right=0.795,
    #     hspace=0.232,
    #     wspace=0.2
    # )
    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def LongitudePlot(lon, title="", filename=""):
    plt.close('all')
    figure, axes = plt.subplots(1, 2, sharey=True, gridspec_kw={'width_ratios': [3, 1]})
    plt.sca(axes[0])
    sns.violinplot(y=lon)

    if gridOn:
        plt.grid(linewidth=2)
    plt.xlabel("")

    axes[0].set_xticks([])
    axes[0].set_ylabel("Longitude (°)", fontdict={'fontsize': fontSize})

    plt.sca(axes[1])
    z, pval = OneSamplePermutationTest(lon, nSim)
    sns.violinplot(y=z)
    sns.despine()
    sns.despine(bottom=True)
    if gridOn:
        plt.grid(axis='x', linewidth=2)

    if title != "":
        axes[0].set_title(title, fontdict={'fontsize': fontSize})

    plt.axhline(np.nanmean(lon), color='red')
    sns.despine()
    sns.despine(bottom=True)

    plt.ylabel("M = {:.2f}, SD = {:.2f}, p = {:.3f}".format(np.nanmean(lon), np.nanstd(lon), pval), fontdict={'fontsize': fontSize})

    plt.ylim([-plotLimit, +plotLimit])

    plt.tight_layout()
    #
    # figure.subplots_adjust(
    #     top=0.939,
    #     bottom=0.121,
    #     left=0.195,
    #     right=0.795,
    #     hspace=0.232,
    #     wspace=0.2
    # )
    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def PlotBehaviour(trialData, filename):
    plt.close('all')
    figure, axes = plt.subplots(2, 1)

    axes[0].plot(trialData['trialData.trial'], trialData['GeneralMethods.BoolToFloat(trialData.array.isCorrect)'],
                 color='k', linewidth=1)

    axes[0].scatter(trialData['trialData.trial'], trialData['GeneralMethods.BoolToFloat(trialData.array.isCorrect)'],
                 color='k', s=20)

    axes[0].set_ylabel('Accuracy', fontdict={'fontsize': fontSize})
    axes[0].set_ylim(-.1, 1.1)
    axes[0].set_yticks(ticks=[0, 1])
    axes[0].set_xticklabels([])
    sns.despine(ax=axes[0])
    axes[0].set_title("Accuracy = ({:.2f}%)".format(
        np.nanmean(trialData['GeneralMethods.BoolToFloat(trialData.array.isCorrect)'] * 100)), fontdict={'fontsize': fontSize})

    axes[1].plot(trialData['trialData.trial'], trialData['(float) trialData.array.RT'] / 1000, color='k', linewidth=1)
    axes[1].scatter(trialData['trialData.trial'], trialData['(float) trialData.array.RT'] / 1000, color='k', s=20)

    axes[1].set_xlabel('Trial Number', fontdict={'fontsize': fontSize})
    axes[1].set_ylabel('RT (s)', fontdict={'fontsize': fontSize})
    sns.despine(ax=axes[1])
    M = (trialData['(float) trialData.array.RT']/1000).mean(skipna=True)
    SD = (trialData['(float) trialData.array.RT']/1000).std(skipna=True)
    axes[1].set_title("M = ({:.2f}), SD = ({:.2f})".format(M, SD), fontdict={'fontsize': fontSize})

    if filename != 0:
        plt.savefig(fname=filename, dpi=dpi)


def PlotElements(lat, lon, title="", filename="", s=5):
    plt.close('all')
    figure, axes = plt.subplots(1, 1)
    plt.scatter(lat, lon, color=[0, 0, 0], s=s, alpha=scatterAlpha)
    plt.xlim(-plotLimit, +plotLimit)
    plt.ylim(-plotLimit, +plotLimit)
    plt.xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    plt.ylabel("Longitude (°)", fontdict={'fontsize': fontSize})
    if gridOn:
        plt.grid(linewidth=2)
    plt.gca().set_aspect('equal', adjustable='box')
    # DrawCrossHairs(color="k")

    if title != "":
        plt.title(title, fontdict={'fontsize': fontSize})

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def LatitudeLongitudeScatterOneSource(lat, lon, title="", filename="", s=5):
    plt.close('all')
    figure, axes = plt.subplots(1, 1)
    plt.scatter(lat, lon, color=[0, 0, 0], s=s, alpha=scatterAlpha)
    plt.xlim(-plotLimit, +plotLimit)
    plt.ylim(-plotLimit, +plotLimit)
    plt.xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    plt.ylabel("Longitude (°)", fontdict={'fontsize': fontSize})
    if gridOn:
        plt.grid(linewidth=2)
    plt.gca().set_aspect('equal', adjustable='box')
    # DrawCrossHairs(color="k")

    if title != "":
        plt.title(title, fontdict={'fontsize': fontSize})

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def LatitudeLongitudeScatterAllSource(raycastData, title="", filename=""):
    plt.close('all')
    figure, axes = plt.subplots(1, 1)

    colorDictionary = {DataStructures.RayCastSources.Headset.name: "r",
                       DataStructures.RayCastSources.Controller.name: 'b',
                       DataStructures.RayCastSources.Gaze.name: [0, 0, 0]}

    for source in DataStructures.RayCastSources:
        plt.scatter(x=raycastData.lat[source.name == raycastData.ID_string],
                    y=raycastData.lon[source.name == raycastData.ID_string],
                    color=colorDictionary[source.name],
                    alpha=scatterAlpha,
                    s=scatterSize)

    plt.gca().set_aspect('equal', adjustable='box')

    legend = plt.legend([source.name for source in DataStructures.RayCastSources])

    for i in range(0, 3):
        legend.legendHandles[i]._sizes = [100]

    plt.xlim(-plotLimit, +plotLimit)
    plt.ylim(-plotLimit, +plotLimit)
    plt.xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    plt.ylabel("Longitude (°)", fontdict={'fontsize': fontSize})

    if gridOn:
        plt.grid(linewidth=2)
    # plt.tight_layout()

    # DrawCrossHairs(color='k')

    if title != "":
        plt.title(title, fontdict={'fontsize': fontSize})

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def GetLostFrames(observer):
    totalFrames = int((observer.frameData['Time.frameCount'] - observer.frameData['Time.frameCount'][0]).iloc[-1]) + 1
    totalFramesRecorded = len(observer.frameData)
    lostFrames = totalFrames - totalFramesRecorded
    lostProportion = lostFrames/totalFrames
    framesString = "total frames:{}, recorded:{}, lost:{}".format(totalFrames, totalFramesRecorded, lostFrames)
    return framesString


def PlotTiming(frameData, optionsNumber, title="", filename=""):
    plt.close('all')
    if optionsNumber == -1:
        index = frameData['(float) GameRunner.trialData.state'] == 2
        timeData = frameData['Time.time'].copy(deep=True)
        timeData[~index] = np.nan
    else:
        index = (frameData['(float)GameRunner.currentTrialData.optionsNumber'] == optionsNumber) & (frameData['(float) GameRunner.trialData.state'] == 2)
        timeData = frameData['Time.time'].copy(deep=True)
        timeData[~index] = np.nan

    flipTimes = np.diff(timeData) * 1000
    experimentTime = timeData[1:] / 60

    title = "Frames (n): {}, Time (min): {:.2f}".format(sum(index), np.nansum(flipTimes)/1000/60)

    figure, axes = plt.subplots(2, 1)
    axes[0].plot(experimentTime, flipTimes, linewidth=1, color='k')
    axes[0].set_xlabel('Time (minutes)', fontdict={'fontsize': fontSize})
    axes[0].set_ylabel(r'$\Delta$ Frame (ms)', fontdict={'fontsize': fontSize})
    axes[0].set_ylim(0, 100)

    counts, bins = np.histogram(flipTimes[~np.isnan(flipTimes)], bins=1000)
    counts = counts/counts.sum() * 100  # convert to percentage

    axes[1].bar(bins[:-1] + np.diff(bins) / 2, counts/len(flipTimes), np.diff(bins), color='k')
    axes[1].set_ylabel('Freq. (%)', fontdict={'fontsize': fontSize})
    axes[1].set_xlabel(r'$\Delta$ Frame (ms)', fontdict={'fontsize': fontSize})
    axes[1].set_title("Mean FPS: {:.2f} Hz".format(1000/np.nanmean(flipTimes)), fontdict={'fontsize': fontSize})
    axes[1].set_xlim(0, 100)

    sns.despine(figure, ax=0, top=True, right=True)

    if title != "":
        axes[0].set_title(title, fontdict={'fontsize': fontSize})

    plt.tight_layout()

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def GenerateFilename(observer, modifier, optionsNumber):
    filename = observer.pathResults + observer.levelStartTime + "." + modifier + "." + str(optionsNumber) + ".png"
    print(filename)
    return filename


def GetTrialData(observer, optionsNumber, isWriteOutput=True):
    trialData = observer.trialData.copy(deep=True)

    if optionsNumber != -1:
        trialData = trialData[trialData['trialData.optionsNumber'] == optionsNumber]
    print("len(trialData): %s" % len(trialData))

    if isWriteOutput:
        feather.write_feather(trialData, observer.pathResults + observer.levelStartTime +
                              ".trialData." + str(optionsNumber) + ".feather")
    return trialData


def GetBehaviourMapData(observer, vertices):
    optionsRange = range(0, len(observer.level['listOptions']))
    DF = []
    for optionsNumber in optionsRange:
        index = vertices["serializableVertex.optionsNumber"] == optionsNumber
        verticesToUse = vertices.loc[index, ["serializableVertex.index", "lat", "lon"]]
        index = observer.trialData['trialData.optionsNumber'] == optionsNumber
        trialData = observer.trialData[index]
        N = trialData.loc[index, '(float) trialData.targetPosition'].value_counts()
        accuracy = trialData.loc[index, ['(float) trialData.targetPosition', 'GeneralMethods.BoolToFloat(trialData.array.isCorrect)']].groupby('(float) trialData.targetPosition').mean() * 100
        RT_M = trialData.loc[index, ['(float) trialData.targetPosition', '(float) trialData.array.RT']].groupby('(float) trialData.targetPosition').mean()/1000
        RT_SD = trialData.loc[index, ['(float) trialData.targetPosition', '(float) trialData.array.RT']].groupby('(float) trialData.targetPosition').std()
        data = [N, accuracy, RT_M, RT_SD]
        df = pd.concat(data, axis=1, keys=[s.keys for s in data])
        df.columns = ["N", "accuracy", "RT_M", "RT_SD"]
        df.reset_index(level=0, inplace=True)
        df = df.merge(right=verticesToUse, left_on="index", right_on="serializableVertex.index")
        df['optionsNumber'] = optionsNumber
        DF.append(df)
        # M = (trialData['(float) trialData.array.RT'] / 1000).mean(skipna=True)
        # SD = (trialData['(float) trialData.array.RT'] / 1000).std(skipna=True)
        # print("M:{:2f}, SD:{:2f}".format(M, SD))

    DF = pd.concat(DF)
    return DF


def PlotBehaviourMap(lat, lon, z, column, filename=""):
    dict = {'N': ['Target Count', 'n', 'gray'], 'accuracy': ['Accuracy', "%", 'gray'], 'RT_M': ["RT", "(s)", 'gray_r']}

    if column == "accuracy":
        vmin = 0
        vmax = 100
    else:
        vmin = min(z)
        vmax = max(z)

    # plt.close('all')
    figures, axes = plt.subplots(1, 1)
    sc = plt.scatter(lat, lon, c=z, vmin=vmin, vmax=vmax, s=100, cmap=dict[column][2], edgecolors='k')
    colorBar = plt.colorbar()
    colorBar.ax.set_title(dict[column][1], fontdict={'fontsize': fontSize})

    if column == "N":
        colorBar.set_ticks([int(vmin), int(vmax)])

    plt.gca().set_aspect('equal', adjustable='box')
    plt.xlim(-plotLimit, +plotLimit)
    plt.ylim(-plotLimit, +plotLimit)

    plt.xlabel("Latitude (°)", fontdict={'fontsize': fontSize})
    plt.ylabel("Longitude (°)", fontdict={'fontsize': fontSize})
    if gridOn:
        plt.grid(linewidth=2)

    plt.title(dict[column][0], fontdict={'fontsize': fontSize})

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)


def GetRaycastData(observer, optionsNumber, isLimitToArray=True, isWriteOutputFile=True):
    if isLimitToArray:
        # get array frames
        raycastAll = observer.frameData[observer.frameData['(float) GameRunner.trialData.state'] == 2].copy(deep=True)
    else:
        raycastAll = observer.frameData.copy(deep=True)

    if optionsNumber != -1:
        # get specific options frame
        raycastAll = raycastAll.loc[observer.frameData['(float)GameRunner.currentTrialData.optionsNumber'] == optionsNumber]

    raycastHead = raycastAll[['Time.time',
                              'Time.frameCount',
                              'headsetSurfacePosition.x',
                              'headsetSurfacePosition.y',
                              'headsetSurfacePosition.z']].copy()

    raycastHead['ID_string'] = DataStructures.RayCastSources.Headset.name
    raycastHead['ID_number'] = DataStructures.RayCastSources.Headset.value
    raycastHead.columns = ['time', 'frameNumber', 'x', 'y', 'z', 'ID_string', 'ID_number']

    raycastController = raycastAll[['Time.time', 'Time.frameCount',
                                    'controller1SurfacePosition.x',
                                    'controller1SurfacePosition.y',
                                    'controller1SurfacePosition.z']].copy()

    raycastController['ID_string'] = DataStructures.RayCastSources.Controller.name
    raycastController['ID_number'] = DataStructures.RayCastSources.Controller.value
    raycastController.columns = ['time', 'frameNumber', 'x', 'y', 'z', 'ID_string', 'ID_number']

    raycastEye = raycastAll[['Time.time', 'Time.frameCount',
                             'eyeGazeSurfacePosition.x',
                             'eyeGazeSurfacePosition.y',
                             'eyeGazeSurfacePosition.z']].copy()

    raycastEye['ID_string'] = DataStructures.RayCastSources.Gaze.name
    raycastEye['ID_number'] = DataStructures.RayCastSources.Gaze.value
    raycastEye.columns = ['time', 'frameNumber', 'x', 'y', 'z', 'ID_string', 'ID_number']

    raycasts = pd.concat([raycastHead, raycastController, raycastEye])
    raycasts = raycasts.reset_index()

    raycasts = RemoveOrigin(observer, data=raycasts)

    raycasts.x = raycasts.x/observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius
    raycasts.y = raycasts.y/observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius
    raycasts.z = raycasts.z/observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius

    raycasts.update(RotateDataFrameZ(raycasts, DataStructures.rotationDictionary[observer.observer.orientation]))

    if (isWriteOutputFile):
        feather.write_feather(raycasts, observer.pathResults + observer.observer.levelStartTime +
                              ".raycasts." + str(optionsNumber) + ".feather")
        print("len(raycasts): %s" % len(raycasts))
    return raycasts


def GetVertices(observer, optionsNumber, isWriteOutFile=True):
    vertices = observer.vertices.copy(deep=True)
    vertices = RemoveOrigin(observer, data=vertices)
    vertices.update(RotateDataFrameZ(vertices, DataStructures.rotationDictionary[observer.observer.orientation]))
    vertices.x = vertices.x / observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius
    vertices.y = vertices.y / observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius
    vertices.z = vertices.z / observer.level['listOptions'][optionsNumber]['radius']  # scale position by radius

    if optionsNumber != -1:
        vertices = vertices[vertices['serializableVertex.optionsNumber'] == optionsNumber]

    if isWriteOutFile:
        feather.write_feather(vertices, observer.pathResults + observer.observer.levelStartTime +
                              ".vertices." + str(optionsNumber) + ".feather")
        print("len(vertices): %s" % len(vertices))
    return vertices


def RemoveOrigin(observer, data):
    data.x = data.x - observer.observer.origin['x']
    data.y = data.y - observer.observer.origin['y']
    data.z = data.z - observer.observer.origin['z']
    return data


def RotateDataFrameZ(df, theta):
    df_rotated = df.loc[:, ['x', 'z', 'y']].to_numpy()
    df_rotated = np.array(df_rotated * ThreeD.rotate_z(theta))
    df_rotated = pd.DataFrame(data=df_rotated, columns=["x", "z", "y"])
    return df_rotated


def CollateLevel(observer=""):
    if observer == "":
        observer = LoadData.PopulateObserver()

    print("CollateLevel(observer)")
    # based on https://towardsdatascience.com/creating-pdf-files-with-python-ad3ccadfae0f

    # A4 measurements
    pdf_w = 210
    pdf_h = 297

    class PDF(FPDF):
        def configure_page(self, gameStartTime, levelDescriptor, imagePath, optionsText, pageNumber):
            self.set_xy(25.4 / 2, 10)
            self.set_font('Arial', 'B', 12)
            self.cell(w=0, h=0, align='L', txt="Game: " + gameStartTime, border=0)

            self.set_xy(0.0, 0.0)
            self.set_font('Arial', '', 12)
            self.cell(w=210.0, h=35, align='C', txt="Level: " + levelDescriptor, border=0)

            self.set_xy(210, 10)
            self.set_font('Arial', '', 12)
            self.cell(w=0, h=0, align='C', txt=str(pageNumber), border=0)

            self.set_xy((25.4 / 2), 20)
            self.image(imagePath, link='', type='', w=210 - 25.4, h=238)

            self.set_xy(25.4 / 2, 260) # 215
            self.set_font('Arial', 'B', 8)
            self.cell(w=0, h=0, align='L', txt="Options", border=0)

            self.set_xy(25.4 / 2, 262)
            self.set_font('Arial', '', 4)
            self.multi_cell(210 - 25.4, 3, align="L", txt=optionsText)

    pdf = PDF(orientation='P', unit='mm', format='A4')

    pageNumber = 1

    if observer.level['isFreeViewing'] == True:  # optionNumber not configured properly so just take all!
        optionsRange = [-1]
    else:
        optionsRange = range(-1, len(observer.level['listOptions']))

    for optionsNumber in optionsRange:
        Common.PrintStars()
        print("optionsNumber: %s" % optionsNumber)

        if optionsNumber == -1:
            options = observer.level['listOptions']

            levelDescriptor = " ("
            for i in range(0, len(observer.level['listOptions'])):
                levelDescriptor = levelDescriptor + observer.level['listOptions'][i]['descriptor']
                if i != len(observer.level['listOptions']) - 1:
                    levelDescriptor = levelDescriptor + ", "
                else:
                    levelDescriptor = levelDescriptor + ")"
            levelDescriptor = observer.level['descriptor'] + levelDescriptor
        else:
            options = observer.level['listOptions'][optionsNumber]

            levelDescriptor = observer.level['descriptor'] + \
                              " (" + observer.level['listOptions'][optionsNumber]['descriptor'] + ")"

        # print(levelDescriptor)

        text = json.dumps(options)
        # print(text)

        filenameSummary = GenerateFilename(observer, "allResults", optionsNumber)

        pdf.add_page()
        pdf.configure_page(gameStartTime=observer.gameStartTime,
                           levelDescriptor=levelDescriptor,
                           imagePath=filenameSummary,
                           optionsText=text,
                           pageNumber=pageNumber)
        pageNumber += 1

    filename = observer.pathResults + observer.levelStartTime + ".new.pdf"
    # print(filename)

    pdf.output(filename, 'F')
    pdf.close()

    if isShowResults:
        print("results displayed!")
        check_output("start " + filename, shell=True)  # view
    else:
        print("results hidden!")


def PrintElapsedTime(startTime):
    elapsedTimeMinutes = (time.time() - startTime) / 60
    print("elapsedTimeMinutes: {0:.2f} minutes".format(elapsedTimeMinutes))
    return elapsedTimeMinutes


def DummyPlot(filename):
    plt.close('all')
    plt.figure()
    plt.savefig(filename)


def AnalyseLevel(levelFolder=""):

    ##
    startTime = time.time()
    observer = LoadData.PopulateObserver(levelFolder=levelFolder)

    if observer.level['isFreeViewing'] == True:  # optionNumber not configured properly so just take all!
        optionsRange = [-1]
    else:
        optionsRange = range(-1, len(observer.level['listOptions']))

    vertices = GetVertices(observer=observer, optionsNumber=-1, isWriteOutFile=False)
    vertices = CartesianToSphericalDataFrame(df=vertices)
    DF = GetBehaviourMapData(observer, vertices)
    Common.Tabulate(DF)

    for optionsNumber in optionsRange:
        print("optionsNumber: %s" % optionsNumber)

        trialData = GetTrialData(observer=observer, optionsNumber=optionsNumber, isWriteOutput=False)
        vertices = GetVertices(observer=observer, optionsNumber=optionsNumber, isWriteOutFile=False)
        vertices = CartesianToSphericalDataFrame(df=vertices)
        raycastData = GetRaycastData(observer=observer, optionsNumber=optionsNumber, isWriteOutputFile=False)
        raycastData = CartesianToSphericalDataFrame(df=raycastData)

        if optionsNumber == -1:
            descriptor = "All"
            mapIndex = DF.optionsNumber >= 0
        else:
            descriptor = observer.level['listOptions'][0]['descriptor']
            mapIndex = DF.optionsNumber == optionsNumber

        filenames = []

        # # ----- all sources plot
        filename = GenerateFilename(observer, "allSources", optionsNumber)
        filenames.append(filename)
        LatitudeLongitudeScatterAllSource(raycastData, "All Sources", filename=filename)

        # ----- element positions plot
        # filename = GenerateFilename(observer, "elementPositions", optionsNumber)
        # filenames.append(filename)
        # PlotElements(lat=vertices.lat,
        #              lon=vertices.lon,
        #              title="Element Positions",
        #              filename=filename,
        #              s=elementSize)

        # ----- behaviour plot

        filename = GenerateFilename(observer, "behaviour", optionsNumber)
        filenames.append(filename)

        if observer.level['isFreeViewing']:
            DummyPlot(filename)
        else:
            PlotBehaviour(trialData, filename)

        # ----- timing plot
        if optionsNumber == -1:
            title = GetLostFrames(observer)
        else:
            title = "total frames: " + str(len(raycastData))
        filename = GenerateFilename(observer, "timingPlot", optionsNumber)
        filenames.append(filename)
        PlotTiming(frameData=observer.frameData, optionsNumber=optionsNumber, title=title, filename=filename)

        # ----- behaviour map plot
        for column in ['N', 'accuracy', 'RT_M']:
            filename = GenerateFilename(observer, column, optionsNumber)
            filenames.append(filename)

            if observer.level['isFreeViewing']:
                DummyPlot(filename)
            else:
                if DF.loc[mapIndex, ["lat", "lon"]].drop_duplicates().__len__() == DF.loc[mapIndex, ["lat", "lon"]].__len__():
                    if DF[mapIndex].__len__() == 0:  # not enough data
                        DummyPlot(filename)
                    else:
                        PlotBehaviourMap(lat=DF.lat[mapIndex],
                                         lon=DF.lon[mapIndex],
                                         z=DF[column][mapIndex],
                                         column=column,
                                         filename=filename)
                else:
                    DF2 = DF.groupby('index').mean()  # average across conditions if vertices are shared
                    PlotBehaviourMap(lat=DF2.lat,
                                     lon=DF2.lon,
                                     z=DF2[column],
                                     column=column,
                                     filename=filename)

        for source in DataStructures.RayCastSources:
            print(source.name)

            # # ----- scatter
            # title = "Scatterplot: " + source.name
            # filename = GenerateFilename(observer, "scatterPlot." + source.name, optionsNumber)
            # filenames.append(filename)
            # LatitudeLongitudeScatterOneSource(lat=raycastData.lat[raycastData.ID_string == source.name],
            #                                   lon=raycastData.lon[raycastData.ID_string == source.name],
            #                                   title=title,
            #                                   filename=filename,
            #                                   s=scatterSize)

            # ----- heatmap
            title = "Heatmap: " + source.name
            filename = GenerateFilename(observer, "heatmap." + source.name, optionsNumber)
            filenames.append(filename)

            H, latEdges, lonEdges = PlotRayCastHeatMap(lat=raycastData.lat[raycastData.ID_string == source.name],
                                                       lon=raycastData.lon[raycastData.ID_string == source.name],
                                                       latEdges=latitudeEdges,
                                                       lonEdges=longitudeEdges,
                                                       cmap='hot',
                                                       title=title,
                                                       filename=filename)

            # ----- symmetry test
            title = "Latitude Test: " + source.name
            filename = GenerateFilename(observer, "latitudeTest." + source.name, optionsNumber)
            filenames.append(filename)
            LatitudePlot(lat=raycastData.lat[raycastData.ID_string == source.name], title=title, filename=filename)

            title = "Longitude Test: " + source.name
            filename = GenerateFilename(observer, "longitudeTest." + source.name, optionsNumber)
            filenames.append(filename)
            LongitudePlot(lon=raycastData.lon[raycastData.ID_string == source.name], title=title, filename=filename)

        plt.close('all')

        # ----- tile results
        filenameSummary = GenerateFilename(observer, "allResults", optionsNumber)
        im = []

        for i in filenames:
            im.append(Image.open(i))

        # im2 = [[im[0], im[1], im[2]], [im[3], im[7], im[11]], [im[4], im[8], im[12]], [im[5], im[9], im[13]], [im[6], im[10], im[14]]]
        im2 = [[im[0], im[1], im[2]],
               [im[3], im[4], im[5]],
               [im[6], im[9], im[12]],
               [im[7], im[10], im[13]],
               [im[8], im[11], im[14]]]
        MergeImages.get_concat_tile_resize(im2).save(filenameSummary)
        # check_output("start " + filenameSummary, shell=True)  # view

        for filename in filenames:
            os.remove(filename)

    CollateLevel(observer)

    # collate game?
    if observer.listLevels.levelStartTime.iloc[-1] == observer.levelStartTime:
        print("final level - collating!")
        CollateGame(observer.gameStartTime)

        if isAnalyseGameAsWhole:
            import SingleSubjectGC
    else:
        print("not yet the final level...")

    elapsedTimeMinutes = PrintElapsedTime(startTime)
    ##
    return observer, elapsedTimeMinutes


def GetLatestGame():
    gameFolder = LoadData.GetSortedFolderList(LoadData.Path.data)[0]
    gameStartTime = str.split(gameFolder, "\\")[-2]
    return gameFolder, gameStartTime


def AnalyseGame(gameStartTime=""):
    startTime = time.time()

    if gameStartTime == "":
        gameFolder, gameStartTime = GetLatestGame()

    levelFolders = LoadData.GetSortedFolderList(LoadData.Path.data + gameStartTime, reverse=False)
    df = pd.DataFrame(columns={'descriptor', 'timeCueSetup', 'timeExperiment'})

    analysisTimes = []
    for levelFolder in levelFolders:
        analysisTimes.append(AnalyseLevel(levelFolder))

    print(df)
    print(df.timeExperiment.sum())

    return df, analysisTimes, PrintElapsedTime(startTime)


def DeleteAllDataAndResults():
    import shutil
    foldersToDelete = LoadData.GetSortedFolderList(LoadData.Path.data) + LoadData.GetSortedFolderList(LoadData.Path.results)
    value = input("Data and results will be deleted, are you sure you know what you are doing? (Y, n): ")

    if value == "Y":
        print("deleting folders...")
        for folder in foldersToDelete:
            print(folder)
            shutil.rmtree(folder)


def DeleteAllResults():
    import shutil
    foldersToDelete = LoadData.GetSortedFolderList(LoadData.Path.results)
    value = input("Results will be deleted, are you sure you know what you are doing? (Y, n): ")

    if value == "Y":
        print("deleting folders...")
        for folder in foldersToDelete:
            print(folder)
            shutil.rmtree(folder)


def CollateGame(gameStartTime="", isExcludeTutorials=False):
    if gameStartTime == "":
        gameFolder, gameStartTime = GetLatestGame()

    game = LoadData.ReadJson(LoadData.Path.data + gameStartTime + "\\" + gameStartTime + ".game.json")
    game = pd.DataFrame(game['listLevels'])

    summaryFiles = []
    merger = PdfFileMerger()

    for i, level in game.iterrows():
        # if not level['isGame'] & isExcludeTutorials:
        #     continue
        filename = LoadData.Path.results + gameStartTime + "\\" + level['levelStartTime'] + "\\" + level['levelStartTime'] + ".new.pdf"
        summaryFiles.append(filename)
        print(summaryFiles[-1])
        print(os.path.exists(summaryFiles[-1]))
        merger.append(summaryFiles[-1])

    filename = LoadData.Path.results + gameStartTime + "\\" + gameStartTime + ".new.pdf"
    merger.write(filename)
    merger.close()

    if isShowResults:
        print("results displayed!")
        check_output("start " + filename, shell=True)  # view
    else:
        print("results hidden!")


##

if __name__ == "__main__":
    AnalyseLevel()
    print('Done :)!')

# AnalyseGame("2021-06-23-11-52-47-2703671")
# AnalyseLevel()
# AnalyseGame(gameStartTime="2021-06-18-10-23-53-6023379")


## notes

# handedness instruction?
# turn off system button
# match retinal extent?
# pupil diamater?

# practice = 0.5
# freeViewing = 1.0
# base = 1.70
# targets = [8, 16, 24, 24, 32, 24]
# times = [practice]
# for target in targets:
#     times.append(target/8*base)
#
# times.append(freeViewing)
# times = np.array(times)
#
# print(times)
# print(times.sum())