from subprocess import check_output
import os
import pandas as pd
import numpy as np
import importlib
import seaborn as sns
from matplotlib import pyplot as plt
import time
from PIL import Image, ImageFont, ImageDraw

import LoadData
importlib.reload(LoadData)
import Common
importlib.reload(Common)
from Common import Tabulate, Stop

import DataStructures
importlib.reload(DataStructures)
from MergeImages import get_concat_tile_resize

import PlotAttentionNew
startTime = time.time()

## methods

# concatenate subject data over levels
def ConcatenateData(dataLogToUse):
    TrialData = []
    RaycastData = []

    for i, row in dataLogToUse.iterrows():
        print(i)
        trialData = results.trialData[i]
        raycastData = results.raycastData[i]

        for variable in ['levelStartTime', 'descriptor', 'Half', 'setSize']:
            trialData[variable] = row[variable]
            raycastData[variable] = row[variable]

        TrialData.append(trialData)
        RaycastData.append(raycastData)

    TrialData = pd.concat(TrialData)
    TrialData = TrialData.reset_index()

    TrialData = TrialData.rename(columns={"index": "trialNumber"})
    TrialData["ordinalTrialNumber"] = TrialData.index
    TrialData['rolling_std'] = TrialData[RT_variable].rolling(window=20).std()
    # Tabulate(TrialData)

    RaycastData = pd.concat(RaycastData)
    RaycastData['descriptorInt'] = RaycastData.descriptor.replace(descriptorDictionary)

    # Tabulate(RaycastData.head())
    return TrialData, RaycastData


# behaviour time course
def PlotBehaviour(trialData, trialNumberVariable='trialData.trial', isShowScatter=True, linewidth=1.0, filename="PlotBehaviour.png"):
    plt.close('all')
    figure, axes = plt.subplots(3, 1)

    # 0
    axes[0].plot(trialData[trialNumberVariable], trialData[accuracyVariable],
                 color='k', linewidth=linewidth)

    if isShowScatter:
        axes[0].scatter(trialData[trialNumberVariable], trialData[accuracyVariable],
                        color='k', s=20)

    axes[0].set_ylabel('Accuracy', fontdict={'fontsize': labelFontSize})
    axes[0].set_ylim(-.1, 1.1)
    axes[0].set_yticks(ticks=[0, 1])
    axes[0].set_xticklabels([])
    sns.despine(ax=axes[0])
    axes[0].set_title("Accuracy = ({:.2f}%)".format(
        np.nanmean(trialData[accuracyVariable] * 100)), fontdict={'fontsize': labelFontSize})

    # 1
    axes[1].plot(trialData[trialNumberVariable], trialData[RT_variable] / 1000, color='k', linewidth=linewidth)

    if isShowScatter:
        axes[1].scatter(trialData[trialNumberVariable], trialData[RT_variable] / 1000, color='k', s=20)

    axes[1].set_ylabel('RT (s)', fontdict={'fontsize': labelFontSize})
    sns.despine(ax=axes[1])
    M = (trialData[RT_variable]/1000).mean(skipna=True)
    SD = (trialData[RT_variable]/1000).std(skipna=True)
    axes[1].set_title("RT M = ({:.2f}), SD = ({:.2f})".format(M, SD), fontdict={'fontsize': labelFontSize})
    axes[1].set_xticklabels([])

    # 2
    axes[2].plot(trialData[trialNumberVariable], trialData['rolling_std'] / 1000, color='k', linewidth=linewidth)
    axes[2].set_xlabel('Trial Number', fontdict={'fontsize': labelFontSize})
    sns.despine(ax=axes[2])
    axes[2].set_ylabel('RTSD (s)', fontdict={'fontsize': labelFontSize})

    for axis in axes[1:]:
        axis.yaxis.set_major_formatter("{x:.2f}")

    for axis in axes:
        plt.sca(axis)
        yLimit = plt.ylim()

        for x in trialData.ordinalTrialNumber[trialData.trialNumber == 0]:
            plt.vlines(x=x, ymin=yLimit[0], ymax=yLimit[1], colors='r', linewidth=linewidth)

        plt.ylim(yLimit)

    M = (trialData['rolling_std']/1000).mean(skipna=True)
    SD = (trialData['rolling_std']/1000).std(skipna=True)
    axes[2].set_title("RTSD M = ({:.2f}), SD = ({:.2f})".format(M, SD), fontdict={'fontsize': labelFontSize})

    figure.align_ylabels(axes)
    plt.tight_layout()

    plt.sca(axes[0])
    setSizes = trialData[trialData.trialNumber == 0][["ordinalTrialNumber", "setSize"]]
    setSizes = setSizes.reset_index()
    y = np.linspace(.8, .2, len(setSizes))
    for i, row in setSizes.iterrows():
        plt.text(row.ordinalTrialNumber, y[i], row.setSize, verticalalignment="center")

    plt.suptitle("Behaviour Time Course")
    plt.tight_layout()

    if filename != "":
        plt.savefig(fname=outputDirectory + filename, dpi=dpi)


