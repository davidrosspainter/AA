import numpy as np
import pandas as pd
import json
import os
import glob
from dataclasses import dataclass


## classes

@dataclass
class Observer:
    def __init__(self,
                 gameFolder,
                 gameStartTime,
                 game,
                 listLevels,
                 levelFolder,
                 levelStartTime,
                 level,
                 observer,
                 vertices,
                 verticesJson,
                 trialData,
                 trialDataJson,
                 frameData,
                 pathResults,
                 filePath,
                 timeCueSetup,
                 timeExperiment,
                 criticalTrialData):

        self.gameFolder = gameFolder
        self.gameStartTime = gameStartTime
        self.game = game
        self.listLevels = listLevels
        self.levelFolder = levelFolder
        self.levelStartTime = levelStartTime
        self.level = level
        self.observer = observer
        self.vertices = vertices
        self.verticesJson = verticesJson
        self.trialData = trialData
        self.trialDataJson = trialDataJson
        self.frameData = frameData
        self.pathResults = pathResults
        self.filePath = filePath
        self.timeCueSetup = timeCueSetup
        self.timeExperiment = timeExperiment
        self.criticalTrialData = criticalTrialData


class Dict2Obj(object):
    """
    Turns a dictionary into a class
    """
    def __init__(self, dictionary):
        """Constructor"""
        for key in dictionary:
            setattr(self, key, dictionary[key])

    def __repr__(self):
        """"""
        return "<dict2obj: %s="">" % self.__dict__


@dataclass
class Path:
    data: str = "..\\_DATA\\"
    results: str = "..\\_RESULTS\\"
    analysis = os.getcwd() + "\\"
    R = os.path.abspath('..\\external_dependencies\\R-4.0.3\\bin\\Rscript.exe')


## methods

def WriteObserverFile(custom_observer):
    text_file = open('custom_observer.txt', "w")
    text_file.write(custom_observer)
    text_file.close()


def ReadObserverFile():
    with open('custom_observer.txt', "r") as myfile:
        custom_observer = myfile.readlines()

    if len(custom_observer) == 0:
        custom_observer = ""
    else:
        custom_observer = custom_observer[0]
    return custom_observer


def SetOutputDirectory(output_directory):
    if not os.path.isdir(output_directory):
        print('creating directory: ' + output_directory)
        os.mkdir(output_directory)
    else:
        print('directory already exists: ' + output_directory)
    return output_directory


def PrintDictionary(data):
    for key, value in data.items():
        print('{0}:{1}', key, value)


def ReadJson(filename, isPrint=False):
    with open(filename, 'r') as json_file:
        data = json.load(json_file)

    if isPrint:
        PrintDictionary(data)
    return data


def ReadNpz(filename, isPrint=False):
    tmp = np.load(filename)
    data = pd.DataFrame(tmp['data'], columns=tmp['header'].astype(str))
    if isPrint:
        print(data)
    return data


def GetSortedFolderList(path, reverse=True):
    folders = glob.glob(os.path.join(path, '*/'))  # get all folders
    folders.sort(key=lambda x: os.path.getmtime(x), reverse=reverse)  # sort newest to oldest
    return folders


def GetFilePath(levelFolder, levelStartTime):
    if os.path.exists(levelFolder + "\\" + levelStartTime + ".filePath.json"):
        filePath = Dict2Obj(ReadJson(levelFolder + "\\" + levelStartTime + ".filePath.json"))
    else:
        return None

    # convert to relative path to run independently of origin machine
    members = [attr for attr in dir(filePath) if not callable(getattr(filePath, attr)) and not attr.startswith("__")]  # doesn't work inside class?

    import ntpath

    def path_leaf(path):
        head, tail = ntpath.split(path)
        return tail or ntpath.basename(head)

    for member in members:
        if member == "name":
            continue
        setattr(filePath, member, levelFolder + "\\" + path_leaf(getattr(filePath, member)))

    return filePath


