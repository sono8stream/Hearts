﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    #region Member
    public CardBox fieldCards;
    public bool onHeartBreak;

    public const int minPlayers = 3;
    public const int maxPlayers = 4;

    [SerializeField]
    CardBox deckCards;
    [SerializeField]
    CardBox talonCards;

    int trickCnt;
    int trickLimit;
    int playerCnt = 0;
    public int PlayerCnt { get { return playerCnt; } }
    int nowPlayerNo;
    int lastPlayerNo;
    int playCnt;
    int stateNo;
    int clientNo;
    public int ClientNo { get { return clientNo; } }
    bool isParent;
    public bool IsParent { get { return isParent; } }

    public List<GamePlayer> players;
    FirebaseConnector connector;
    #endregion

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        switch (stateNo)
        {
            case (int)MasterState.SetUp:
                if (players[nowPlayerNo].stateNo == (int)PlayerState.SetUp) return;
                stateNo = (int)MasterState.TrickBegin;
                break;

            case (int)MasterState.TrickBegin:
                players[nowPlayerNo].stateNo = (int)PlayerState.BeginMyPhase;
                stateNo = (int)MasterState.OnTrick;
                break;

            case (int)MasterState.OnTrick:
                if (players[nowPlayerNo].stateNo != (int)PlayerState.Idle) return;

                if (!onHeartBreak && fieldCards.Count > 0
                    && fieldCards[fieldCards.Count - 1].markNo == (int)MarkName.heart)
                {
                    onHeartBreak = true;
                }
                if (nowPlayerNo == lastPlayerNo)
                {
                    stateNo = (int)MasterState.TrickEnd;
                }
                else
                {
                    Debug.Log("nextPlayer");
                    nowPlayerNo = nowPlayerNo == playerCnt - 1 ? 0 : nowPlayerNo + 1;
                    players[nowPlayerNo].stateNo = (int)PlayerState.BeginMyPhase;
                }
                break;

            case (int)MasterState.TrickEnd:
                EndTrick();
                Debug.Log(trickLimit);
                trickCnt++;
                if (trickCnt >= trickLimit)
                {
                    int shootedScore = 0;
                    for (int i = 0; i < playerCnt; i++)
                    {
                        if (players[i].CalculateScore()) { shootedScore = 26; }
                        else { players[i].AddScore(shootedScore); }
                    }
                    NewPlay();
                }
                else
                {
                    stateNo = (int)MasterState.TrickBegin;
                }
                break;
        }
    }

    #region Test Methods
    public void ClickTest(Text testText)
    {
        if (deckCards.Count == 0) return;

        testText.text = (52 - deckCards.Count).ToString()
            + deckCards[0].markNo.Name<MarkName>() + deckCards[0].value.ToString();
        Debug.Log(testText.text);
        deckCards.RemoveAt(0);
    }

    public void TurnDeck()
    {
        if (deckCards.Count == 0) return;

        int deckCnt = deckCards.Count;
        deckCards[deckCnt - 1].frontFace = true;
        deckCards.MoveTo(ref fieldCards, deckCnt - 1);
        fieldCards.ListView();
    }
    #endregion

    public void InitializeDB(int playerCount, bool isParent, string[] nameArray, int clientNo,
        Firebase.Database.DatabaseReference masterReference)
    {
        playerCnt = playerCount;
        this.isParent = isParent;
        connector = new FirebaseConnector(masterReference, isParent);
        Debug.Log("connected");
        InvitePlayers(nameArray, clientNo);
        this.clientNo = clientNo;
        NewPlay();
        GameObject.Find(SoundPlayer.objectName).GetComponent<SoundPlayer>().PlayBGM(
            BGMname.Main);
    }

    void NewPlay()
    {
        trickLimit = 52 / playerCnt;
        trickCnt = 0;
        if (isParent)
        {
            AddTrumpSetToDeck();
            deckCards.SyncCardObjects();
            //deckCards.TurnAll();
            ExcludeCards();
            ServeCards(true);
        }

        for (int i = 0; i < playerCnt; i++)
        {
            players[i].stateNo = (int)PlayerState.SetUp;
            players[i].myDB.ReadQuery(GamePlayer.handDBname);
        }
        nowPlayerNo = (playerCnt - clientNo) % playerCnt;
        lastPlayerNo = nowPlayerNo == 0 ? playerCnt - 1 : nowPlayerNo - 1;
        stateNo = (int)MasterState.SetUp;
        fieldCards.allFront = true;
        talonCards.allFront = true;
    }

    #region Setup Methods
    void AddTrumpSetToDeck()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 2; j <= 14; j++)
            {
                deckCards.Add(new Card(i, j));
            }
        }
    }

    /// <summary>
    /// ハーツのルールにあわせて山札を間引き
    /// </summary>
    void ExcludeCards()
    {
        System.Predicate<Card> pred = (c) => { return false; };

        switch (playerCnt)
        {
            case 3:
                pred = (c) =>
                {
                    return c.markNo == (int)MarkName.club
                    && c.value == 2;
                };
                break;
            case 5:
                pred = (c) =>
                {
                    return c.value == 2
                    && (c.markNo == (int)MarkName.club || c.markNo == (int)MarkName.diamond);
                };
                break;
            case 6:
                pred = (c) => { return c.value == 2; };
                break;
            default:
                return;
        }
        deckCards.RemoveAll(pred);
    }

    /// <summary>
    /// used only when ofline
    /// </summary>
    void InvitePlayers(string[] nameArray, int clientNo)
    {
        Debug.Log(clientNo);
        players = new List<GamePlayer>();
        ResourceLoader rl
            = GameObject.Find("Resource").GetComponent<ResourceLoader>();
        Vector3[] playerPoses = SetPlayerPoses();

        for (int i = 0; i < playerCnt; i++)
        {
            int playerNo = (i + clientNo) % playerCnt;
            GameObject g = Instantiate(rl.playerOrigin);
            g.transform.SetParent(transform.parent);

            g.transform.localPosition = (Vector2)playerPoses[i];
            
            if (playerPoses[i].z == 90)//boxオブジェクトを下に
            {
                g.transform.Find("Box").localPosition = Vector3.down * 240;
            }
            else if (playerPoses[i].z == 180)//chatterオブジェクトを下に
            {
                g.transform.Find("Box").localPosition += Vector3.down * 150;
                g.transform.Find("Box").localPosition += Vector3.left * 50;
                g.transform.Find("Chatter").localPosition = Vector3.down * 210;

            }
            g.transform.localScale = Vector3.one;
            GamePlayer player = g.GetComponent<GamePlayer>();

            player.myNo = playerNo;
            player.master = this;
            player.InitializeDB(playerNo, playerNo == clientNo, nameArray[playerNo],
                    connector.MyReference.Parent.Child(
                        string.Format("Player{0}", playerNo.ToString())));
            players.Add(player);
        }
    }

    Vector3[] SetPlayerPoses()
    {
        List<Vector3> poses = new List<Vector3>();
        poses.Add(new Vector3(-200, -320, 0));
        poses.Add(new Vector3(700, 150, 90));
        poses.Add(new Vector3(-700, 150, 90));

        if (playerCnt >= 4)
        {
            poses.Insert(2, new Vector3(-200, 550, 180));

            if (playerCnt >= 5)
            {
                poses[0] = new Vector3(-650, -320, 0);
                poses.Insert(1, new Vector3(310, -250, 0));

                if (playerCnt == maxPlayers)
                {
                    poses[3] = new Vector3(-650, 550, 180);
                    poses.Insert(3, new Vector3(310, 550, 180));
                }
            }
        }
        return poses.ToArray();
    }

    void ServeCards(bool randomSelect)
    {
        FirebaseConnector tempConnector = new FirebaseConnector(connector.MyReference.Parent);
        trickLimit = deckCards.Count / playerCnt;
        for (int i = 0; i < playerCnt; i++)
        {
            Dictionary<string, object> cardDict = new Dictionary<string, object>();
            for (int j = 0; j < trickLimit; j++)
            {
                int index = Random.Range(0, deckCards.Count);
                cardDict.Add(Card.dbName + j.ToString(), deckCards[index].DataDictionary());
                deckCards.RemoveAt(index);
            }

            tempConnector.AddAsync(string.Format("Player{0}/{1}", i, GamePlayer.handDBname),
                cardDict);
        }
    }
    #endregion

    void EndTrick()
    {
        nowPlayerNo = TrickWinnerIndex();
        GiveHearts(nowPlayerNo);
        //lastPlayerNo = (nowPlayerNo + playerCnt - 1) % playerCnt;
        lastPlayerNo = nowPlayerNo == 0 ? playerCnt - 1 : nowPlayerNo - 1;
        DiscardFieldCards();
    }

    int TrickWinnerIndex()
    {
        int index = 0;
        int markNo = fieldCards[0].markNo;
        int val = fieldCards[0].value;
        int fieldCardsCnt = fieldCards.Count;
        int firstPlayerNo = (lastPlayerNo + 1) % playerCnt;

        for (int i = 1; i < fieldCardsCnt; i++)
        {
            if (markNo == fieldCards[i].markNo
                && val < fieldCards[i].value)
            {
                index = i;
                val = fieldCards[i].value;
            }
        }
        return (firstPlayerNo + index) % playerCnt;
    }

    void GiveHearts(int winnerIndex)
    {
        List<int> givenCardIndexes = fieldCards.IndexListWhere(
            (c) =>
            {
                return c.IsMatch((int)MarkName.spade, 12)
           || c.markNo == (int)MarkName.heart;
            });
        int givenCnt = givenCardIndexes.Count;

        for (int i = 0; i < givenCnt; i++)
        {
            fieldCards.MoveTo(ref players[winnerIndex].heartBox, givenCardIndexes[i] - i);
        }
        players[winnerIndex].heartBox.ListView();
    }

    void DiscardFieldCards()
    {
        int fieldCardsCnt = fieldCards.Count;
        for (int i = 0; i < fieldCardsCnt; i++)
        {
            fieldCards.MoveTo(ref talonCards, 0);
        }
    }

    public void Initialize()
    {
        for (int i = 0; i < playerCnt; i++)
        {
            Destroy(players[i].gameObject);
        }
        playerCnt = 0;
        playCnt = 0;
        stateNo = (int)MasterState.Idle;
    }
}

public enum MasterState
{
    SetUp = 0, TrickBegin,OnTrick,TrickEnd,Idle
}