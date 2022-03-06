using System.Collections;
using System.Collections.Generic;
using DataStructures;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LevelPanelController : MonoBehaviour
{
    public int levelNumber;

    public InputField descriptorField;
    public Toggle isFreeviewToggle;
    public Toggle isRandomizedToggle;
    public Toggle isGameToggle;
    public InputField timeLimitField;

    public Button expandListButton;
    public Button plusButton;
    public Button minusButton;

    public GameManager.Level lvl = new GameManager.Level();

    private bool isExpanded;

    private float expandedHeight = 0;
    private float collapsedHeight = 0;


    void Start()
    {
        isExpanded = false;

        descriptorField.onValueChanged.AddListener(UpdateDescriptor);
        isFreeviewToggle.onValueChanged.AddListener(UpdateIsFreeView);
        isRandomizedToggle.onValueChanged.AddListener(UpdateIsRandomized);
        isGameToggle.onValueChanged.AddListener(UpdateIsGame);
        timeLimitField.onValueChanged.AddListener(UpdateTimeLimit);

        plusButton.onClick.AddListener(PlusLevel);
        minusButton.onClick.AddListener(MinusLevel);
    }

    public void PlusLevel()
    {
        print("PlusLevel()");
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel").GetComponent<LevelVertPanel>().InsertLevel(levelNumber+1);
    }

    public void MinusLevel()
    {
        print("MinusLevel()");
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel").GetComponent<LevelVertPanel>().RemoveLevel(levelNumber);
    }
    

    /// <summary>
    /// Call after all public vars have been populated
    /// </summary>
    /// 

    private void UpdateDescriptor(string descriptor)
    {
        lvl.descriptor = descriptorField.text;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber] = lvl;

    }

    public void UpdateIsFreeView(bool isFreeView)
    {
        lvl.isFreeViewing = isFreeviewToggle.isOn;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber] = lvl;
    }

    public void UpdateIsRandomized(bool isFreeView)
    {
        lvl.isRandomiseTrialOrder = isRandomizedToggle.isOn;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber] = lvl;
    }

    public void UpdateIsGame(bool isGame)
    {
        lvl.isGame = isGameToggle.isOn;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber] = lvl;
    }

    public void UpdateTimeLimit(string timeLimit)
    {
        lvl.timeLimitMinutes = float.Parse(timeLimitField.text);
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel/LevelExpandablePanel").GetComponent<LevelExpandablePanelLayoutScript>().gameOptions.listLevels[levelNumber] = lvl;
        GameObject.Find("BaseUICanvas/LevelVertLayoutPanel").GetComponent<LevelVertPanel>().UpdateDurationText();
    }


    public void buildView(int levelNumber)
    {
        this.levelNumber = levelNumber;

        descriptorField.text = lvl.descriptor;
        isFreeviewToggle.isOn = lvl.isFreeViewing;
        isRandomizedToggle.isOn = lvl.isRandomiseTrialOrder;
        isGameToggle.isOn = lvl.isGame;
        timeLimitField.text = lvl.timeLimitMinutes.ToString();
    }

    public void getLevel()
    {

    }





    //private void buildOptionsPanels()
    //{
    //    int optionsNumber = 0;
    //    foreach(DataStructures.Options options in lvl.listOptions)
    //    {
    //        GameObject optionsPanel = (GameObject)Instantiate(Resources.Load("prefabs/OptionsPanel"));
    //        optionsPanel.GetComponent<OptionsPanelController>().setOptionsParameters(options, optionsNumber);

    //        optionsPanel.transform.name = options.descriptor;
    //        optionsPanel.transform.parent = transform;
    //        optionsNumber++;
    //    }
    //}

    public void toggleLevelOptions()
    {
        if (collapsedHeight == 0) getCollapsedHeight();

        Image buttonImage = expandListButton.GetComponent<Image>();
        RectTransform rt = transform.parent.gameObject.GetComponent<RectTransform>();
        if (isExpanded)
        {
            buttonImage.sprite = Resources.Load<Sprite>("Symbols/Uncollapse");
            expandListButton.image = buttonImage;
            setActiveOptions(false);

            rt.sizeDelta = new Vector2(rt.rect.width, collapsedHeight);
            isExpanded = false;
        }
        else
        {
            buttonImage.sprite = Resources.Load<Sprite>("Symbols/Collapse");
            expandListButton.image = buttonImage;

            setActiveOptions(true);

            if (expandedHeight == 0) getExpandedHeight();

            rt.sizeDelta = new Vector2(rt.rect.width, expandedHeight);
            isExpanded = true;
        }
        changeGrandParentHeight(isExpanded);
    }

    private void getExpandedHeight()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform child = transform.parent.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                expandedHeight = expandedHeight + child.GetComponent<RectTransform>().rect.height;
            }
        }
    }

    private void getCollapsedHeight()
    {
        collapsedHeight = transform.parent.GetChild(0).GetComponent<RectTransform>().rect.height;
    }

    /// <summary>
    /// This method sets the Options for the level atcive or inactive.  This should be used in conjunction with the collapse button.
    /// </summary>
    /// <param name="active">Are the Options Active in view</param>
    private void setActiveOptions(bool active)
    {
        for (int i = 1; i < transform.parent.childCount; i++)       //  i = 1 to ignore the level row
        {
            Transform child = transform.parent.GetChild(i);
            child.gameObject.SetActive(active);
        }
    }

    private void changeGrandParentHeight(bool isExpanded)
    {
        RectTransform rt = transform.parent.parent.gameObject.GetComponent<RectTransform>();

        if (isExpanded)
        {
            Debug.Log("Exp Height: " + expandedHeight);
            Debug.Log("Coll Height: " + collapsedHeight);
            rt.sizeDelta = new Vector2(rt.rect.width, rt.rect.height + (expandedHeight - collapsedHeight));
        }
        else
        {
            rt.sizeDelta = new Vector2(rt.rect.width, rt.rect.height - (expandedHeight - collapsedHeight));
        }
    }

}
