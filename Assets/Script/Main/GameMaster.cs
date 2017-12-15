using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    #region Member
    public CardBox fieldCards;

    const int minPlayers = 3;
    const int maxPlayers = 6;

    [SerializeField]
    CardBox deckCards;
    [SerializeField]
    CardBox talonCards;

    int turnCnt;
    int playerCnt = 5;
    int nowPlayerNo;
    float playerRadius = 400;
    List<GamePlayer> players;
    #endregion

    // Use this for initialization
    void Start()
    {
        SetUpGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!players[nowPlayerNo].onMyTurn)
        {
            nowPlayerNo = nowPlayerNo == playerCnt - 1 ? 0 : nowPlayerNo + 1;
            players[nowPlayerNo].onMyTurn = true;
        }
    }

    void SetUpGame()
    {
        AddTrumpSetToDeck();
        deckCards.RandomShuffle();
        deckCards.SyncCardObjects();
        deckCards.TurnAll();
        deckCards.ListView();
        InvitePlayers();
        ExcludeCards();
        ServeCards(false);
        for (int i = 0; i < playerCnt; i++)
        {
            players[i].handCards.ListView();
        }
    }

    void InvitePlayers()
    {
        players = new List<GamePlayer>();
        ResourceLoader rl
            = GameObject.Find("Resource").GetComponent<ResourceLoader>();
        Vector3[] playerPoses = SetPlayerPoses();

        for (int i = 0; i < playerCnt; i++)
        {
            GameObject g = Instantiate(rl.playerOrigin);
            g.transform.SetParent(transform.parent);

            g.transform.localPosition = (Vector2)playerPoses[i];
            g.transform.localEulerAngles = Vector3.forward * playerPoses[i].z;

            g.transform.localScale = Vector3.one;
            players.Add(g.GetComponent<GamePlayer>());
        }
    }

    Vector3[] SetPlayerPoses()
    {
        List<Vector3> poses = new List<Vector3>();
        poses.Add(new Vector3(0, -400, 0));
        poses.Add(new Vector3(800, 0, 90));
        poses.Add(new Vector3(-800, 0, 270));

        if (playerCnt >= 4)
        {
            poses.Insert(2, new Vector3(0, 400, 180));

            if (playerCnt >= 5)
            {
                poses[0] = new Vector3(-350, -400, 0);
                poses.Insert(1, new Vector3(350, -400, 0));

                if (playerCnt == maxPlayers)
                {
                    poses[3]=new Vector3(-350, 400, 180);
                    poses.Insert(3, new Vector3(350, 400, 180));
                }
            }
        }
        return poses.ToArray();
    }

    void AddTrumpSetToDeck()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j <= 13; j++)
            {
                deckCards.Add(new Card(i, j));
            }
        }
    }

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
                    return c.markNo == MarkName.club.No()
                    && c.value == 2;
                };
                break;
            case 5:
                pred = (c) =>
                {
                    return c.value == 2
                    && (c.markNo == MarkName.club.No() || c.markNo == MarkName.diamond.No());
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

    void ServeCards(bool randomSelect)
    {
        int index;
        while (deckCards.Count > 0)
        {
            for (int i = 0; i < playerCnt; i++)
            {
                index = randomSelect ? Random.Range(0, deckCards.Count) : 0;
                deckCards.MoveTo(ref players[i].handCards, index);
            }
        }
    }
}