# set size effect
def polyfit(x, y, degree, nEstimatedPoints=10):
    # polynomial Regression
    # https://stackoverflow.com/questions/893657/how-do-i-calculate-r-squared-using-python-and-numpy
    results = {}

    # fit polynomial
    fitCoefficients = np.polyfit(x=x, y=y, deg=degree)
    model = np.poly1d(fitCoefficients)
    print(str.format("model: {}", model))

    # calculate r-squared
    yHat = model(x)
    yBar = np.sum(y)/len(y)
    SSreg = np.sum((yHat-yBar)**2)
    SStotal = np.sum((y-yBar)**2)
    rSquared = SSreg / SStotal
    print(str.format("rSquared: {}", rSquared))

    # calculate p-value (to do?)
    # https://stackoverflow.com/questions/67608520/how-can-i-get-t-values-and-p-values-for-regression-coefficients-in-numpy-polyfit
    # np.polynomial.polynomial.polyvander2d(x=x, y=y, deg=(2, 2))
    # statsmodels

    xFit = np.linspace(min(x), max(x), nEstimatedPoints)  # overkill for linear (requires two points) but useful for higher-order polynomials

    if degree == 1:  # explict
        a = fitCoefficients[0]  # slope
        b = fitCoefficients[1]  # intercept
        yFit = (a * xFit) + b
        modelString = "%.4f" % a + "x + " "%.4f" % b
    elif degree == 2: # explict
        a = fitCoefficients[0]
        b = fitCoefficients[1]
        c = fitCoefficients[2]
        yFit = (a * np.square(xFit)) + (b * xFit) + c
        modelString = "%.4f" % a + "x^2 + " + "%.4f" % b + "x + " + "%.4f" % c
    else:  # magic taken care of - also works for degrees of 1 and 2
        yFit = model(xFit)
        modelString = "?"  # to do?

    results['fitCoefficients'] = fitCoefficients
    results['model'] = model
    results['rSquared'] = rSquared
    results['xFit'] = xFit
    results['yFit'] = yFit
    results['x'] = x
    results['y'] = y
    results['modelString'] = modelString
    return results


