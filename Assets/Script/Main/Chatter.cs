using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chatter : MonoBehaviour
{
    [SerializeField]
    GameObject chatWindowOrigin;

    const string messageKey = "Message";

    [SerializeField]
    bool isPlayable;
    FirebaseConnector connector;
    TextFielder fielder;
    string message;
    GameObject chatBox;
    List<GameObject> chatBoxList;
    int maxBoxCnt;

    // Use this for initialization
    void Start()
    {
        chatBoxList = new List<GameObject>();
        maxBoxCnt = 4;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayable) return;

        message = fielder.GetMessage();
        if (message == null||message.Equals("")) return;

        //SendMessageBox(message);
        SendMessageBox2(message);
        connector.AddAsync(messageKey, message);
        fielder.CleanInputField("メッセージ", "");
    }

    public void InitializeChatDB(Firebase.Database.DatabaseReference reference, bool playable)
    {
        isPlayable = playable;
        connector = new FirebaseConnector(reference, !playable);
        if (playable)
        {
            fielder = transform.parent.parent.Find("TextField").GetComponent<TextFielder>();
            fielder.CleanInputField("メッセージ", "");
        }
        else
        {
            //Debug.Log(connector.MyReference.Key);
            connector.MyReference.ValueChanged
                += (object sender, Firebase.Database.ValueChangedEventArgs args) =>
            {
                if (args.DatabaseError != null)
                {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                }

                string rawMessage = args.Snapshot.Child(messageKey).Value.ToString();
                //SendMessageBox(rawMessage);
                SendMessageBox2(rawMessage);
            };
        }
    }

    void SendMessageBox(string message)
    {
        Destroy(chatBox, 1);

        chatBox = Instantiate(chatWindowOrigin);
        chatBox.transform.SetParent(transform);
        chatBox.transform.localScale = Vector3.one;
        chatBox.transform.localPosition = Vector3.zero;
        Transform text = chatBox.transform.Find("Text");
        if (transform.localPosition.y < 0)
        {
            chatBox.transform.localEulerAngles = Vector3.forward * 180;
            text.transform.localEulerAngles = Vector3.forward * 180;
        }
        text.GetComponent<Text>().text = message;
    }

    void SendMessageBox2(string message)
    {
        float updateLength = 30;
        float updateHeight = 10;
        int boxCnt = chatBoxList.Count;
        for(int i = 0; i < boxCnt; i++)
        {
            chatBoxList[i].transform.localPosition += Vector3.right * updateLength;
            chatBoxList[i].transform.localPosition += Vector3.up * updateHeight;
            if (i == boxCnt - 1)
            {
                Destroy(chatBoxList[i].transform.Find("Bar").gameObject);
                Destroy(chatBoxList[i].transform.Find("Bar2").gameObject);
            }
        }

        GameObject chatBox = Instantiate(chatWindowOrigin);
        chatBox.transform.SetParent(transform);
        chatBox.transform.localScale = Vector3.one;
        chatBox.transform.localPosition = Vector3.zero;
        Transform text = chatBox.transform.Find("Text");
        if (transform.localPosition.y < 0)
        {
            chatBox.transform.localEulerAngles = Vector3.forward * 180;
            text.transform.localEulerAngles = Vector3.forward * 180;
        }
        text.GetComponent<Text>().text = message;
        chatBoxList.Add(chatBox);
        if (chatBoxList.Count > maxBoxCnt)
        {
            Destroy(chatBoxList[0]);
            chatBoxList.RemoveAt(0);
        }
        GameObject.Find(SoundPlayer.objectName).GetComponent<SoundPlayer>().PlaySE(
            SEname.Message);
    }
}