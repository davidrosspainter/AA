using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
//using UnityEditor;
using System.IO;
using DataStructures;
using System.Runtime.Remoting.Messaging;
using SFB;


public class LevelVertPanel : MonoBehaviour
{
    public static bool isUseUI = false; // default state is false - run from GameManager.cs - if true and run from InitializerScene - will overide parameters in GameManager.cs

    public Button VRButton;
    public Button saveButton;
    public Button loadButton;
    public Button addLevelButton;
    public Button removeLastLevelButton;
    public Button addOptionsButton;
    public Button removeLastOptionsButton;
    public Button exitButton;

    public static GameManager.Game gameOptions;
    public LevelExpandablePanelLayoutScript childPanel;
    
    string presetsDirectory;
    string gameStartTime = GameManager.GetNewStartTime(); // suggested filename for saving preset

    public Text configurationName;
    public Text durationText;

    public Dropdown presetsDropdown;
    string[] presetsFullPath;
    List<string> presetsFilename = new List<string>();

    void Start()
    {
        isUseUI = true;

        string pathModifier = CentralMemory.ReturnPathModifier();
        presetsDirectory = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"_PRESETS\"));

        gameOptions = PopulateGame();
        childPanel.gameOptions = gameOptions;
        childPanel.populateLevels();

        UpdateDurationText();

        VRButton.onClick.AddListener(VRButtonOnClick);
        saveButton.onClick.AddListener(SaveButtonOnClick);
        loadButton.onClick.AddListener(LoadButtonOnClick);
        addLevelButton.onClick.AddListener(AddLevelClick);
        removeLastLevelButton.onClick.AddListener(SubtractLastLevelClick);
        addOptionsButton.onClick.AddListener(AddOptionsClick);
        removeLastOptionsButton.onClick.AddListener(RemoveLastOptionsClick);
        exitButton.onClick.AddListener(ExitButtonClick);

        // presets dropdown
        presetsFullPath = Directory.GetFiles(presetsDirectory, "*.json");
        presetsDropdown.options = new List<Dropdown.OptionData>();

        foreach (var preset in presetsFullPath)
        {
            presetsFilename.Add(Path.GetFileName(preset));
            Dropdown.OptionData optionsData = new Dropdown.OptionData();
            optionsData.text = presetsFilename[presetsFilename.Count - 1];
            presetsDropdown.options.Add(optionsData);
        }

        presetsDropdown.onValueChanged.AddListener(LoadPreset);
    }


    void LoadPreset(int presetNumber)
    {
        gameOptions = SaveData.ReadJson<GameManager.Game>(presetsFullPath[presetNumber]);
        Debug.LogFormat("gameOptions.listLevels.Count: {0}", gameOptions.listLevels.Count);
        RefreshLevelDisplay();
    }


    void SaveButtonOnClick() // save game options as .json
    {
        print("You have clicked the Save button!");

        if (!Directory.Exists(presetsDirectory))
        {
            Directory.CreateDirectory(presetsDirectory);
        }

        //string path = EditorUtility.SaveFilePanel(title: "Save Game Options as *.json", directory: presetsDirectory, defaultName: gameStartTime + ".game.json", extension: "json");
        string path = StandaloneFileBrowser.SaveFilePanel(title: "Save Game Options as *.json", directory: presetsDirectory, defaultName: gameStartTime + ".game.json", extension: "json");

        GameManager.SaveGameConfiguration(childPanel.gameOptions, path);
        print(path);
    }

    public void RefreshLevelDisplay()
    {
        // remove old
        Transform panelParentTransform = GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel/").transform;
        List<GameObject> children = new List<GameObject>();

        foreach (Transform panelChildTransform in panelParentTransform)
        {
            if (panelChildTransform.name != "LevelTitlePanel")
            {
                print(panelChildTransform.name);
                children.Add(panelChildTransform.gameObject);
            }
        }

        foreach (var child in children) // doesn't seem to work properly if destroyed in "identification" loop above
        {
            DestroyImmediate(child);
        }

        // populate new
        childPanel.gameOptions = gameOptions;
        childPanel.populateLevels();

        UpdateDurationText();
    }