def PlotSetSize(isRemoveOutliers=False, filename="PlotSetSize.png"):
    # remove outliers
    df = TrialData.copy(deep=True)
    df[RT_variable] = df[RT_variable]/1000

    if isRemoveOutliers:
        g = df.groupby(['setSize'])
        df1 = g.transform('quantile', 0.025)
        df2 = g.transform('quantile', 0.975)

        c = df.columns.difference(['setSize'])
        mask = df[c].lt(df1) | df[c].gt(df2)
        # mask = df[c].gt(df2)
        df[c] = df[c].mask(mask)
        Tabulate(df[['setSize', RT_variable]])

    M = df.groupby("setSize").mean()
    M = M.reset_index()
    # Tabulate(M[["setSize", RT_variable]])

    SD = df.groupby("setSize").std()
    SD = SD.reset_index()
    # Tabulate(SD[["setSize", RT_variable]])

    for setSize in df.setSize.unique():
        std = df[df.setSize == setSize][RT_variable].mean()
        mean = df[df.setSize == setSize][RT_variable].std()
        print(str.format("{}, {}, {}", setSize, std, mean))

    figure, axes = plt.subplots(4, 1, sharex=True)

    # 0
    plt.sca(axes[0])
    plt.bar(x=M.setSize+2, height=M[accuracyVariable]*100, facecolor=[.75, .75, .75], edgecolor='k')
    plt.ylim(0, 100)
    axes[0].set_xticks(ticks=M.setSize + 2)
    plt.ylabel("Acc. (%)", fontdict={'fontsize': labelFontSize})

    for i, setSize in enumerate(M.setSize):
        text = "{:.2f}".format(M.iloc[i][accuracyVariable]*100) + "%"
        plt.text(setSize+2, max(plt.ylim())*1.1, text, horizontalalignment='center')

    # 1
    plt.sca(axes[1])
    sns.violinplot(x="setSize", y=RT_variable, data=df, order=range(-2, 28), width=6, color=[.75, .75, .75])
    axes[1].set_ylabel("RT (s)", fontdict={'fontsize': labelFontSize})
    axes[1].set_xlabel("")

    # 2
    plt.sca(axes[2])
    resultsD1 = polyfit(x=M.setSize+2, y=M[RT_variable], degree=1)
    resultsD2 = polyfit(x=M.setSize+2, y=M[RT_variable], degree=2)

    for results in [resultsD1, resultsD2]:
        print(results['fitCoefficients'])
        print(results['model'])
        print(results['modelString'])

    plt.plot(resultsD1['xFit'], resultsD1['yFit'], color='r', label=resultsD1['modelString'], linewidth=.75)
    plt.plot(resultsD2['xFit'], resultsD2['yFit'], color='b', label=resultsD2['modelString'], linewidth=.75)

    plt.scatter(x=M.setSize+2, y=M[RT_variable], color='k', s=25, label="Mean")
    plt.errorbar(x=M.setSize+2, y=M[RT_variable], yerr=SD[RT_variable], fmt="o", c='black')
    plt.legend(frameon=False, loc="upper left", fontsize=8)

    axes[2].set_xticks(ticks=M.setSize+2)
    axes[2].set_ylabel("RT (s)", fontdict={'fontsize': labelFontSize})

    for i, setSize in enumerate(M.setSize):
        # text = "M = {:.2f},\n SD = {:.2f}".format(M.iloc[i][RT_variable], SD.iloc[i][RT_variable])
        text = "M = {:.2f}".format(M.iloc[i][RT_variable])
        plt.text(setSize+2, max(plt.ylim())*1.1, text, horizontalalignment='center')

    for axis in axes:
        axis.yaxis.set_major_formatter("{x:.2f}")
        sns.despine()

    # 3
    plt.sca(axes[3])
    plt.bar(x=SD.setSize+2, height=SD[RT_variable], facecolor=[.75, .75, .75], edgecolor='k')
    # plt.ylim(0, 100)
    axes[0].set_xticks(ticks=SD.setSize + 2)
    plt.ylabel("RTSD (s)", fontdict={'fontsize': labelFontSize})

    for i, setSize in enumerate(SD.setSize):
        text = "{:.2f}".format(SD.iloc[i][RT_variable])
        plt.text(setSize+2, max(plt.ylim())*1.1, text, horizontalalignment='center')

    axes[3].set_xlabel("Set Size", fontdict={'fontsize': labelFontSize})

    # general
    plt.suptitle("Set Size Effect")
    plt.tight_layout()
    figure.align_ylabels(axes)

    if filename != "":
        plt.savefig(fname=outputDirectory + filename, dpi=dpi)

    return M


# raycast maps
def PlotAttentionMaps(filename="PlotAttentionMaps.png"):
    print("PlotAttentionMaps")
    figure, axes = plt.subplots(len(DataStructures.RayCastSources), len(setSizes))
    figure.set_size_inches(12.8, 12.8*(6/10))

    inv_map = {v: k for k, v in setSizeDictionary.items()}

    for setSizeIndex, setSize in enumerate(setSizes):
        raycastData = RaycastData[(RaycastData.setSize == setSize)]
        for sourceIndex, source in enumerate(DataStructures.RayCastSources):
            title = str.format("SS: {}, {} ", setSize, source.name)
            plt.sca(axes[sourceIndex, setSizeIndex])

            # ----- heatmap
            # filename = GenerateFilename(observer, "heatmap." + source.name, optionsNumber)
            # filenames.append(filename)

            H, latEdges, lonEdges = PlotRayCastHeatMap(lat=raycastData.lat[raycastData.ID_string == source.name],
                                                       lon=raycastData.lon[raycastData.ID_string == source.name],
                                                       latEdges=latitudeEdges,
                                                       lonEdges=longitudeEdges,
                                                       cmap='hot',
                                                       title=title,
                                                       filename="",
                                                       isCreateFigure=False,
                                                       axis=plt.gca(),
                                                       labelFontSize=10,
                                                       isDrawCrossHairs=True)

    plt.suptitle("Attention Maps")
    plt.tight_layout()

    if filename != "":
        plt.savefig(fname=outputDirectory + filename, dpi=dpi)


