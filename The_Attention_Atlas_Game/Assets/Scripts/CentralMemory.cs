using System.Collections.Generic;
using UnityEngine;
using DataStructures;
using NumSharp;
using System.IO;

public class CentralMemory : MonoBehaviour
{
    // data
    public static Observer observer;
    public static FilePath filePath;
    public static GameManager.Level level;

    public static List<GameRunner.TrialData> trialDataList = new List<GameRunner.TrialData>();
    public static List<List<float>> trialDataFloat = new List<List<float>>();
    public static NDArray trialDataNp;

    public static List<FrameRecorder.FrameData> frameDataList = new List<FrameRecorder.FrameData>();
    public static List<List<float>> frameDataFloat = new List<List<float>>();
    public static float[,] frameDataArray = new float[,] { };

    public static string[] raycastTargetLayers = new string[] { "searchTarget" };

    // paths
    public static class GamePaths
    {
        public static string data;
        public static string network;
        public static string screenshot;
        public static string pythonRelative = "";
        public static string pythonAbsolute = ""; // defined in start
        public static string pythonAnalysis;

        public static void Setup() // differs depending on editor or build
        {
            Debug.Log("GamePaths.Setup");
            string pathModifier = ReturnPathModifier();
            Debug.LogFormat("pathModifier:{0}", pathModifier);

            Debug.LogFormat("@Application.dataPath:{0}", @Application.dataPath);

            data = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"_DATA\")); // whenever changed, also needs to be updated in Globals.cs
            Debug.LogFormat("data:{0}", data);

            network = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"external_dependencies\Network.Buffer.DRP.24.09.18\"));
            Debug.LogFormat("network:{0}", network);

            screenshot = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"screenshots\"));
            Debug.LogFormat("screenshot:{0}", screenshot);

            pythonAbsolute = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"external_dependencies\Python39\python.exe"));
            Debug.LogFormat("pythonPathAbsolute:{0}", pythonAbsolute);

            pythonAnalysis = Path.Combine(@Application.dataPath, pathModifier, @"offline_analyses\PlotAttentionNew.py");
            Debug.LogFormat("pythonAnalysis:{0}", pythonAnalysis);
        }
    }

    public static string ReturnPathModifier()
    {
        Debug.Log("ReturnPathModifier()");

        string pathModifier;

        if (Application.isEditor)
        {
            Debug.LogFormat("Application.isEditor:{0}", Application.isEditor);
            pathModifier = @"..\..\";
        }
        else
        {
            Debug.LogFormat("Application.isEditor:{0}", Application.isEditor);
            pathModifier = @"..\..\..\";
        }
        return pathModifier;
    }

    public static void StartExternalProcess(string command)
    {
        string pathModifier = ReturnPathModifier();

        string commandTxtPath = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"external_dependencies\StartExternalProcess\bin\Debug\command.txt"));
        string startExternalProcessPath = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"external_dependencies\StartExternalProcess\bin\Debug\StartExternalProcess.exe"));

        Debug.LogFormat("Application.dataPath:{0}", Application.dataPath);
        Debug.LogFormat("commandTxtPath:{0}", commandTxtPath);
        Debug.LogFormat("startExternalProcessPath:{0}", startExternalProcessPath);

        SaveData.WriteTextFile(commandTxtPath, command);
        Application.OpenURL(startExternalProcessPath);
    }

    void ResetStaticVariables()
    {
        trialDataList = new List<GameRunner.TrialData>();
        trialDataFloat = new List<List<float>>();
        frameDataList = new List<FrameRecorder.FrameData>();
        frameDataFloat = new List<List<float>>();
        frameDataArray = new float[,] { };
    }

    void Start()
    {
        ResetStaticVariables();

        Debug.LogFormat("GameManager.game.listLevels.Count > 0:{0}", GameManager.game.listLevels.Count > 0);

        GamePaths.Setup();
        GameOptions.Setup();

        // setup observer
        observer = new Observer(ID: GamePanelController.playerID,
                                gameStartTime: GameManager.gameStartTime,
                                levelStartTime: GameManager.GetNewStartTime());

        filePath = new FilePath(dataPath: observer.dataPath, levelStartTime: observer.levelStartTime);

        // load level
        Debug.LogFormat("GameManager.currentLevel:{0}", GameManager.currentLevel);
        level = GameManager.game.listLevels[GameManager.currentLevel];
        level.levelStartTime = observer.levelStartTime;

        if (GameOptions.isAutomate)
        {
            //level.timeLimitMinutes = float.NegativeInfinity;
            level.timeLimitMinutes *= .1f;
        }

        GameManager.game.listLevels[GameManager.currentLevel].levelStartTime = observer.levelStartTime;

        if (GameOptions.saveDataThread)
            SaveData.SaveGameConfigurationThread(GameManager.game, GameManager.currentGameSummaryPath); // (with updated level start time for the previous level)
        else
            GameManager.SaveGameConfiguration(); // (with updated level start time for the previous level)

    }
}