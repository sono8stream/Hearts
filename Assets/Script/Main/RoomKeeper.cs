using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomKeeper : MonoBehaviour
{
    [SerializeField]
    GamePlayer player;
    [SerializeField]
    TextFielder textFielder;

    const int nameMaxLength = 6;

    FirebaseConnector connector;
    bool roomParent;
    int stateNo;
    string playerName;
    string roomName;
    int nowPlayerCnt;

    // Use this for initialization
    void Start()
    {
        textFielder.StartCoroutine(textFielder.SwitchDialog(true, string.Format(
                "ようこそ。っと、あなた何さんだい？({0}文字以内)", nameMaxLength)));

        connector = new FirebaseConnector("Rooms");
    }

    // Update is called once per frame
    void Update()
    {
        switch (stateNo)
        {
            case (int)KeeperState.NameReception:
                playerName = textFielder.GetMessage();
                if (playerName == null || playerName.Length > nameMaxLength) return;

                stateNo = (int)KeeperState.RoomReception;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true, string.Format(
                    "OK、" + playerName
                    + "さん。プレイしたい部屋はどこだい。({0}文字以内)",
                    nameMaxLength)));
                break;
            case (int)KeeperState.RoomReception:
                roomName = textFielder.GetMessage();
                if (roomName == null) return;

                connector.Read(roomName + "/players");
                stateNo = (int)KeeperState.Inquiry;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                    "問い合わせ中..."));
                break;

            case (int)KeeperState.Inquiry:
                string jsonSnap = null;
                if (!connector.GetReadData(ref jsonSnap)) return;

                if (jsonSnap == null)
                {
                    MakeRoom();
                    roomParent = true;
                }
                else
                {
                    Debug.Log("Entered");
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                        roomName + "に入ったぜ。参加者募集中だ。"));
                    roomParent = false;
                }
                stateNo = (int)KeeperState.Idle;
                break;
        }
    }

    void OnApplicationQuit()
    {
        connector.RemoveItem(roomName + "/Players");
    }

    void MakeRoom()
    {
        connector.AddAsync(roomName + "/Players",
            new Dictionary<string, object>() { { playerName, 0 } });
        textFielder.StartCoroutine(textFielder.SwitchDialog(true,
            roomName + "を用意しておいたぜ。参加者募集中だ。"));
        player.InitializeDB(0, connector.MyReference.Child(roomName + "/player0/handCards"));
    }
}

enum KeeperState
{
    NameReception = 0,RoomInquiry, RoomReception, Inquiry, Idle
}