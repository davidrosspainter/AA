using DataStructures;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsDropdown : MonoBehaviour
{
    public enum DropDownType { Stimulus, SearchMode, Coordinates };
    public DropDownType ddType = DropDownType.SearchMode;

    private Dropdown dd;
    private int startingVal;
    // Start is called before the first frame update
    void Start()
    {
        dd = this.GetComponent<Dropdown>();
        dd.ClearOptions();
        switch (ddType)
        {
            case DropDownType.SearchMode:
                setSearchVars();
                break;
            case DropDownType.Stimulus:
                setStimulussVars();
                break;
            case DropDownType.Coordinates:
                setCoordinatesVars();
                break;
            default:
                Debug.LogError("OptionsDropdown: ddType not set to recognizable value.");
                break;
        }
        dd.value = startingVal;
    }

    public void addNamesToDD(string[] names)
    {
        foreach (string t in names)
        {
            Dropdown.OptionData m_NewData = new Dropdown.OptionData();
            m_NewData.text = t;
            dd.options.Add(m_NewData);
        }
    }

    public static void AddNamesToDD(string[] names, Dropdown dd)
    {
        foreach (string t in names)
        {
            Dropdown.OptionData m_NewData = new Dropdown.OptionData();
            m_NewData.text = t;
            dd.options.Add(m_NewData);
        }
    }

    private void setSearchVars()
    {
        string[] stimNames = Enum.GetNames(typeof(Options.SearchMode));
        addNamesToDD(stimNames);
    }

    private void setCoordinatesVars()
    {
        string[] stimNames = Enum.GetNames(typeof(Options.Coordinates));
        addNamesToDD(stimNames);
    }

    private void setStimulussVars()
    {
        string[] stimNames = Enum.GetNames(typeof(Options.Stimulus));
        addNamesToDD(stimNames);
    }

    public void setValue(int val)
    {
        startingVal = val;
    }
}
