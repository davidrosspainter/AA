using DataStructures;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class OptionsPanelController2 : MonoBehaviour
{
    public int levelNumber;
    public int optionsNumber;
    public Options options;

    Options defaultOptions = new Options(
                                        descriptor: "oneRingUniqueFeature",
                                        numberOfRepetitions: 1,
                                        stimulus: Options.Stimulus.letters,
                                        searchMode: Options.SearchMode.uniqueFeature,
                                        coordinates: Options.Coordinates.fullField,
                                        keepAngleArray: GameOptions.sphericalSpacing * 1,
                                        radius: 2);

    public TextMeshProUGUI optionsNumberText;
    public TMP_InputField descriptorField;
    public TMP_InputField numRepititionsField;
    public TMP_Dropdown stimulusDropdown;
    public TMP_Dropdown searchDropdown;
    public TMP_Dropdown coordinateDropdown;
    public TMP_InputField radiusField;
    public TMP_InputField radiusDepthField;
    public TMP_InputField keepAngleArrayField;
    public TMP_InputField recursionLevelField;
    public Button plusButton;
    public Button minusButton;

    public GameObject optionsPanelPrefab;

    GamePanelController gamePanelController;
    Transform windowContentsTransform;


    // Start is called before the first frame update
    void Start()
    {
        print("OptionsPanelController.Start()");

        descriptorField.onValueChanged.AddListener(UpdateDescriptor);
        numRepititionsField.onValueChanged.AddListener(UpdateNumberOfRepetitions);
        stimulusDropdown.onValueChanged.AddListener(UpdateStimulus);
        searchDropdown.onValueChanged.AddListener(UpdateSearchMode);
        coordinateDropdown.onValueChanged.AddListener(UpdateCoordinates);
        radiusField.onValueChanged.AddListener(UpdateRadius);
        radiusDepthField.onValueChanged.AddListener(UpdateRadiusDepth);
        keepAngleArrayField.onValueChanged.AddListener(UpdateKeepAngleArray);
        recursionLevelField.onValueChanged.AddListener(UpdateRecursionLevel);

        plusButton.onClick.AddListener(PlusOptions);
        minusButton.onClick.AddListener(MinusOptions);

        GamePanelController.AddNamesToDropDown(stimulusDropdown, Enum.GetNames(typeof(Options.Stimulus)).ToList());
        GamePanelController.AddNamesToDropDown(searchDropdown, Enum.GetNames(typeof(Options.SearchMode)).ToList());
        GamePanelController.AddNamesToDropDown(coordinateDropdown, Enum.GetNames(typeof(Options.Coordinates)).ToList());

        gamePanelController = GameObject.Find("SCRIPTS").GetComponent<GamePanelController>();
        windowContentsTransform = GameObject.Find("Canvas/CollapsableWindow/WindowContents").transform;

        optionsNumberText.text = options.optionsNumber.ToString();

        UpdateOptionsPanelDisplay();
    }

    public void SetOptionsParameters(int levelNumber, int optionsNumber, Options options)
    {
        print("SetAndDisplayOptionsPanelParameters()");
        this.levelNumber = levelNumber;
        this.optionsNumber = optionsNumber;
        this.options = options;
    }

    public void UpdateOptionsPanelDisplay()
    {
        print("UpdateOptionsPanelDisplay()");
        optionsNumberText.text = optionsNumber.ToString();
        transform.gameObject.name = "level." + levelNumber + ".options." + optionsNumber;

        descriptorField.text = this.options.descriptor;
        numRepititionsField.text = options.numberOfRepetitions.ToString();
        stimulusDropdown.value = (int)options.stimulus;
        searchDropdown.value = (int)options.searchMode;
        coordinateDropdown.value = (int)options.coordinates;
        radiusField.text = options.radius.ToString();
        radiusDepthField.text = options.radiusDepth.ToString();
        keepAngleArrayField.text = options.keepAngleArray.ToString();
        recursionLevelField.text = options.recursionLevel.ToString();
    }

    private void AddOptionsPanel() // for adding options from within options
    {
        GameObject optionsPanel = Instantiate(optionsPanelPrefab);
        optionsPanel.GetComponent<OptionsPanelController2>().SetOptionsParameters(levelNumber, -1, GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber + 1]);
        optionsPanel.transform.SetParent(windowContentsTransform);
        optionsPanel.transform.name = "level." + levelNumber + ".options." + optionsNumber;

        int levelSiblingIndex = transform.GetSiblingIndex();
        Debug.LogFormat("levelSiblingIndex: {0}", levelSiblingIndex);
        optionsPanel.transform.SetSiblingIndex(levelSiblingIndex + 1);

        gamePanelController.UpdateLevelAndOptionsNumbers();
    }

    void PlusOptions()
    {
        print("PlusOptions()");
        GamePanelController.game.listLevels[levelNumber].listOptions.Insert(optionsNumber + 1, defaultOptions);
        AddOptionsPanel();
        UpdateAllOptionsPanelsWithNewOptionsNumbers();
    }

    void MinusOptions()
    {
        print("MinusOptions()");

        if (GamePanelController.game.listLevels[levelNumber].listOptions.Count > 1)
        {
            GamePanelController.game.listLevels[levelNumber].listOptions.RemoveAt(optionsNumber);
            DestroyImmediate(transform.gameObject);
            UpdateAllOptionsPanelsWithNewOptionsNumbers();
            Debug.LogFormat("GamePanelController.game.listLevels[levelNumber].listOptions.Count: {0}", GamePanelController.game.listLevels[levelNumber].listOptions.Count);
        }
        else
        {
            gamePanelController.UpdateLog("The level requires at least one options...");
        }

        gamePanelController.UpdateLevelAndOptionsNumbers();
    }

    public void UpdateAllOptionsPanelsWithNewOptionsNumbers() // to keep track when contracting options panels so that appropriate options panels can be destroyed - also looks nice on the interface
    {
        print("UpdateAllOptionsPanelsWithNewOptionsNumbers()");

        int optionsNumber = 0;

        foreach (Options options in GamePanelController.game.listLevels[levelNumber].listOptions)
        {
            options.optionsNumber = optionsNumber;
            optionsNumber++;
        }

        List<GameObject> panels = GamePanelController.GetChildren(parentTransform: windowContentsTransform, childrenToExclude: new List<string> { "levelHeader" });

        foreach (var item in panels)
        {
            Debug.LogFormat("item.name: {0}, item.transform.GetSiblingIndex(): {1}", item.name, item.transform.GetSiblingIndex());
        }

        int optionsCounter = 0;

        foreach (GameObject panel in panels)
        {
            if (panel.HasComponent<OptionsPanelController2>() & panel.activeInHierarchy) // allows for inactive debugging panels in hierarchy under WindowsContents
            {
                if (panel.GetComponent<OptionsPanelController2>().levelNumber == levelNumber)
                {
                    Debug.LogFormat("optionsCounter: {0}, levelCounter: {1}, panel.name: {2}", optionsCounter, levelNumber, panel.name);
                    GamePanelController.game.listLevels[levelNumber].listOptions[optionsCounter].optionsNumber = optionsCounter;
                    panel.GetComponent<OptionsPanelController2>().SetOptionsParameters(levelNumber, optionsCounter, GamePanelController.game.listLevels[levelNumber].listOptions[optionsCounter]);
                    print(GamePanelController.game.listLevels[levelNumber].listOptions[optionsCounter].descriptor);

                    optionsCounter++;
                }
            }
        }
    }

    public void UpdateDescriptor(string descriptor)
    {
        print("UpdateDescriptor()");
        options.descriptor = descriptorField.text;
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateNumberOfRepetitions(string numberOfRepetitions)
    {
        print("UpdateNumberOfRepetitions()");
        options.numberOfRepetitions = Int16.Parse(numRepititionsField.text);
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateStimulus(int stimulus)
    {
        print("UpdateStimulus()");
        options.stimulus = (Options.Stimulus)stimulus;
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateSearchMode(int searchMode)
    {
        print("UpdateSearchMode()");
        options.searchMode = (Options.SearchMode)searchMode;
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateCoordinates(int coordinates)
    {
        print("UpdateCoordinates()");
        options.coordinates = (Options.Coordinates)coordinates;
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRadius(string radius)
    {
        print("UpdateRadius()");
        options.radius = Single.Parse(radius);
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRadiusDepth(string radiusDepth)
    {
        print("UpdateRadiusDepth()");
        options.radiusDepth = Single.Parse(radiusDepth);
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateKeepAngleArray(string keepAngleArray)
    {
        print("UpdateKeepAngleArray()");
        options.keepAngleArray = Single.Parse(keepAngleArray);
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRecursionLevel(string recursionLevel)
    {
        print("UpdateRecursionLevel()");
        options.recursionLevel = Int16.Parse(recursionLevel);
        GamePanelController.game.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }
}
