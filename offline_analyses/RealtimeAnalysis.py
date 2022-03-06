## clear
globals().clear()

print('loading python modules...')
import sys
print("Python " + sys.version)

import os
import time
import FieldTrip
import BuildControl  # controls working directory for relative paths depending on whether script is run from shell
import importlib
import numpy as np
import pandas as pd
import matplotlib
# ['GTK3Agg', 'GTK3Cairo', 'MacOSX', 'nbAgg', 'Qt4Agg', 'Qt4Cairo', 'Qt5Agg', 'Qt5Cairo', 'TkAgg', 'TkCairo', 'WebAgg', 'WX', 'WXAgg', 'WXCairo', 'agg', 'cairo', 'pdf', 'pgf', 'ps', 'svg', 'template']
if BuildControl.isRunningInPyCharm:
    matplotlib.use('Qt5Agg')
else:
    matplotlib.use('agg')
import matplotlib.pyplot as plt
import seaborn as sns
import keyboard


## project modules
import DataStructures
importlib.reload(DataStructures)

import LoadData
importlib.reload(LoadData)

import Common
importlib.reload(Common)

# import PlotAttention
# importlib.reload(PlotAttention)

import ThreeD
importlib.reload(ThreeD)


## classes

class Buffer:
    def __init__(self, port, host="127.0.0.1", labels=None):
        self.port = port
        self.host = host
        self.labels = labels
        self.lastSample = 0
        self.header = None
        self.data = None
        self.dataFrame = None
        self.ftc = FieldTrip.Client()
        self.GetHeader()

    def GetHeader(self):
        try:
            self.ftc.connect(self.host, self.port)  # might throw IOError
            self.header = self.ftc.getHeader()
        except:
            print("connection failed")
            self.header = None
        return None

    def GetSamples(self, samples):
        try:
            if samples == 'all':
                samples = [0, self.header.nSamples - 1]
            elif samples == 'last':
                samples = [self.header.nSamples - 1, self.header.nSamples - 1]
            elif samples == 'first':
                samples = [0, 0]

            self.ftc.connect(self.host, self.port)  # might throw IOError
            self.header = self.ftc.getHeader()
            self.data = self.ftc.getData(samples)
            self.dataFrame = pd.DataFrame(self.data, columns=self.labels)
            self.lastSample = self.header.nSamples - 1
        except:
            print("connection failed")
            self.data = None
            self.dataFrame = None

    def PrintHeader(self):
        print(self.header)

    def PrintData(self):
        print(self.data.shape)


class BufferPort:
    frameData: int = 1
    trialData: int = 2


labelsTrialData = [
    "trialData.trial",
    "trialData.optionsNumber",
    "trialData.recursionLevel",
    "trialData.radius",

    "trialData.cue.timeOnset",
    "(float) trialData.cue.frameCountOnset",
    "trialData.cue.timeResponse",
    "(float) trialData.cue.frameCountResponse",
    "(float) trialData.cue.RT",
    "GeneralMethods.BoolToFloat(trialData.cue.isCorrect)",

    "trialData.array.timeOnset",
    "(float) trialData.array.frameCountOnset",
    "trialData.array.timeResponse",
    "(float) trialData.array.frameCountResponse",
    "(float) trialData.array.RT",
    "GeneralMethods.BoolToFloat(trialData.array.isCorrect)",

    "(float) trialData.targetPosition",
    "(float) trialData.targetRotation"
]