# raycast heatmap
def PlotRayCastHeatMap(lat, lon, latEdges, lonEdges, cmap, title="", vertices=None, filename="", gridOn=False, isDrawCrossHairs=False, isCreateFigure=True, axis=None, labelFontSize=10):
    # plt.close('all')
    H, latEdges, lonEdges = np.histogram2d(x=lon, y=lat, bins=(latEdges, lonEdges))  # reversed?
    H = H / H.sum() * 100  # convert to percentage
    H = H.astype(np.float32)

    vmin = 0
    vmax = np.percentile(H, 99)

    if vmax == 0:
        vmax = np.max(H)

    print("vmin:{}, vmax:{}".format(vmin, vmax))

    if isCreateFigure:
        figure, axis = plt.subplots(1, 1)

    im = plt.imshow(H, interpolation='none',
                    origin='lower',
                    extent=[latEdges[0], latEdges[-1], lonEdges[0], lonEdges[-1]],
                    cmap=cmap,
                    vmin=vmin,
                    vmax=vmax)

    if title != "":
        plt.title(title, fontdict={'fontsize': labelFontSize})

    axis.set_xlabel("Latitude (°)", fontdict={'fontsize': labelFontSize})
    axis.set_ylabel("Longitude (°)", fontdict={'fontsize': labelFontSize})

    colorBar = plt.colorbar(im, fraction=0.046, pad=0.04)
    colorBar.ax.set_title('%', fontdict={'fontsize': labelFontSize})

    colorBar.ax.yaxis.set_major_formatter("{x:.2f}")

    colorBar.set_ticks([vmin, vmax])
    colorBar.set_ticklabels(["{:.2f}".format(vmin), "{:.2f}".format(vmax)])

    if gridOn:
        plt.grid(color=[1, 1, 1], linewidth=2)

    if isDrawCrossHairs:
        DrawCrossHairs('b')

    if vertices is not None:
        plt.scatter(vertices.lat, vertices.lon, color="b")

    plt.gca().set_aspect('equal', adjustable='box')
    # plt.tight_layout()

    if filename != "":
        plt.savefig(fname=filename, dpi=dpi)

    return H, latEdges, lonEdges


def DrawCrossHairs(color):
    plt.axhline(linewidth=1, color=color)
    plt.axvline(linewidth=1, color=color)


# distribution of attention and means
def DrawComparisonLine(isHorizontal=True):
    if isHorizontal:
        # horizontal align
        xLimit = plt.xlim()
        plt.hlines(y=0, xmin=xLimit[0], xmax=xLimit[1], color='k')
        plt.xlim(xLimit)
    else:
        # vertical line
        yLimit = plt.ylim()
        plt.vlines(x=0, xmin=yLimit[0], xmax=yLimit[1], color='k')
        plt.ylim(yLimit)


def SetSymmetricalLimit(isY=True):
    if isY:
        # symmetrical x
        yLimit = np.array(plt.ylim())
        plt.ylim(-max(abs(yLimit)), +max(abs(yLimit)))
    else:
        # symmetrical y
        xLimit = np.array(plt.xlim())
        plt.xlim(-max(abs(xLimit)), +max(abs(xLimit)))