    public void UpdateDurationText()
    {
        float duration = 0;

        foreach (var level in gameOptions.listLevels)
        {
            duration += level.timeLimitMinutes;
            durationText.text = "Duration: " + duration.ToString("0.00") + " minutes";
        }
    }

    void LoadButtonOnClick() // save game options as .json - does not currently save amendments to level
    {
        print("You have clicked the Load button!");

        if (!Directory.Exists(presetsDirectory))
        {
            Directory.CreateDirectory(presetsDirectory);
        }

        //string path = EditorUtility.OpenFilePanel(title: "Read Game Options *.json", directory: presetsDirectory, extension: "json");
        string[] pathArray = StandaloneFileBrowser.OpenFilePanel(title: "Read Game Options *.json", directory: presetsDirectory, extension: "json", multiselect: false);
        string path = pathArray[0];
        print(path);

        configurationName.text = "Configuration: " + Path.GetFileName(path);

        gameOptions = SaveData.ReadJson<GameManager.Game>(path);
        Debug.LogFormat("gameOptions.listLevels.Count: {0}", gameOptions.listLevels.Count);

        RefreshLevelDisplay();
    }

    void AddLevelClick() // button on left
    {
        print("You have clicked the Add Level button!");
        gameOptions.listLevels.Add(new GameManager.Level(listOptions: new List<Options> { new Options() }));
        RefreshLevelDisplay();
    }

    void SubtractLastLevelClick()
    {
        print("You have clicked the Subtract Last Level button!");
        gameOptions.listLevels.RemoveAt(gameOptions.listLevels.Count - 1);
        RefreshLevelDisplay();
    }

   
    public void InsertLevel(int levelNumber) // add button on level
    {
        print("InsertLevel()");
        gameOptions.listLevels.Insert(levelNumber, new GameManager.Level(listOptions: new List<Options> { new Options() }));
        Debug.LogFormat("gameOptions.listLevels.Count:{0}", gameOptions.listLevels.Count);
        RefreshLevelDisplay();
    }

    public void RemoveLevel(int levelNumber)
    {
        print("LevelPanelController.RemoveLevel()");
        gameOptions.listLevels.RemoveAt(levelNumber);
        RefreshLevelDisplay();
    }

    void AddOptionsClick()
    {
        print("AddOptionsClick()");
        gameOptions.listLevels[gameOptions.listLevels.Count-1].listOptions.Add(new Options());
        RefreshLevelDisplay();
    }

    void RemoveLastOptionsClick()
    {
        print("RemoveLastOptionsClick()");
        gameOptions.listLevels[gameOptions.listLevels.Count-1].listOptions.RemoveAt(gameOptions.listLevels[gameOptions.listLevels.Count-1].listOptions.Count - 1);
        RefreshLevelDisplay();
    }

    void VRButtonOnClick()
    {
        Debug.Log("You have clicked the VR button!");
        StartCoroutine(GoToGameManager());

        if (!Directory.Exists(presetsDirectory))
        {
            Directory.CreateDirectory(presetsDirectory);
        }

        GameManager.SaveGameConfiguration(childPanel.gameOptions, presetsDirectory + "last.game.json");
    }

    private IEnumerator GoToGameManager()
    {
        UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
        yield return null;

        UnityEngine.XR.XRSettings.enabled = true;
        SceneManager.LoadScene("GameManager");
    }

    public void ExitButtonClick()
    {
        SaveData.QuitGame();
    }

