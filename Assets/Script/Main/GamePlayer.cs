using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    public bool onMyTurn;

    [SerializeField]
    CardBox handCards;

    CardBox fieldBox;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!onMyTurn) return;


    }

    public void SetCard(Card c)
    {
        handCards.Add(c);
    }
}