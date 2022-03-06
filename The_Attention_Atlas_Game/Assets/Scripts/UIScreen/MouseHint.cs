using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string hintText = "";        //  Set in the editor

    private bool mouseInElement = false;
    private bool showingHint = false;
    private IEnumerator showHintCoRoutine;
    private GameObject hintBox;

    private void Start()
    {
        hintBox = GameObject.Find("MouseHintTextImage");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseInElement = false;
        StopCoroutine(showHintCoRoutine);
        pushHintOffScreen();       
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseInElement = true;
        showHintCoRoutine = showHint(hintText);
        StartCoroutine(showHintCoRoutine);
    }

    IEnumerator showHint(string text)
    {
        yield return new WaitForSeconds(0.75f);  
        if (mouseInElement)
        {
            if (!showingHint)
            {
                hintBox.transform.position = new Vector3(Input.mousePosition.x + 2, Input.mousePosition.y + 2, 0f);     //  +2 +2 because the 0 0 means it is pointing at the text box and messes up the algorithm
                hintBox.GetComponent<FitHintTextBox>().inputText = text;
                showingHint = true;
            }
        }
    }

    /// <summary>
    /// This is an extrememly janky piece of code that pushes the hint box off the screen.
    /// Setting it inactive was causing lots of issues
    /// </summary>
    private void pushHintOffScreen()
    {
        hintBox.transform.position = new Vector3(-100000, -100000, 0f);
        showingHint = false;
    }
}
