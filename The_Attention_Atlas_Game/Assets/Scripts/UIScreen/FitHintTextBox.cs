using UnityEngine;
using UnityEngine.UI;

public class FitHintTextBox : MonoBehaviour
{
    public Text textBox;
    public string inputText;

    private Vector2 boxSize;

    private void OnGUI()
    {
        if (inputText == null)
        {
            inputText = "Error";
        }
        GUIContent content = new GUIContent(inputText);

        GUIStyle style = GUI.skin.box;
        style.alignment = TextAnchor.MiddleCenter;

        // Compute how large the button needs to be.
        Vector2 size = style.CalcSize(content);

        setBoxSize(size);
    }

    private void setBoxSize(Vector2 size)
    {
        if (size != boxSize)
        {
            Vector2 newSize = new Vector2(size.x + 20f, size.y+ 4f);
            textBox.GetComponent<RectTransform>().sizeDelta = newSize;
            GetComponent<RectTransform>().sizeDelta = newSize;
            textBox.text = inputText;
            boxSize = newSize;
            //Debug.Log(inputText + ":  width: " + newSize.x + ", height:  " + newSize.y);
        }
    }


}
