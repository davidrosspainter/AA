using DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelController : MonoBehaviour
{
    public int levelNumber;
    public int optionsNumber;
    
    public InputField descriptorField;
    public InputField numRepititionsField;
    public Dropdown stimulusDropdown;
    public Dropdown searchDropdown;
    public Dropdown coordinateDropdown;
    public InputField radiusField;
    public InputField radiusDepthField;
    public InputField keepAngleArrayField;
    public InputField recursionLevelField;

    private Transform parentTransform;
    private Transform thisTransform;
    private RectTransform parentRectTransform;
    private RectTransform thisRectTransform;
    private Options options;

    // Start is called before the first frame update
    void Start()
    {
        print("OptionsPanelController.Start()");

        parentTransform = this.transform.parent;
        thisTransform = this.transform;
        updateTransform();

        descriptorField.onValueChanged.AddListener(UpdateDescriptor);
        numRepititionsField.onValueChanged.AddListener(UpdateNumberOfRepetitions);
        stimulusDropdown.onValueChanged.AddListener(UpdateStimulus);
        searchDropdown.onValueChanged.AddListener(UpdateSearchMode);
        coordinateDropdown.onValueChanged.AddListener(UpdateCoordinates);
        radiusField.onValueChanged.AddListener(UpdateRadius);
        radiusDepthField.onValueChanged.AddListener(UpdateRadiusDepth);
        keepAngleArrayField.onValueChanged.AddListener(UpdateKeepAngleArray);
        recursionLevelField.onValueChanged.AddListener(UpdateRecursionLevel);
    }

    // Update is called once per frame
    void Update()
    {
        if (parentTransform.hasChanged)
        {
            //Debug.Log("OptionsPanelController:  Registered Change in Transform");
            updateTransform();
            setComponentSizes(); 

            parentTransform.hasChanged = false;
        }
    }

    private void updateTransform()
    {
        parentRectTransform = parentTransform.GetComponent<RectTransform>();
        thisRectTransform = thisTransform.GetComponent<RectTransform>();
        thisRectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width, thisRectTransform.rect.height);
    }

    private void setComponentSizes()
    {

    }

    /// <summary>
    /// Sets the local options variable and applies the internal variables to the UI fields
    /// </summary>
    /// <param name="op"></param>
    public void setOptionsParameters(Options op, int optionsNumber, int levelNumber)
    {
        this.optionsNumber = optionsNumber;
        this.levelNumber = levelNumber;

        options = op;

        descriptorField.text = options.descriptor;
        numRepititionsField.text = options.numberOfRepetitions.ToString();
        stimulusDropdown.GetComponent<OptionsDropdown>().setValue((int)options.stimulus);
        searchDropdown.GetComponent<OptionsDropdown>().setValue((int)options.searchMode);
        coordinateDropdown.GetComponent<OptionsDropdown>().setValue((int)options.coordinates);
        radiusField.text = options.radius.ToString();
        radiusDepthField.text = options.radiusDepth.ToString();
        keepAngleArrayField.text = options.keepAngleArray.ToString();
        recursionLevelField.text = options.recursionLevel.ToString();
    }


    public void UpdateDescriptor(string descriptor)
    {
        print("UpdateDescriptor()");
        options.descriptor = descriptorField.text;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateNumberOfRepetitions(string numberOfRepetitions)
    {
        print("UpdateNumberOfRepetitions()");
        options.numberOfRepetitions = System.Int16.Parse(numRepititionsField.text);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateStimulus(int stimulus)
    {
        print("UpdateStimulus()");
        options.stimulus = (Options.Stimulus)stimulus;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateSearchMode(int searchMode)
    {
        print("UpdateSearchMode()");
        options.searchMode = (Options.SearchMode)searchMode;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateCoordinates(int coordinates)
    {
        print("UpdateCoordinates()");
        options.coordinates = (Options.Coordinates)coordinates;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRadius(string radius)
    {
        print("UpdateRadius()");
        options.radius = System.Int16.Parse(radius);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRadiusDepth(string radiusDepth)
    {
        print("UpdateRadiusDepth()");
        options.radiusDepth = System.Int16.Parse(radiusDepth);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateKeepAngleArray(string keepAngleArray)
    {
        print("UpdateKeepAngleArray()");
        options.keepAngleArray = System.Int16.Parse(keepAngleArray);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }

    public void UpdateRecursionLevel(string recursionLevel)
    {
        print("UpdateRecursionLevel()");
        options.recursionLevel = System.Int16.Parse(recursionLevel);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber].listOptions[optionsNumber] = options;
    }
}
