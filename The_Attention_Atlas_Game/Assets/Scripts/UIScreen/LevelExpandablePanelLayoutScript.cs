using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExpandablePanelLayoutScript : MonoBehaviour
{

    public GameManager.Level level;

    public OptionsPanelController levelListOptionsView;
    public GameManager.Game gameOptions;

    private float height;

    /// <summary>
    /// Call this after the gameOptions have been filled
    /// </summary>
    public void populateLevels()
    {
        height = 0f;
        int levelNumber = 0;
        foreach (GameManager.Level lvl in gameOptions.listLevels)
        {
            addLevel(lvl, true, levelNumber);
            levelNumber++;
        }
        setFrameHeight();
    }

    private void addLevel(GameManager.Level lvl, bool isOptions, int levelNumber)
    {
        //GameObject expPanel = (GameObject)Instantiate(Resources.Load("prefabs/ExpandableLevelPanel"));
        GameObject expPanel = Instantiate(Resources.Load("prefabs/ExpandableLevelPanel")) as GameObject;

        GameObject lvlPanel = (GameObject)Instantiate(Resources.Load("prefabs/LevelPanel"));
        LevelPanelController lvlOptns = lvlPanel.GetComponent<LevelPanelController>();
        lvlOptns.lvl = lvl;
        lvlOptns.buildView(levelNumber);
        lvlPanel.transform.name = lvl.descriptor;
        expPanel.transform.name = lvl.descriptor;

        expPanel.transform.SetParent(transform);
        lvlPanel.transform.SetParent(expPanel.transform);

        if (isOptions) addOptionsPanel(lvl, expPanel, levelNumber);
    }

    private void addOptionsPanel(GameManager.Level lvl, GameObject expPanel, int levelNumber)
    {
        int optionsNumber = 0;

        foreach (DataStructures.Options options in lvl.listOptions)
        {
            GameObject optionsPanel = (GameObject)Instantiate(Resources.Load("prefabs/OptionsPanel"));
            optionsPanel.GetComponent<OptionsPanelController>().setOptionsParameters(options, optionsNumber, levelNumber);

            optionsPanel.transform.name = options.descriptor;
            optionsPanel.transform.SetParent(expPanel.transform);

            optionsNumber++;
        }
    }


    private void Start()
    {
        
    }

    private void setFrameHeight()
    {
        Debug.LogFormat("this.transform.childCount: {0}", transform.childCount.ToString());
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform child = this.transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                RectTransform childrt = child.GetComponent(typeof(RectTransform)) as RectTransform;
                height = height + childrt.rect.height;
            }
        }
        RectTransform rt = this.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2(rt.rect.width, height);
    }
}
