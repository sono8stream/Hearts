using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomKeeper : MonoBehaviour
{
    [SerializeField]
    GameMaster master;
    [SerializeField]
    TextFielder textFielder;

    const int nameMaxLength = 12;
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
                "ようこそ。早速だが、あなたの名前は何だい？({0}文字以内)", nameMaxLength)));
        textFielder.CleanInputField("名前を入力してね", "testPlayer");

        connector = new FirebaseConnector("Rooms");
        connector.MyReference.ChildRemoved
     += (object sender, Firebase.Database.ChildChangedEventArgs args) =>
     {
         if (stateNo != (int)KeeperState.Idle) return;

         InterruptGame();
     };
        stateNo = (int)KeeperState.NameReception;
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
                textFielder.CleanInputField("部屋名を入力してね", "TestRoom");
                break;

            case (int)KeeperState.RoomReception:
                roomName = textFielder.GetMessage();
                if (roomName == null) return;
                Debug.Log(roomName);
                connector.ReadQuery(roomName + "/" + playersDir, true);
                stateNo = (int)KeeperState.Inquiry;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                    "問い合わせ中..."));
                break;

            case (int)KeeperState.Inquiry:
                if (connector.SnapData == null) return;

                if (connector.SnapData.Value == null)
                {
                    MakeRoom();
                    playerNo = 0;
                    roomParent = true;
                    stateNo = (int)KeeperState.Wanting;
                }
                else if (connector.SnapData.ChildrenCount >= GameMaster.maxPlayers)
                {
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true, string.Format(
                        "おっと、そこは満席みたいだな。他の部屋はどうだい。({0}文字以内)",
                        nameMaxLength)));
                    textFielder.CleanInputField("部屋名を入力してね", roomName);
                    stateNo = (int)KeeperState.RoomReception;
                    connector.RemoveReadData();
                    return;
                }
                else
                {
                    EnterRoom();
                    playerNo = (int)connector.SnapData.ChildrenCount;
                    roomParent = false;
                    stateNo = (int)KeeperState.PlayWaiting;
                }

                connector.RemoveReadData();
                connector.AddAsync(roomName + "/" + playersDir + "/" + playerNo.ToString(),
                    playerName);
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
                if (playerNameArray.Length > GameMaster.maxPlayers)
                {
                    playerNameArray = playerNameArray.Take(GameMaster.maxPlayers).ToArray();
                }
                master.InitializeDB(nowPlayerCnt, roomParent, playerNameArray, playerNo,
                    connector.MyReference.Child(roomName + "/Master"));
                stateNo = (int)KeeperState.Idle;
                connector.MyReference.Child(roomName + "/" + playersDir).ValueChanged
             += (object sender, Firebase.Database.ValueChangedEventArgs args) =>
             {
                 if (stateNo != (int)KeeperState.Idle) return;

                 InterruptGame();
             };
                break;
        }
    }

    /// <summary>
    /// 部屋が丸々消えた場合、子の通信管理がかなり厄介なので検討中
    /// </summary>
    void OnApplicationQuit()
    {
        if (roomName == null) return;

        if (roomParent)
        {
            connector.RemoveItem(roomName);
        }
        else
        {
            connector.RemoveItem(roomName + "/" + playersDir + "/" + playerNo.ToString());
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

                if (stateNo != (int)KeeperState.Wanting) return;

                playerNameArray = connector.GetChildrenValueString(args.Snapshot);
                nowPlayerCnt = (int)args.Snapshot.ChildrenCount;
                if (nowPlayerCnt >= GameMaster.minPlayers)
                {
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                 roomName + "を用意しておいたぜ。参加者募集中だ。(現在" + nowPlayerCnt + "人)"));
                    textFielder.CleanInputField("Sでゲーム開始");
                    onReady = true;
                }
                else
                {
                    textFielder.StartCoroutine(textFielder.SwitchDialog(true,
                 roomName + "を用意しておいたぜ。参加者募集中だ。(現在" + nowPlayerCnt + "人)"));
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

                 if (stateNo != (int)KeeperState.PlayWaiting) return;

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

                playerNameArray = connector.GetChildrenValueString(args.Snapshot);
                nowPlayerCnt = (int)args.Snapshot.ChildrenCount;
                textFielder.StartCoroutine(textFielder.SwitchDialog(true,
            roomName + "に入ったぜ。ゲームが始まるまで待機だ。(現在" + nowPlayerCnt + "人)"));

                Debug.Log(nowPlayerCnt);
            };
    }

    void InterruptGame()
    {
        master.Initialize();
        stateNo = (int)KeeperState.RoomReception;
        textFielder.StartCoroutine(textFielder.SwitchDialog(true, string.Format(
                        "誰かがいなくなったみたいだな。他の部屋はどうだい。({0}文字以内)",
                        nameMaxLength)));
        textFielder.CleanInputField("部屋名を入力してね", "TestRoom");
        connector.RemoveItem(roomName);
    }
}

enum KeeperState
{
    NameReception = 0, RoomInquiry, RoomReception, Inquiry, Wanting, PlayWaiting,
    SetupGameMaster,Idle
}

enum RoomState
{
    Wanting, Playing,
}