def FindLevelFolder():
    gameFolders = GetSortedFolderList(Path.data)

    for gameFolder in gameFolders:
        print(gameFolder)
        gameStartTime = str.split(gameFolder, "\\")[-2]
        gameFilename = gameFolder + gameStartTime + ".game.json"

        if os.path.exists(gameFilename):
            levelFolders = GetSortedFolderList(gameFolder)

            for levelFolder in levelFolders:
                levelStartTime = str.split(levelFolder, "\\")[-2]
                filePath = GetFilePath(levelFolder=levelFolder, levelStartTime=levelStartTime)

                if filePath is not None:
                    print(filePath.levelJson)
                    level = ReadJson(filePath.levelJson)
                    filename = filePath.frameDataNpz

                    if os.path.exists(filename):  # level complete!
                        return levelFolder
    return None


def PopulateObserver(levelFolder="", isPopulatePerformanceData=True):
    if levelFolder == "":
        levelFolder = FindLevelFolder()

    gameStartTime = str.split(levelFolder, "\\")[2]
    gameFolder = Path.data + gameStartTime + "\\"
    levelStartTime = str.split(levelFolder, "\\")[3]
    game = ReadJson(gameFolder + gameStartTime + ".game.json")
    listLevels = pd.DataFrame(game['listLevels'])
    filePath = GetFilePath(levelFolder=levelFolder, levelStartTime=levelStartTime)
    level = ReadJson(filePath.levelJson)

    observer = Dict2Obj(ReadJson(filePath.observerJson))
    vertices = ReadNpz(filePath.verticesNpz)
    vertices = vertices.rename(columns={'serializableVertex.position.x': 'x',
                                        'serializableVertex.position.y': 'y',
                                        'serializableVertex.position.z': 'z'})

    verticesJson = pd.DataFrame(ReadJson(filePath.verticesJson)['listSerializableVertex'])
    verticesJson = verticesJson.rename(columns={'serializableVertex.position.x': 'x',
                                                'serializableVertex.position.y': 'y',
                                                'serializableVertex.position.z': 'z'})

    if isPopulatePerformanceData:
        trialData = ReadNpz(filePath.trialDataNpz)
        trialData = trialData[(trialData.T != 0).any()]  # drop rows with all zeros due to level time lists and pre-allocation in GameRunner.cs
        trialDataJson = pd.DataFrame(ReadJson(filePath.trialDataJson)['listTrialData'])
        frameData = ReadNpz(filename=filePath.frameDataNpz)
        timeCueSetup = np.nan
        timeExperiment = (frameData['Time.time'].iloc[-1] - frameData['Time.time'].iloc[0])/60
        if hasattr(filePath, 'listCriticalTrialDataJson'):
            criticalTrialData = pd.DataFrame(ReadJson(filePath.listCriticalTrialDataJson)['listCriticalTrialData'])
        else:
            criticalTrialData = None
    else:
        trialData = None
        trialDataJson = None
        frameData = None
        timeCueSetup = None
        timeExperiment = None
        criticalTrialData = None

    pathResults = Path.results + gameStartTime + "\\" + levelStartTime + "\\"

    print("gameFolder: {}".format(gameFolder))
    print("gameStartTime: {}".format(gameStartTime))
    print("game: {}".format(game))
    print("levelFolder: {}".format(levelFolder))
    print("levelStartTime: {}".format(levelStartTime))
    print("level: {}".format(level))
    print("listLevels".format(listLevels))
    print("observer".format(observer))
    print("vertices".format(vertices))
    print("verticesJson".format(verticesJson))
    print("trialData".format(trialData))
    print("trialDataJson".format(trialDataJson))
    print("frameData".format(frameData))
    print("pathResults".format(pathResults))
    print("filePath: {}".format(filePath))
    print("timeCueSetup: {}".format(timeCueSetup))
    print("timeExperiment: {}".format(timeExperiment))
    print("criticalTrialData: {}".format(criticalTrialData))

    # WriteObserverFile(custom_observer=levelFolder)

    if not os.path.isdir(pathResults):
        os.makedirs(pathResults)

    observer = Observer(gameFolder=gameFolder,
                        gameStartTime=gameStartTime,
                        game=game,
                        listLevels=listLevels,
                        levelFolder=levelFolder,
                        levelStartTime=levelStartTime,
                        level=level,
                        observer=observer,
                        vertices=vertices,
                        verticesJson=verticesJson,
                        trialData=trialData,
                        trialDataJson=trialDataJson,
                        frameData=frameData,
                        pathResults=pathResults,
                        filePath=filePath,
                        timeCueSetup=timeCueSetup,
                        timeExperiment=timeExperiment,
                        criticalTrialData=criticalTrialData)
    return observer
