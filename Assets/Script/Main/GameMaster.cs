using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    #region Member
    public CardBox fieldCards;
    public bool onHeartBreak;

    const int minPlayers = 3;
    const int maxPlayers = 6;

    [SerializeField]
    CardBox deckCards;
    [SerializeField]
    CardBox talonCards;

    int turnCnt;
    int playerCnt = 5;
    int nowPlayerNo;
    int lastPlayerNo;
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
        if (players[nowPlayerNo].stateNo == (int)PlayerState.Idle)
        {
            if (!onHeartBreak && fieldCards.Count > 0
                && fieldCards[fieldCards.Count - 1].markNo == (int)MarkName.heart)
            {
                onHeartBreak = true;
            }

            if (nowPlayerNo == lastPlayerNo)
            {
                EndTurn();
            }
            else
            {
                nowPlayerNo = nowPlayerNo == playerCnt - 1 ? 0 : nowPlayerNo + 1;
            }

            players[nowPlayerNo].stateNo = (int)PlayerState.BeginMyPhase;
            Debug.Log(nowPlayerNo);
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
        players[nowPlayerNo].stateNo = (int)PlayerState.BeginMyPhase;
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

        lastPlayerNo = playerCnt - 1;
        nowPlayerNo = 0;
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
                    poses[3] = new Vector3(-350, 400, 180);
                    poses.Insert(3, new Vector3(350, 400, 180));
                }
            }
        }
        return poses.ToArray();
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
    #endregion

    void EndTurn()
    {
        nowPlayerNo = TurnWinnerIndex();
        players[nowPlayerNo].AddScore(GivenScore(nowPlayerNo));
        lastPlayerNo = nowPlayerNo == 0 ? playerCnt - 1 : nowPlayerNo - 1;
        DiscardFieldCards();
        Debug.Log("Discard");
    }

    int TurnWinnerIndex()
    {
        int index = 0;
        int markNo = fieldCards[0].markNo;
        int val = fieldCards[0].value;
        int fieldCardsCnt = fieldCards.Count;
        for (int i = 1; i < fieldCardsCnt; i++)
        {
            if (markNo == fieldCards[i].markNo
                && val < fieldCards[i].value)
            {
                index = i;
                val = fieldCards[i].value;
            }
        }
        return index;
    }

    int GivenScore(int winnerIndex)
    {
        int score = 0;
        score += fieldCards.CountAll((c) => { return c.markNo == (int)MarkName.heart; });

        if (fieldCards.IndexListWhere(
            (c) => { return c.IsMatch((int)MarkName.spade, 12); }).Count > 0)
        {
            score += 12;
        }

        return score;
    }

    void DiscardFieldCards()
    {
        int fieldCardsCnt = fieldCards.Count;
        for (int i = 0; i < fieldCardsCnt; i++)
        {
            fieldCards.MoveTo(ref talonCards, 0);
        }
    }
}