def DistributionPlot(isLongitude=True, filename=""):
    str.format("DistributionPlot: isLongitude: {}", isLongitude)

    if isLongitude:
        figure, axes = plt.subplots(2, 1)
        x = "descriptorInt"
        y = "lon"
        xlabel = "Set Size"
        ylabel = "Longitude (°)"
        suptitle = "Longitude Distrubition"
        isHorizontal = True
        isY = True
    else:
        figure, axes = plt.subplots(2, 1)
        y = "lat"
        x = "descriptorInt"
        ylabel = "Latitude (°)"
        xlabel = "Set Size"
        suptitle = "Latitude Distribution"
        isHorizontal = True
        isY = True

    # 0
    plt.sca(axes[0])
    sns.violinplot(x=x, y=y, hue="ID_string", data=RaycastData, width=.75)
    plt.xticks(ticks=list(descriptorDictionary.values()), labels=[])
    plt.xlabel("")
    axes[0].yaxis.set_major_formatter("{x:.0f}")

    # 1
    plt.sca(axes[1])
    sns.pointplot(x=x, y=y, hue="ID_string", data=RaycastData, join=False, dodge=True, scale=.75, errwidth=1)
    plt.xticks(ticks=list(descriptorDictionary.values()), labels=list(setSizeDictionary.values()))
    plt.xlabel(xlabel, fontdict={'fontsize': labelFontSize})
    axes[0].yaxis.set_major_formatter("{x:.1f}")

    # general

    for axis in axes:
        plt.sca(axis)
        plt.ylabel(ylabel, fontdict={'fontsize': labelFontSize})

        DrawComparisonLine(isHorizontal=isHorizontal)
        SetSymmetricalLimit(isY=isY)
        plt.legend(frameon=False, loc="best", fontsize=8)
        sns.despine()

    plt.suptitle(suptitle, fontdict={'fontsize': labelFontSize})
    figure.align_ylabels(axes)
    plt.tight_layout()

    if filename != "":
        plt.savefig(fname=outputDirectory + filename, dpi=dpi)


def CollateSingleSubject(observerName=None):
    if observerName is None:
        filenames = ["Behaviour.png",
                     "SetSize.png",
                     "AttentionMaps.png",
                     "DistributionLatitude.png",
                     "DistributionLongitude.png"]
        collatedFilename = "GameSummary.png",
    else:
        filenames = [observerName + ".Behaviour.png",
                     observerName + ".SetSize.png",
                     observerName + ".AttentionMaps.png",
                     observerName + ".DistributionLatitude.png",
                     observerName + ".DistributionLongitude.png"]
        collatedFilename = observerName + ".GameSummary.png"

    imageList = []
    for filename in filenames:
        imageList.append(Image.open(outputDirectory + filename))

    tiledImageList = [[imageList[0], imageList[1]], [imageList[2]], [imageList[3], imageList[4]]]
    get_concat_tile_resize(tiledImageList).save(outputDirectory + collatedFilename)

    # add observer identifier
    collatedImage = Image.open(outputDirectory + collatedFilename)
    draw = ImageDraw.Draw(collatedImage)
    font = ImageFont.truetype(font="OpenDyslexicMono-Regular.otf", size=120)
    draw.text((2800, 0), observerName, (0, 0, 0), font)

    draw = ImageDraw.Draw(collatedImage)
    draw.line([(0, 0), (0, 20000)], fill="black", width=20)
    # collatedImage.show()
    collatedImage.save(outputDirectory + collatedFilename)
    collatedImage.close()

    return collatedFilename


def CollateGroup(filenames, collatedFilename="group.CollatedMaps.png"):
    imageList = []
    for filename in filenames:
        imageList.append(Image.open(outputDirectory + filename))

    tiledImageList = [imageList]
    get_concat_tile_resize(tiledImageList).save(outputDirectory + collatedFilename)


## setup

class Results:
    def __init__(self, dataLog=[], raycastData=[], trialData=[], observer=[], vertices=[]):
        self.dataLog = pd.DataFrame(columns=["ID", "gameStartTime", "levelStartTime", "descriptor"])
        self.raycastData = raycastData
        self.trialData = trialData
        self.observer = observer
        self.vertices = vertices


