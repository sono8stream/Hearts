using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextFielder : MonoBehaviour
{
    GameObject dialogBox, inputField, sendButton;
    string messageCash;

    // Use this for initialization
    void Awake()
    {
        dialogBox = transform.Find("DialogBox").gameObject;
        inputField = transform.Find("InputField").gameObject;
        sendButton = transform.Find("SendButton").gameObject;
        messageCash = null;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerator SwitchDialog(bool onDialog,string s = "")
    {
        Image image = dialogBox.GetComponent<Image>();
        Transform t = dialogBox.transform;
        Text text = dialogBox.transform.Find("Text").GetComponent<Text>();

        Vector3 destPos;
        Color destColor;
        text.gameObject.SetActive(onDialog);
        if (onDialog)//消える
        {
            destPos = new Vector2(-150, -415);
            destColor = new Color(1, 1, 0, 0.6f);
            text.text = s;
        }
        else
        {
            destPos = new Vector2(-150, -485);
            destColor = new Color(1, 1, 0, 0);
        }

        while (Mathf.Abs(t.localPosition.y - destPos.y) > 0.1f)
        {
            t.localPosition = (t.localPosition * 3 + destPos) / 4;
            image.color = (image.color * 3 + destColor) / 4;
            yield return new WaitForEndOfFrame();
        }
    }

    public void CashMessage()
    {
        InputField input = inputField.GetComponent<InputField>();
        messageCash = input.text;
        input.text = "";
        GameObject.Find(SoundPlayer.objectName).GetComponent<SoundPlayer>().PlaySE(
            SEname.Enter);
    }

    public string GetMessage()
    {
        if (messageCash == null) return null;

        string temp = messageCash;
        messageCash = null;
        inputField.GetComponent<InputField>().text = "";
        return temp;
    }

    public void CleanInputField(string caption,string defaultInput="")
    {
        inputField.GetComponent<InputField>().text = defaultInput;
        inputField.transform.Find("Placeholder").GetComponent<Text>().text = caption;
    }
}