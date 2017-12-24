using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFielder : MonoBehaviour
{
    GameObject dialogBox, inputField, sendButton;

    // Use this for initialization
    void Start()
    {
        dialogBox = transform.Find("DialogBox").gameObject;
        inputField = transform.Find("InputField").gameObject;
        sendButton = transform.Find("SendButton").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator SwitchDialog(bool on)
    {
        Image image = dialogBox.GetComponent<Image>();

        Vector2 destPos;
        Color destColor;
        if (on)
        {
            destPos = new Vector2(-150, -370);
            destColor = new Color(1, 1, 0, 0.6f);
        }
        else
        {
            destPos = new Vector2(-150, -470);
            destColor = new Color(1, 1, 0, 0);
        }

        yield return new WaitForEndOfFrame();
    }
}
