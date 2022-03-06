using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataStructures;
using System.Collections.Generic;

public class LevelPanelController2 : MonoBehaviour
{
    private bool isExpanded = false;

    public int levelNumber;
    public GameManager.Level level;

    public TextMeshProUGUI levelNumberText;
    public TMP_InputField descriptorField;
    public Toggle isFreeviewToggle;
    public Toggle isRandomizedToggle;
    public Toggle isGameToggle;
    public TMP_InputField timeLimitField;
    public Button expandButton;
    public Button plusButton;
    public Button minusButton;

    GamePanelController gamePanelController;
    Transform windowContentsTransform;

    public GameObject levelPanelPrefab;
    public GameObject optionsPanelPrefab;

    void Start()
    {
        descriptorField.onValueChanged.AddListener(UpdateDescriptor);
        isFreeviewToggle.onValueChanged.AddListener(UpdateIsFreeView);
        isRandomizedToggle.onValueChanged.AddListener(UpdateIsRandomized);
        isGameToggle.onValueChanged.AddListener(UpdateIsGame);
        timeLimitField.onValueChanged.AddListener(UpdateTimeLimit);

        expandButton.onClick.AddListener(ExpandButtonClick);
        plusButton.onClick.AddListener(PlusLevel);
        minusButton.onClick.AddListener(MinusLevel);

        gamePanelController = GameObject.Find("SCRIPTS").GetComponent<GamePanelController>();
        windowContentsTransform = GameObject.Find("Canvas/CollapsableWindow/WindowContents").transform;

        UpdateLevelPanelDisplay();
    }

    public void SetLevelParameters(int levelNumber, GameManager.Level level)
    {
        this.levelNumber = levelNumber;
        this.level = level;
    }

    public void UpdateLevelPanelDisplay()
    {
        descriptorField.text = level.descriptor;
        isFreeviewToggle.isOn = level.isFreeViewing;
        isRandomizedToggle.isOn = level.isRandomiseTrialOrder;
        isGameToggle.isOn = level.isGame;
        timeLimitField.text = level.timeLimitMinutes.ToString();
        levelNumberText.text = levelNumber.ToString();
        transform.name = "level." + levelNumber.ToString();
    }

    void ExpandButtonClick()
    {
        if (!isExpanded)
        {
            AddOptionsPanels();
            expandButton.GetComponentInChildren<TextMeshProUGUI>().text = "▲";
            isExpanded = true;
        }
        else
        {
            // get all options panels matching the current level
            List<GameObject> panels = GamePanelController.GetChildren(parentTransform: windowContentsTransform, childrenToExclude: new List<string> { "levelHeader" });
            List<GameObject> childrenToDestroy = new List<GameObject>();

            foreach (GameObject panel in panels)
            {
                if (panel.HasComponent<OptionsPanelController2>())
                {
                    if (panel.GetComponent<OptionsPanelController2>().levelNumber == levelNumber)
                    {
                        childrenToDestroy.Add(panel);
                    }
                }
            }

            foreach (GameObject child in childrenToDestroy)
            {
                DestroyImmediate(child);
            }

            expandButton.GetComponentInChildren<TextMeshProUGUI>().text = "▼";
            isExpanded = false;
        }
    }

    private void AddOptionsPanels()
    {
        print("AddOptionsPanels");
        Debug.LogFormat("levelNumber: {0}, GamePanelController.game.listLevels.Count - 1: {1}", levelNumber, GamePanelController.game.listLevels.Count - 1);

        int optionsNumber = 0;

        foreach (Options options in level.listOptions)
        {
            GameObject optionsPanel = Instantiate(optionsPanelPrefab);
            optionsPanel.GetComponent<OptionsPanelController2>().SetOptionsParameters(levelNumber, optionsNumber, options);
            optionsPanel.transform.SetParent(windowContentsTransform);
            optionsPanel.transform.name = "level." + levelNumber + ".options." + optionsNumber;

            int levelSiblingIndex = transform.GetSiblingIndex();
            optionsPanel.transform.SetSiblingIndex(levelSiblingIndex + 1 + optionsNumber);

            optionsNumber++;
        }
    }

