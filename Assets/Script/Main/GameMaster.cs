using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    #region Member
    public CardBox fieldCards;

    const int minPlayers = 4;
    const int maxPlayers = 6;

    [SerializeField]
    CardBox deckCards;
    [SerializeField]
    CardBox talonCards;

    int turnCnt;
    int playerCnt = 4;
    int nowPlayerNo;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            deckCards.RandomShuffle();
            deckCards.SyncCardObjects();
            deckCards.ListView();
        }
    }

    void SetUpGame()
    {
        players = new List<GamePlayer>();
        AddTrumpSetToDeck();
        deckCards.RandomShuffle();
        deckCards.SyncCardObjects();
        deckCards.StackView();
        InvitePlayers();
    }

    void InvitePlayers()
    {
        ResourceLoader rl = GameObject.Find("Resource").GetComponent<ResourceLoader>();
        Debug.Log(rl);
        
        for(int i = 0; i < playerCnt; i++)
        {
            GameObject g = Instantiate(rl.playerOrigin);
            g.transform.SetParent(transform.parent);
            g.transform.localPosition = Vector3.zero;
            g.transform.localScale = Vector3.one;
        }
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
            + deckCards[0].markNo.GetMarkName() + deckCards[0].value.ToString();
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
}