labelsFrameData = [
    "Time.time",
    "Time.frameCount",

    "eyeTrackingData.Timestamp",

    "GeneralMethods.BoolToFloat(eyeTrackingData.IsLeftEyeBlinking)",
    "GeneralMethods.BoolToFloat(eyeTrackingData.IsRightEyeBlinking)",
    "eyeTrackingData.ConvergenceDistance",
    "GeneralMethods.BoolToFloat(eyeTrackingData.ConvergenceDistanceIsValid)",
    "GeneralMethods.BoolToFloat(eyeTrackingData.GazeRay.IsValid)",

    "eyeTrackingData.GazeRay.Origin.x",
    "eyeTrackingData.GazeRay.Origin.y",
    "eyeTrackingData.GazeRay.Origin.z",

    "eyeTrackingData.GazeRay.Direction.x",
    "eyeTrackingData.GazeRay.Direction.y",
    "eyeTrackingData.GazeRay.Direction.z",

    "eyeGazeSurfacePosition.x",
    "eyeGazeSurfacePosition.y",
    "eyeGazeSurfacePosition.z",

    "ViveInput1.Transforms.headset.transform.position.x",
    "ViveInput1.Transforms.headset.transform.position.y",
    "ViveInput1.Transforms.headset.transform.position.z",

    "ViveInput1.Transforms.headset.transform.rotation.w",
    "ViveInput1.Transforms.headset.transform.rotation.x",
    "ViveInput1.Transforms.headset.transform.rotation.y",
    "ViveInput1.Transforms.headset.transform.rotation.z",

    "ViveInput1.Transforms.headset.transform.forward.x",
    "ViveInput1.Transforms.headset.transform.forward.y",
    "ViveInput1.Transforms.headset.transform.forward.z",

    "headsetSurfacePosition.x",
    "headsetSurfacePosition.y",
    "headsetSurfacePosition.z",

    "ViveInput1.Transforms.controllers[0].transform.position.x",
    "ViveInput1.Transforms.controllers[0].transform.position.y",
    "ViveInput1.Transforms.controllers[0].transform.position.z",

    "ViveInput1.Transforms.controllers[0].transform.rotation.w",
    "ViveInput1.Transforms.controllers[0].transform.rotation.x",
    "ViveInput1.Transforms.controllers[0].transform.rotation.y",
    "ViveInput1.Transforms.controllers[0].transform.rotation.z",

    "ViveInput1.Transforms.controllers[0].transform.forward.x",
    "ViveInput1.Transforms.controllers[0].transform.forward.y",
    "ViveInput1.Transforms.controllers[0].transform.forward.z",

    "controller0SurfacePosition.x",
    "controller0SurfacePosition.y",
    "controller0SurfacePosition.z",

    "ViveInput1.Transforms.controllers[1].transform.position.x",
    "ViveInput1.Transforms.controllers[1].transform.position.y",
    "ViveInput1.Transforms.controllers[1].transform.position.z",

    "ViveInput1.Transforms.controllers[1].transform.rotation.w",
    "ViveInput1.Transforms.controllers[1].transform.rotation.x",
    "ViveInput1.Transforms.controllers[1].transform.rotation.y",
    "ViveInput1.Transforms.controllers[1].transform.rotation.z",

    "ViveInput1.Transforms.controllers[1].transform.forward.x",
    "ViveInput1.Transforms.controllers[1].transform.forward.y",
    "ViveInput1.Transforms.controllers[1].transform.forward.z",

    "controller1SurfacePosition.x",
    "controller1SurfacePosition.y",
    "controller1SurfacePosition.z",

    "GameRunner.trialData.trial",
    "(float)GameRunner.currentTrialData.optionsNumber",
    "(float)GameRunner.currentTrialData.options.recursionLevel",
    "GameRunner.trialData.radius",

    "(float) GameRunner.trialData.state",

    "(float) GameRunner.trialData.origin.x",
    "(float) GameRunner.trialData.origin.y",
    "(float) GameRunner.trialData.origin.z",

    "(float) GameRunner.trialData.yRotation",

    "(float) PointerGlobal.Decision.stopwatch.Elapsed.TotalMilliseconds",

    "GenericFunctions.BoolToFloat(InputManager.headset.isTracked)",
    "GenericFunctions.BoolToFloat(InputManager.headset.isProximitySensorActivated)",
    "GenericFunctions.BoolToFloat(InputManager.controllers[0].essentialTransform.isTracked)",
    "GenericFunctions.BoolToFloat(InputManager.controllers[1].essentialTransform.isTracked)",
    "(float) PointerGlobal.pointerToUse",
]


def GetBufferHeaders(host='127.0.0.1'):
    import FieldTrip
    for port in [1, 2]:
        ftc = FieldTrip.Client()
        ftc.connect(host, port)
        header = ftc.getHeader()
        print("port: {}, hdr.nSamples: {}, hdr.nChannels: {}".format(port, header.nSamples, header.nChannels))




## initialise buffers

bufferTrialData = Buffer(port=BufferPort.trialData, labels=labelsTrialData)
bufferFrameData = Buffer(port=BufferPort.frameData, labels=labelsFrameData)

bufferFrameData.GetHeader()
bufferFrameData.GetSamples("first")
firstFrame = int(bufferFrameData.dataFrame['Time.frameCount'].iloc[0])

while True:
    time.sleep(1)
    if keyboard.is_pressed('q'):
        print('You pressed q!')
        break

    bufferFrameData.GetHeader()
    bufferFrameData.GetSamples("last")

    totalFrames = int(bufferFrameData.dataFrame['Time.frameCount'].iloc[0]) - firstFrame + 1
    totalFramesRecorded = bufferFrameData.header.nSamples
    lostFrames = totalFrames - totalFramesRecorded
    lostProportion = lostFrames/totalFrames
    print("totalFrames:{}, totalFramesRecorded:{}, lostFrames:{}, lostProportion:{:2f}".format(totalFrames, totalFramesRecorded, lostFrames, lostProportion))


Common.Stop()





