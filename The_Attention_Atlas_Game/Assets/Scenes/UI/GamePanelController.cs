using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using DataStructures;
using TMPro;
using System.Linq;
using System;

public class GamePanelController : MonoBehaviour
{
    // options
    public Toggle isPlayAudioInstructions;
    public Toggle isPlayAffirmations;
    public Toggle isShowLevelResults;

    public static bool isUseUI = false; // default state is false - run from GameManager.cs - if true and run from InitializerScene - will overide parameters in GameManager.cs
    public static string playerID = "";

    // configuration
    public TextMeshProUGUI configurationNameText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI levelsText;
    public TextMeshProUGUI optionsText;

    // presets
    public TMP_Dropdown presetsDropdown;
    public TMP_InputField filenameField;
    public Button saveButton;

    // ui
    public TMP_InputField logInputField;

    // game
    public Button start_VR_button;
    public Button exitButton;

    // player
    public TMP_InputField playerField;

    // other references
    public Transform windowContentsTransform; // expandable panel parent
    public GameObject levelPanelPrefab;

    public static GameManager.Game game; // needs to be static to be referenced from GameManager.cs

    string presetsDirectory;
    string gameStartTime = GameManager.GetNewStartTime(); // suggested filename for saving preset

    List<string> presetsFullPath = new List<string>();
    List<string> presetsFilename = new List<string>();

    public static List<GameObject> levelGameObjects = new List<GameObject>();
    public static List<int> levelNumbers = new List<int>();

    public TMP_Dropdown calibrationDropdown;

    [Serializable]
    public enum Calibration
    {
        allLevels,
        firstLevel,
        none // use last
    }

    public static Calibration calibration;
    