def AnalyseGame(gameStartTime=""):
    def GetLatestGame():
        gameFolder = LoadData.GetSortedFolderList(LoadData.Path.data)[0]
        gameStartTime = str.split(gameFolder, "\\")[-2]
        return gameFolder, gameStartTime

    if gameStartTime == "":
        gameFolder, gameStartTime = GetLatestGame()

    levelFolders = LoadData.GetSortedFolderList(LoadData.Path.data + gameStartTime, reverse=False)

    results = Results()

    for levelFolder in levelFolders:
        observer = LoadData.PopulateObserver(levelFolder=levelFolder)

        optionsNumber = -1
        trialData = PlotAttentionNew.GetTrialData(observer=observer, optionsNumber=optionsNumber, isWriteOutput=False)
        vertices = PlotAttentionNew.GetVertices(observer=observer, optionsNumber=optionsNumber, isWriteOutFile=False)
        vertices = PlotAttentionNew.CartesianToSphericalDataFrame(df=vertices)
        raycastData = PlotAttentionNew.GetRaycastData(observer=observer, optionsNumber=optionsNumber, isWriteOutputFile=False)
        raycastData = PlotAttentionNew.CartesianToSphericalDataFrame(df=raycastData)

        results.raycastData.append(raycastData)
        results.trialData.append(trialData)
        results.observer.append(observer)
        results.vertices.append(vertices)

        dataLog = {"ID": observer.observer.ID,
                   "gameStartTime": gameStartTime,
                   "levelStartTime": levelFolder,
                   "descriptor": observer.level['descriptor'],
                   "nTrials": len(trialData)}

        results.dataLog = results.dataLog.append(dataLog, ignore_index=True)
    return results, gameStartTime


## structures
RT_variable = '(float) trialData.array.RT'
accuracyVariable = 'GeneralMethods.BoolToFloat(trialData.array.isCorrect)'

setSizeDictionary = {"Pair": 2,
                     "One Ring": 8,
                     "Two Rings": 16,
                     "Three Rings": 24}

descriptorDictionary = {"Pair": 0,
                        "One Ring": 1,
                        "Two Rings": 2,
                        "Three Rings": 3}

setSizes = setSizeDictionary.values()

## get data
startTimeSubject = time.time()

# gameStartTime = "2021-11-10-10-12-47-2412658"
# results = AnalyseGame(gameStartTime)
results, gameStartTime = AnalyseGame()
outputDirectory = LoadData.Path.results + gameStartTime + "\\"

## decorate dataLog
dataLog = results.dataLog

dataLog['Half'] = [1, 1, 1, 1, 2, 2, 2, 2]*dataLog.ID.unique().__len__()
dataLog['setSize'] = dataLog.descriptor.replace(setSizeDictionary)

dataLog.nTrials = dataLog.nTrials.astype(int)
dataLog['nTrialsPerSetSize'] = dataLog.nTrials/dataLog['setSize']
Tabulate(dataLog)

# plot settings
dpi = 600
labelFontSize = 12
plotLimit = 50
histogramSpacing = 1
latitudeEdges = np.arange(-plotLimit, plotLimit + 1, histogramSpacing)
longitudeEdges = np.arange(-plotLimit, plotLimit + 1, histogramSpacing)

## analyse

ID = results.dataLog.ID.unique()[0]
dataLogToUse = dataLog
observerName = dataLogToUse.gameStartTime.unique()[0]

Common.PrintStars()
print(str.format("ID: {}", ID))

index = (dataLog.ID == ID)

# Tabulate(dataLogToUse)

TrialData, RaycastData = ConcatenateData(dataLogToUse=dataLogToUse)

plt.close('all')
PlotBehaviour(trialData=TrialData,
              trialNumberVariable="ordinalTrialNumber",
              isShowScatter=False,
              linewidth=0.5,
              filename=observerName + ".Behaviour.png")
PlotSetSize(isRemoveOutliers=False, filename=observerName + ".SetSize.png")
PlotAttentionMaps(filename=observerName + ".AttentionMaps.png")
DistributionPlot(isLongitude=True, filename=observerName + ".DistributionLongitude.png")
DistributionPlot(isLongitude=False, filename=observerName + ".DistributionLatitude.png")
collatedFilename = CollateSingleSubject(observerName=observerName)

stopTime = time.time()
print(str.format("stopTime-startTimeSubject: {}", stopTime-startTimeSubject))

## remove unnecessary files
filenames = [observerName + ".Behaviour.png",
             observerName + ".SetSize.png",
             observerName + ".AttentionMaps.png",
             observerName + ".DistributionLatitude.png",
             observerName + ".DistributionLongitude.png"]

for filename in filenames:
    os.remove(outputDirectory + filename)

check_output("start " + outputDirectory + collatedFilename, shell=True)  # view
print(gameStartTime + "... done single-subject game analysis :)!")