## get preliminary data
gameFolder = LoadData.GetSortedFolderList(LoadData.Path.data)[0]
gameStartTime = str.split(gameFolder, "\\")[-2]
levelFolder = LoadData.GetSortedFolderList(gameFolder)[0]
levelStartTime = str.split(levelFolder, "\\")[-2]
game = LoadData.ReadJson(gameFolder + gameStartTime + ".game.json")
listLevels = pd.DataFrame(game['listLevels'])
numberOfLevels = len(listLevels)


# get level data
while len(os.listdir(levelFolder)) != 10:
    time.sleep(1)


observer = LoadData.PopulateObserver(levelFolder, isPopulatePerformanceData=False)

observer.level['descriptor']
currentLevelNumber = np.where(observer.listLevels.levelStartTime == observer.levelStartTime)[0][0]

totalNumberOfTrials = 0

for optionsNumber in range(0, len(observer.level['listOptions'])):
    print("optionsNumber:{}".format(optionsNumber))
    numberOfVertices = (observer.vertices['serializableVertex.optionsNumber'] == optionsNumber).sum()
    numberOfRepetitions = observer.level['listOptions'][optionsNumber]['numberOfRepetitions']
    totalNumberOfTrials += numberOfVertices * numberOfRepetitions


levelDictionary = {'descriptor': observer.level['descriptor'],
                   'currentLevelNumber': currentLevelNumber,
                   'numberOfLevels': numberOfLevels}


## initialise buffer

def GetMinimalFrameData():
    bufferFrameData.GetHeader()

    if bufferFrameData.header is None:
        return None, None, None

    if bufferFrameData.header.nSamples > 0:
        bufferFrameData.GetSamples('last')
        observer.frameData = bufferFrameData.dataFrame

        frameInfo = bufferFrameData.dataFrame.loc[:, ["Time.time",
                                                      "GameRunner.trialData.trial",
                                                      "(float)GameRunner.currentTrialData.optionsNumber",
                                                      "(float) GameRunner.trialData.state"]]
        frameInfo = frameInfo.rename(columns={"Time.time": "time",
                                              "GameRunner.trialData.trial": "trial",
                                              "(float)GameRunner.currentTrialData.optionsNumber": "options",
                                              "(float) GameRunner.trialData.state": "state"})

        for column in ['time', 'trial', 'options', 'state']:
            frameInfo[column] = frameInfo[column].astype(int)

        frameInfo = frameInfo.iloc[0].to_dict()
        # print("frameInfo: {}".format(frameInfo))

        vertices = PlotAttention.GetVertices(observer=observer, optionsNumber=int(frameInfo['options']),
                                             isWriteOutFile=False)
        raycasts = PlotAttention.GetRaycastData(observer=observer, optionsNumber=int(frameInfo['options']),
                                                isLimitToArray=False, isWriteOutputFile=False)
        return frameInfo, vertices, raycasts
    else:
        return None, None, None


def GetMinimalTrialData():
    bufferTrialData.GetHeader()

    if bufferTrialData.header.nSamples == bufferTrialData.lastSample:
        return None

    if bufferTrialData.header is None:
        return None

    if bufferTrialData.header.nSamples > 0:
        bufferTrialData.GetSamples('last')

        trialData = bufferTrialData.dataFrame
        trialInfo = trialData.loc[:, ["trialData.trial",
                                      "trialData.optionsNumber",
                                      "(float) trialData.cue.RT",
                                      "GeneralMethods.BoolToFloat(trialData.cue.isCorrect)",
                                      "(float) trialData.targetPosition"]]

        trialInfo = trialInfo.rename(columns={"trialData.trial": "trial",
                                              "trialData.optionsNumber": "options",
                                              "(float) trialData.cue.RT": "RT",
                                              "GeneralMethods.BoolToFloat(trialData.cue.isCorrect)": "accuracy",
                                              "(float) trialData.targetPosition": "target"})

        for column in ['trial', 'RT', 'options', 'accuracy', 'target']:
            trialInfo[column] = trialInfo[column].astype(int)

        trialInfo = trialInfo.iloc[0].to_dict()
        print("trialInfo: {}".format(trialInfo))
        return trialInfo
    else:
        return None


def ProcessFrameData():
    Common.PrintStars()
    frameInfo, vertices, raycasts = GetMinimalFrameData()
    trialInfo = GetMinimalTrialData()






## real-time loop



while True:
    time.sleep(1)
    if keyboard.is_pressed('q'):
        print('You pressed q!')
        break

    ProcessFrameData()


    len(os.listdir(levelFolder)) == 10







listLevels = pd.DataFrame(game['listLevels'])
numberOfLevels = len(listLevels)


# get level data
len(os.listdir(levelFolder)) == 10


observer = LoadData.PopulateObserver(levelFolder, isPopulatePerformanceData=False)

observer.level['descriptor']
currentLevelNumber = np.where(observer.listLevels.levelStartTime == observer.levelStartTime)[0][0]

totalNumberOfTrials = 0


