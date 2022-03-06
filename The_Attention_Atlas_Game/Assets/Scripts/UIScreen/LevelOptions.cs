using System.Collections;
using System.Collections.Generic;
using DataStructures;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LevelOptions : MonoBehaviour
{

    public InputField descriptorField;
    public Toggle isFreeviewToggle;
    public Toggle isRandomizedToggle;
    public Toggle isGameToggle;
    public InputField timeLimitField;
    public Button expandListButton;


    public GameManager.Level lvl;

    private bool isExpanded;
    private GameObject optionsPanel;
    private SpriteAtlas collapseButtonSA = Resources.Load<SpriteAtlas>("Symbols/CollapseButton");

    // Start is called before the first frame update
    void Start()
    {
        isExpanded = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Call after all public vars have been populated
    /// </summary>
    public void buildView()
    {
        descriptorField.text = lvl.descriptor;
        isFreeviewToggle.isOn = lvl.isFreeViewing;
        isRandomizedToggle.isOn = lvl.isRandomiseTrialOrder;
        isGameToggle.isOn = lvl.isGame;
        timeLimitField.text = lvl.timeLimitMinutes.ToString();
    }

    public void toggleLevelOptions()
    {
        Debug.Log("Button Pressed");
        Image buttonImage = expandListButton.GetComponent<Image>();
        if (isExpanded)
        {
            buttonImage.sprite = collapseButtonSA.GetSprite("Collapse");
            expandListButton.image = buttonImage;
            isExpanded = false;
        }
        else
        {
            buttonImage.sprite = collapseButtonSA.GetSprite("Uncollapse");
            expandListButton.image = buttonImage;
            isExpanded = true;
        }
    }

   /* public string descriptor;
    public List<Options> listOptions;
    public bool isFreeViewing;
    public bool isRandomiseTrialOrder;
    public bool isGame; // true for game, false for tutorial - for integration with analyses - to immplement
    public float timeLimitMinutes; // minutes*/
}