    public static string ReturnPresetsDirectory()
    {
        string pathModifier = CentralMemory.ReturnPathModifier();
        string presetsDirectory = Path.GetFullPath(Path.Combine(@Application.dataPath, pathModifier, @"_PRESETS\"));
        return presetsDirectory;
    }

    void UpdateAudioInstructions(bool isPlayAudioInstructions)
    {
        GameOptions.isPlayAudioInstructions = isPlayAudioInstructions;
    }

    void UpdateAffirmations(bool isPlayAffirmations)
    {
        GameOptions.isPlayAffirmations = isPlayAffirmations;
    }

    void UpdateShowLevelResults(bool isUpdateLevelResults)
    {
        GameOptions.isShowLevelResults = isUpdateLevelResults;
    }

    void Start()
    {
        // options
        isPlayAudioInstructions.onValueChanged.AddListener(UpdateAudioInstructions);
        GameOptions.isPlayAudioInstructions = isPlayAudioInstructions.isOn;
        isPlayAffirmations.onValueChanged.AddListener(UpdateAffirmations);
        GameOptions.isPlayAffirmations = isPlayAffirmations.isOn;
        isShowLevelResults.onValueChanged.AddListener(UpdateShowLevelResults);
        GameOptions.isShowLevelResults = isShowLevelResults.isOn;

        isUseUI = true;

        presetsDirectory = ReturnPresetsDirectory();

        start_VR_button.onClick.AddListener(VRButtonOnClick);
        saveButton.onClick.AddListener(SaveButtonOnClick);
        exitButton.onClick.AddListener(ExitButtonClick);

        // calibration
        calibrationDropdown.onValueChanged.AddListener(UpdateCalibration);
        AddNamesToDropDown(calibrationDropdown, Enum.GetNames(typeof(Calibration)).ToList());
        LoadCalibration();

        RefreshPresetsDropdown();

        presetsDropdown.onValueChanged.AddListener(LoadPreset);

        filenameField.text = gameStartTime + ".game.json";

        game = PopulateGame();
        UpdateLevelAndOptionsNumbers();
        AddLevelPanels();

        UpdateDurationText();

        playerField.onValueChanged.AddListener(UpdatePlayerID);
        UpdatePlayerID(playerField.text);
    }

    private void Update()
    {
        GameManager.EscapeGame(isSaveData: false);
    }

    // playerID

    private void UpdatePlayerID(string ID)
    {
        playerID = ID;
        Debug.LogFormat("playerID: {0}", playerID);
    }

    // calibration

    void LoadCalibration()
    {
        if (File.Exists(presetsDirectory + "last.calibration.json")) // load from file
        {
            calibration = SaveData.ReadJson<Calibration>(presetsDirectory + "last.calibration.json");
            calibrationDropdown.value = (int)calibration;
        }
        else
        {
            calibration = Calibration.allLevels;
            calibrationDropdown.value = ((int)calibration);
            SaveData.WriteAndReadJson<Calibration>(calibration, presetsDirectory + "last.calibration.json");
        }
    }

    void UpdateCalibration(int value)
    {
        calibration = (Calibration)value;
    }

    // configuration
    public void UpdateLevelAndOptionsNumbers() // // to keep track when contracting options panels so that appropriate options panels can be destroyed - also looks nice on the interface
    { 
        levelNumbers = new List<int>();
        int levelNumber = 0;
        int totalOptions = 0;

        foreach (GameManager.Level level in game.listLevels)
        {
            levelNumbers.Add(levelNumber);
            int optionsNumber = 0;

            foreach (Options options in level.listOptions)
            {
                options.optionsNumber = optionsNumber;
                optionsNumber++;
                totalOptions++;
            }
            levelNumber++;
        }

        levelsText.text = "Number of Levels: " + levelNumber.ToString();
        optionsText.text = "Number of Options: " + totalOptions.ToString();
    }

    public void UpdateDurationText()
    {
        float duration = 0;

        foreach (var level in game.listLevels)
        {
            duration += level.timeLimitMinutes;
            durationText.text = "Duration: " + duration.ToString("0.00") + " minutes";
        }
    }

    // presets

    public GameManager.Game PopulateGame()
    {
        GameManager.Game game;

        if (File.Exists(presetsDirectory + "last.game.json")) // load from file
        {
            game = SaveData.ReadJson<GameManager.Game>(presetsDirectory + "last.game.json");
            configurationNameText.text = "Name: last.game.json";
            SetPresetsDropdownValue("last.game.json");
        }
        else // create new configuration
        {
            configurationNameText.text = "Name: default";

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

    public void AddLevelPanels()
    {
        levelGameObjects = new List<GameObject>();

        int levelNumber = 0;
        foreach (GameManager.Level level in game.listLevels)
        {
            levelGameObjects.Add(AddLevelPanel(levelNumber, level));
            levelNumber++;
        }
    }

    private GameObject AddLevelPanel(int levelNumber, GameManager.Level level)
    {
        GameObject levelPanel = Instantiate(levelPanelPrefab);
        levelPanel.transform.SetParent(windowContentsTransform);

        LevelPanelController2 levelPanelController2 = levelPanel.GetComponent<LevelPanelController2>();
        levelPanelController2.SetLevelParameters(levelNumber, level);
        return (levelPanel);
    }

    public void RefreshPresetsDropdown()
    {
        // presets dropdown
        presetsFullPath = Directory.GetFiles(presetsDirectory, "*.json").ToList();
        presetsFilename = new List<string>();

        foreach (var preset in presetsFullPath)
        {
            presetsFilename.Add(Path.GetFileName(preset));
        }

        AddNamesToDropDown(presetsDropdown, presetsFilename);
    }

    public static void AddNamesToDropDown(TMP_Dropdown dropdown, List<string> textList)
    {
        dropdown.ClearOptions();

        foreach (string text in textList)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData
            {
                text = text
            };
            dropdown.options.Add(optionData);
        }
    }

    void SetPresetsDropdownValue(string value)
    {
        for (int i = 0; i < presetsDropdown.options.Count; i++)
        {
            if (presetsDropdown.options[i].text == value)
            {
                presetsDropdown.SetValueWithoutNotify(i);
                break;
            }
        }
    }

    public void RefreshLevelDisplay()
    {
        // remove old
        Transform panelParentTransform = windowContentsTransform.transform;
        List<GameObject> children = new List<GameObject>();

        foreach (Transform panelChildTransform in panelParentTransform)
        {
            if (panelChildTransform.name != "LevelHeader")
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
        AddLevelPanels();
        UpdateDurationText();
    }

    void LoadPreset(int presetNumber)
    {
        game = SaveData.ReadJson<GameManager.Game>(presetsFullPath[presetsDropdown.value]);
        Debug.LogFormat("gameOptions.listLevels.Count: {0}", game.listLevels.Count);
        RefreshLevelDisplay();
        UpdateDurationText();
        UpdateLevelAndOptionsNumbers();
        configurationNameText.text = "Name: " + presetsFilename[presetsDropdown.value];
    }

    void SaveButtonOnClick() // save game options as .json
    {
        print("You have clicked the Save button!");

        if (!Directory.Exists(presetsDirectory))
        {
            Directory.CreateDirectory(presetsDirectory);
        }

        // error checking on filename to go here...

        string path = presetsDirectory + filenameField.text;
        print(path);
        GameManager.SaveGameConfiguration(game, path);
        RefreshPresetsDropdown();
        SetPresetsDropdownValue(filenameField.text);
        configurationNameText.text = "Name: " + filenameField.text;
        SaveData.WriteAndReadJson<Calibration>(calibration, presetsDirectory + "last.calibration.json");
    }

    // ui
    public void UpdateLog(string text)
    {
        logInputField.text = text;
    }

    // game
    void VRButtonOnClick()
    {
        Debug.Log("You have clicked the VR button!");
        GameManager.SaveGameConfiguration(game, presetsDirectory + "last.game.json");
        SaveData.WriteAndReadJson<Calibration>(calibration, presetsDirectory + "last.calibration.json");

        StartCoroutine(GoToGameManager());

        if (!Directory.Exists(presetsDirectory))
        {
            Directory.CreateDirectory(presetsDirectory);
        }

        
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

    // generic
    public static List<GameObject> GetChildren(Transform parentTransform, List<string> childrenToExclude = null)
    {
        List<GameObject> childrenOfInterest = new List<GameObject>();

        foreach (Transform childTransform in parentTransform)
        {
            if (childrenToExclude == null)
            {
                childrenOfInterest.Add(childTransform.gameObject);
            }
            else if (!childrenToExclude.Contains(childTransform.name))
            {
                childrenOfInterest.Add(childTransform.gameObject);
            }
        }
        return childrenOfInterest;
    }

}
