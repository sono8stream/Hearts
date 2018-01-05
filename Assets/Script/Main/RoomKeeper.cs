﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomKeeper : MonoBehaviour
{
    [SerializeField]
    GameMaster master;
    [SerializeField]
    TextFielder textFielder;

    const int nameMaxLength = 6;
    const string playersDir = "Players";

    FirebaseConnector connector;
    bool roomParent;
    bool onReady;
    int stateNo;
    string playerName;
    string roomName;
    int playerNo;
    int nowPlayerCnt;
    string[] playerNameArray;

    // Use this for initialization
    void Start()
    {
        textFielder.StartCoroutine(textFielder.SwitchDialog(true, string.Format(
                "ようこそ。あなたは何さんだい？({0}文字以内)", nameMaxLength)));
        textFielder.CleanInputField("名前を入力してね");

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
                textFielder.CleanInputField("部屋名を入力してね");
                break;

            case (int)KeeperState.RoomReception:
                roomName = textFielder.GetMessage();
                if (roomName == null) return;

                connector.Read(roomName + "/" + playersDir);
                stateNo = (int)KeeperState.Inquiry;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                    "問い合わせ中..."));
                break;

            case (int)KeeperState.Inquiry:
                Firebase.Database.DataSnapshot snap = null;
                if (!connector.GetReadData(ref snap)) return;

                if (snap.Value == null)
                {
                    MakeRoom();
                    playerNo = 0;
                    roomParent = true;
                    stateNo = (int)KeeperState.Wanting;
                }
                else
                {
                    EnterRoom();
                    playerNo = (int)snap.ChildrenCount;
                    roomParent = false;
                    stateNo = (int)KeeperState.PlayWaiting;
                }

                connector.AddAsync(roomName + "/" + playersDir + "/" + playerNo, playerName);
                break;

            case (int)KeeperState.Wanting:
                string command = textFielder.GetMessage();
                if (!onReady) return;
                if ((command == null || !command.Equals("S"))
                    && !Input.GetKeyDown(KeyCode.S)) return;

                connector.AddAsync(roomName + "/RoomState", (int)RoomState.Playing);
                stateNo = (int)KeeperState.SetupGameMaster;
                break;

            case (int)KeeperState.SetupGameMaster:
                master.InitializeDB(nowPlayerCnt, roomParent, playerNameArray, playerNo,
                    connector.MyReference.Child(roomName + "/Master"));
                gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 部屋が丸々消えた場合、子の通信管理がかなり厄介なので検討中
    /// </summary>
    void OnApplicationQuit()
    {
        if (roomParent)
        {
            connector.RemoveItem(roomName);
        }
        else
        {
            connector.RemoveItem(roomName + "/" + playersDir + "/" + playerName);
            connector.RemoveItem(roomName + "/Player" + playerNo.ToString());
        }
    }

    void MakeRoom()
    {
        connector.MyReference.Child(roomName + "/" + playersDir).ValueChanged
            += (object sender, Firebase.Database.ValueChangedEventArgs args) =>
            {
                if (args.DatabaseError != null)
                {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                }

                playerNameArray = connector.GetChildrenValueString(args.Snapshot);
                nowPlayerCnt = (int)args.Snapshot.ChildrenCount;
                if (nowPlayerCnt >= 3)
                {
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                 roomName + "を用意しておいたぜ。参加者募集中。(現在" + nowPlayerCnt + "人)"));
                    textFielder.CleanInputField("Sでゲーム開始");
                    onReady = true;
                }
                else
                {
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                 roomName + "を用意しておいたぜ。参加者募集中。(現在" + nowPlayerCnt + "人)"));
                }

                Debug.Log(nowPlayerCnt);
            };
    }

    void EnterRoom()
    {
        Debug.Log("Entered");

        connector.MyReference.Child(roomName + "/RoomState").ValueChanged
             += (object sender, Firebase.Database.ValueChangedEventArgs args) =>
             {
                 if (args.DatabaseError != null)
                 {
                     Debug.LogError(args.DatabaseError.Message);
                     return;
                 }

                 if (args.Snapshot.GetRawJsonValue().Equals(
                     ((int)(RoomState.Playing)).ToString()))
                 {
                     stateNo = (int)KeeperState.SetupGameMaster;
                 }
             };

        connector.MyReference.Child(roomName + "/" + playersDir).ValueChanged
            += (object sender, Firebase.Database.ValueChangedEventArgs args) =>
            {
                if (args.DatabaseError != null)
                {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                }

                nowPlayerCnt = (int)args.Snapshot.ChildrenCount;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true,
            roomName + "に入ったぜ。ゲームが始まるまで待機だ。(現在" + nowPlayerCnt + "人)"));

                Debug.Log(nowPlayerCnt);
            };
    }
}

enum KeeperState
{
    NameReception = 0, RoomInquiry, RoomReception, Inquiry, Wanting, PlayWaiting,
    SetupGameMaster
}

enum RoomState
{
    Wanting, Playing,
}