    public void PlusLevel()
    {
        print("PlusLevel()");

        GameManager.Level tutorial = new GameManager.Level(gameStartTime: "", descriptor: "Tutorial", isGame: false, isRandomiseTrialOrder: true, timeLimitMinutes: 0.5f); // 8 targets
        tutorial.listOptions.Add(new Options(
            descriptor: "oneRingUniqueFeature",
            numberOfRepetitions: 1,
            stimulus: Options.Stimulus.letters,
            searchMode: Options.SearchMode.uniqueFeature,
            coordinates: Options.Coordinates.fullField,
            keepAngleArray: GameOptions.sphericalSpacing * 1,
            radius: 2));

        GamePanelController.game.listLevels.Insert(levelNumber + 1, tutorial);
        AddLevelPanel(levelNumber, level);
        UpdateAllLevelPanelsWithNewLevelNumbers();
        gamePanelController.UpdateLevelAndOptionsNumbers();
        gamePanelController.UpdateDurationText();
    }

    private GameObject AddLevelPanel(int levelNumber, GameManager.Level level)
    {
        print("AddLevelPanel()");
        GameObject levelPanel = Instantiate(levelPanelPrefab);
        levelPanel.transform.SetParent(windowContentsTransform);

        LevelPanelController2 levelPanelController2 = levelPanel.GetComponent<LevelPanelController2>();
        levelPanelController2.SetLevelParameters(levelNumber, level);
        return (levelPanel);
    }

    public void UpdateAllLevelPanelsWithNewLevelNumbers() // to keep track when contracting options panels so that appropriate options panels can be destroyed - also looks nice on the interface
    {
        print("UpdateAllLevelPanelsWithNewLevelNumbers()");
        List<GameObject> panels = GamePanelController.GetChildren(parentTransform: windowContentsTransform, childrenToExclude: new List<string> { "levelHeader" });
        int levelCounter = 0;

        foreach (GameObject panel in panels)
        {
            if (panel.HasComponent<LevelPanelController2>() & panel.activeInHierarchy) // allows for inactive debugging panels in hierarchy under WindowsContents
            {
                Debug.LogFormat("levelCounter: {0}, panel.name: {1}", levelCounter, panel.name);
                panel.GetComponent<LevelPanelController2>().SetLevelParameters(levelCounter, GamePanelController.game.listLevels[levelCounter]);
                levelCounter++;
            }
        }
    }

    public void MinusLevel()
    {
        print("MinusLevel()");
        if (GamePanelController.game.listLevels.Count > 1)
        {
            GamePanelController.game.listLevels.RemoveAt(levelNumber);
            DestroyImmediate(transform.gameObject);
            UpdateAllLevelPanelsWithNewLevelNumbers();
        }
        else
        {
            gamePanelController.UpdateLog("The game requires at least one level...");
        }
        gamePanelController.UpdateLevelAndOptionsNumbers();
        gamePanelController.UpdateDurationText();
    }

    private void UpdateDescriptor(string descriptor)
    {
        level.descriptor = descriptorField.text;
        GamePanelController.game.listLevels[levelNumber] = level;
    }

    public void UpdateIsFreeView(bool isFreeView)
    {
        level.isFreeViewing = isFreeviewToggle.isOn;
        GamePanelController.game.listLevels[levelNumber] = level;
    }

    public void UpdateIsRandomized(bool isFreeView)
    {
        level.isRandomiseTrialOrder = isRandomizedToggle.isOn;
        GamePanelController.game.listLevels[levelNumber] = level;
    }

    public void UpdateIsGame(bool isGame)
    {
        level.isGame = isGameToggle.isOn;
        GamePanelController.game.listLevels[levelNumber] = level;
    }

    public void UpdateTimeLimit(string timeLimit)
    {
        level.timeLimitMinutes = float.Parse(timeLimitField.text);
        GamePanelController.game.listLevels[levelNumber] = level;
        gamePanelController.UpdateDurationText();
    }
}