    public GameManager.Game PopulateGame()
    {
        GameManager.Game game;

        if (File.Exists(presetsDirectory + "last.game.json")) // load from file
        {
            game = SaveData.ReadJson<GameManager.Game>(presetsDirectory + "last.game.json");
            configurationName.text = "Configuration: last.game.json";
        }
        else // create new configuration
        {
            configurationName.text = "Configuration: default";

            GameManager.Level tutorial = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Tutorial", isGame: false, isRandomiseTrialOrder: true, timeLimitMinutes: 0.5f); // 8 targets
            tutorial.listOptions.Add(new Options(
                descriptor: "oneRingUniqueFeature",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.uniqueFeature,
                coordinates: Options.Coordinates.fullField,
                keepAngleArray: GameOptions.sphericalSpacing * 1,
                radius: 2));

            GameManager.Level level01 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 1: Pairs", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 1.7f); // 8 targets
            level01.listOptions.Add(new Options(
               descriptor: "pair1",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.letters,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.pair1,
               radius: 2));
            level01.listOptions.Add(new Options(
                descriptor: "pair2",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.pair2,
                radius: 2));
            level01.listOptions.Add(new Options(
                descriptor: "pair3",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.pair3,
                radius: 2));
            level01.listOptions.Add(new Options(
                descriptor: "pair4",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.pair4,
                radius: 2));

            GameManager.Level level02 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 2: Horizontal/Vertical", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 3.4f); // 16 targets (2)
            level02.listOptions.Add(new Options(
               descriptor: "horizontal",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.letters,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.horizontal,
               radius: 2));
            level02.listOptions.Add(new Options(
               descriptor: "vertical",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.letters,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.vertical,
               radius: 2));

            GameManager.Level level03 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 3: Search Modes", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 5.1f); // 24 targets
            level03.listOptions.Add(new Options(
                descriptor: "oneRingUniqueFeature",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.uniqueFeature,
                coordinates: Options.Coordinates.fullField,
                keepAngleArray: GameOptions.sphericalSpacing * 1,
                radius: 2));
            level03.listOptions.Add(new Options(
                descriptor: "oneRingConjunction",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.conjunction,
                coordinates: Options.Coordinates.fullField,
                keepAngleArray: GameOptions.sphericalSpacing * 1,
                radius: 2));
            level03.listOptions.Add(new Options(
                descriptor: "oneRingRainbow",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.rainbow,
                coordinates: Options.Coordinates.fullField,
                keepAngleArray: GameOptions.sphericalSpacing * 1,
                radius: 2));

            GameManager.Level level04 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 4: Stimuli", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: 5.1f); // 24 targets
            level04.listOptions.Add(new Options(
               descriptor: "oneRingFood",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.food,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));
            level04.listOptions.Add(new Options(
               descriptor: "oneRingCards",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.cards,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));
            level04.listOptions.Add(new Options(
               descriptor: "oneRingBalloons",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.balloons,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));

            GameManager.Level level05 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 5: Depth", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: 6.8f); // 32 targets
            level05.listOptions.Add(new Options(
                descriptor: "depthConfig1",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.depthConfig1,
                radius: 2,
                radiusDepth: 4,
                keepAngleArray: GameOptions.sphericalSpacing * 2));
            level05.listOptions.Add(new Options(
                descriptor: "depthConfig2",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.depthConfig2,
                radius: 2,
                radiusDepth: 4,
                keepAngleArray: GameOptions.sphericalSpacing * 2));

            GameManager.Level level06 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 6: Full Field", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: 5.1f); // 24 targets
            level06.listOptions.Add(new Options(
                descriptor: "fullFieldSerial",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.fullField,
                radius: 2,
                keepAngleArray: GameOptions.sphericalSpacing * 3));


            GameManager.Level level07 = new GameManager.Level(gameStartTime: gameStartTime, descriptor: "Level 7: Free Viewing", isGame: true, isFreeViewing: true, timeLimitMinutes: 1.0f);
            level07.listOptions.Add(new Options(radius: 2, coordinates: Options.Coordinates.fullField, keepAngleArray: GameOptions.sphericalSpacing * 3)); // to generate surfaces and vertices to allow analysis to run

            // game
            game = new GameManager.Game(startTime: gameStartTime,
                                        descriptor: "main game",
                                        listLevels: new List<GameManager.Level> { tutorial,
                                        level01,
                                        level02,
                                        level03,
                                        level04,
                                        level05,
                                        level06,
                                        level07,
                            });
        }
        return game;
    }
}
