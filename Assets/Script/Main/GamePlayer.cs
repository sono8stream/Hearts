using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    public bool onMyTurn;
    public CardBox handCards;

    CardBox fieldBox;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!onMyTurn) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndMyTurn();
        }
    }

    public void SetCard(Card c)
    {
        handCards.Add(c);
    }

    void DiscardField()
    {

    }

    void EndMyTurn()
    {
        onMyTurn = false;
    }
}

enum PlayerState
{

}