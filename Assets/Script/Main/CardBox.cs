﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBox : MonoBehaviour
{
    public int Count { get { return cards.Count; } }
    public Card this[int i]
    {
        set { cards[i] = value; }
        get { return cards[i]; }
    }

    public bool autoSortable;

    GameObject cardObjectOrigin;
    List<GameObject> cardObjects;
    List<Card> cards;
    NetworkAdaptor adaptor;

    private void Awake()
    {
        cardObjects = new List<GameObject>();
        cards = new List<Card>();
        adaptor = new NetworkAdaptor();
        cardObjectOrigin
            = GameObject.Find("Resource").GetComponent<ResourceLoader>().trumpOrigin;
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            SpeedTester.Test(() => { RealShuffle(50); }, () => { RandomShuffle(); });
        }*/
    }

    public void SyncCardObjects()
    {
        DeleteCardObjects();
        cardObjects = new List<GameObject>();
        GenerateCardObjects();
    }

    void DeleteCardObjects()
    {
        int cardCnt = cardObjects.Count;
        for (int i = 0; i < cardCnt; i++)
        {
            Destroy(cardObjects[0]);
            cardObjects.RemoveAt(0);
        }
    }

    void GenerateCardObjects()
    {
        int cardCnt = cards.Count;
        for (int i = 0; i < cardCnt; i++)
        {
            AddCardObject();
        }
    }

    public void StackView()
    {
        int cardCnt = cardObjects.Count;
        Vector2 pos = Vector2.zero;
        float margine = 1;
        for (int i = 0; i < cardCnt; i++)
        {
            cardObjects[i].GetComponent<RectTransform>().anchoredPosition = pos;
            pos += Vector2.one * margine;
        }
    }

    public void ListView()
    {
        int cardCnt = cardObjects.Count;
        Vector2 pos = Vector2.zero;
        int height = 50;
        int yLim = 13;
        for (int i = 0; i < cardCnt; i++)
        {
            if (i % yLim == 0) { pos = Vector2.right * 220 * (i / yLim); }
            cardObjects[i].GetComponent<RectTransform>().anchoredPosition = pos;
            pos += Vector2.down * height;
        }
        Debug.Log("Listed");
    }

    /// <summary>
    /// 範囲をランダムに選択して並べ替える、リアルなシャッフル
    /// ベンチマーク完敗
    /// </summary>
    /// <param name="shuffleLim"></param>
    public void RealShuffle(int shuffleLim)
    {
        if (cards.Count <= 1) return;

        int shuffleIndex, shuffleCount;
        List<Card> tempCards;
        for (int i = 0; i < shuffleLim; i++)
        {
            shuffleIndex = Random.Range(0, cards.Count);
            shuffleCount = Random.Range(1, cards.Count - shuffleIndex + 1);

            tempCards = cards.GetRange(shuffleIndex, shuffleCount);
            cards.RemoveRange(shuffleIndex, shuffleCount);
            cards.InsertRange(0, tempCards);
        }
        ListView();
        Debug.Log("shuffled");
    }

    /// <summary>
    /// インデックスをランダムに並べ替える機械シャッフル
    /// ベンチマーク60倍くらい速くて圧勝
    /// </summary>
    public void RandomShuffle()
    {
        List<int> indexList = new List<int>();
        int cardCnt = cards.Count;
        for (int i = 0; i < cardCnt; i++)
        {
            indexList.Add(i);
        }
        List<int> indexTempList = new List<int>();
        int index;
        for (int i = 0; i < cardCnt; i++)
        {
            index = Random.Range(0, cardCnt - i);
            indexTempList.Add(indexList[index]);
            indexList.RemoveAt(index);
        }
        List<Card> cardsTemp = new List<Card>();
        for (int i = 0; i < cardCnt; i++)
        {
            cardsTemp.Add(cards[indexTempList[i]]);
        }
        cards = cardsTemp;
    }

    #region ListMethods
    public void Add(Card c)
    {
        cards.Add(c);
        AddCardObject();

        if (autoSortable) SortLastCard();
    }

    public void AddCardObject()
    {
        GameObject g = Instantiate(cardObjectOrigin);
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;
        g.transform.localScale = Vector3.one;
        g.GetComponent<Image>().sprite = cards[cards.Count - 1].GetSprite();
        cardObjects.Add(g);
    }

    public void RemoveAt(int index)
    {
        cards.RemoveAt(index);
        Destroy(cardObjects[index]);
        cardObjects.RemoveAt(index);

    }
    #endregion

    /// <summary>
    /// ソート方式はマーク>値
    /// </summary>
    void SortLastCard()
    {
        if (cards.Count < 2) return;

        int checkIndex = 0;
        int lastIndex = cards.Count - 1;
        while (checkIndex <lastIndex)
        {
            if (cards[checkIndex].markNo < cards[lastIndex].markNo
                || (cards[checkIndex].markNo == cards[lastIndex].markNo
                && cards[checkIndex].value < cards[lastIndex].value))
            {
                checkIndex++;
            }
            else
            {
                cards.Insert(checkIndex, cards[lastIndex]);
                cards.RemoveAt(lastIndex + 1);
                cardObjects.Insert(checkIndex, cardObjects[lastIndex]);
                cardObjects.RemoveAt(lastIndex + 1);

                SortHierarchy();
                return;
            }
        }
    }

    void Swap(int index1, int index2)
    {
        Card cardTemp = cards[index2];
        GameObject objectTemp = cardObjects[index2];

        cards[index2] = cards[index1];
        cardObjects[index2] = cardObjects[index1];
        cards[index1] = cardTemp;
        cardObjects[index1] = objectTemp;
    }

    void SortHierarchy()
    {
        int objectCnt = cardObjects.Count;
        for(int i = 0; i < objectCnt; i++)
        {
            cardObjects[i].transform.SetSiblingIndex(i);
        }
    }

    public void MoveTo(ref CardBox box, int cardIndex)
    {
        box.Add(cards[cardIndex]);
        RemoveAt(cardIndex);
    }
}