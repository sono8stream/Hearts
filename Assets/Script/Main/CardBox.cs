using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class CardBox : MonoBehaviour
{
    public int Count { get { return cards.Count; } }
    public Card this[int i]
    {
        set { cards[i] = value; }
        get { return cards[i]; }
    }

    public bool autoSortable;
    public bool allFront = false;
    //public int width;
    //public int xLim=17;

    GameObject cardObjectOrigin;
    List<GameObject> cardObjects;
    List<Card> cards;

    private void Awake()
    {
        cardObjects = new List<GameObject>();
        cards = new List<Card>();
        cardObjectOrigin
            = GameObject.Find("Resource").GetComponent<ResourceLoader>().trumpOrigin;
    }

    private void Update()
    {

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

    public Dictionary<string,object> CardListDictionary()
    {
        Dictionary<string, object> cardMap = new Dictionary<string, object>();
        int cnt = Count;
        for (int i = 0; i < cnt; i++)
        {
            cardMap.Add(Card.dbName + i.ToString(), cards[i].DataDictionary());
        }

        return cardMap;
    }

    #region View Methods
    public void StackView()
    {
        int cardCnt = cardObjects.Count;
        Vector2 pos = Vector2.zero;
        float margine = 1;
        for (int i = 0; i < cardCnt; i++)
        {
            cardObjects[i].transform.localPosition = pos;
            cardObjects[i].transform.localEulerAngles = Vector3.zero;
            pos += Vector2.one * margine;
        }
    }

    public void ListView()
    {
        int cardCnt = cardObjects.Count;
        int width = allFront ? 30 : 10;
        int xLim = Count > 17 ? 17 : Count;
        Vector2 iniPos = Vector2.left * width * (xLim - 1) * 0.5f;
        Vector2 pos = iniPos;
        for (int i = 0; i < cardCnt; i++)
        {
            if (i % xLim == 0)
            {
                pos = iniPos + Vector2.down * 50 * (i / xLim);
            }
            cardObjects[i].transform.localPosition = pos;
            cardObjects[i].transform.localEulerAngles = Vector3.zero;
            pos += Vector2.right * width;
        }
    }

    public void EmphasizeOne(int index)
    {
        cardObjects[index].transform.localPosition += Vector3.up * 30;
    }

    public void Highlight(int[] lightIndexes)
    {
        int cardsCnt = Count;
        int lightCnt = lightIndexes.Length;
        int lightIterator = 0;

        for (int i = 0; i < cardsCnt; i++)
        {
            if (lightIterator < lightCnt
                && i == lightIndexes[lightIterator])
            {
                lightIterator++;
                cardObjects[i].GetComponent<Image>().color = Color.white;
            }
            else
            {
                cardObjects[i].GetComponent<Image>().color = Color.gray;
            }
        }
    }
    #endregion

    public void Turn(int index)
    {
        cards[index].frontFace = !cards[index].frontFace;
        cardObjects[index].GetComponent<Image>().sprite = cards[index].GetSprite();
    }

    public void TurnAll()
    {
        int cardCnt = cards.Count;
        for (int i = 0; i < cardCnt; i++)
        {
            Turn(i);
        }
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
        if (allFront)
        {
            c.frontFace = true;
        }
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
        GameObject.Find(SoundPlayer.objectName).GetComponent<SoundPlayer>().PlaySE(
            SEname.Turn);
    }

    #region Linq like Methods
    public int CountAll(System.Predicate<Card> pred)
    {
        int cnt = 0;
        int cardCnt = Count;
        for(int i = 0; i < Count; i++)
        {
            if (pred(cards[i]))
            {
                cnt++;
            }
        }
        return cnt;
    }
    
    /// <summary>
    /// 条件に合うものが無ければ、-1を返す
    /// </summary>
    /// <param name="pred">条件関数</param>
    /// <returns></returns>
    public List<int> IndexListWhere(System.Predicate<Card> pred)
    {
        List<int> targetIndexList = new List<int>();
        int cardCnt = Count;
        for (int i = 0; i < Count; i++)
        {
            if (pred(cards[i]))
            {
                targetIndexList.Add(i);
            }
        }
        return targetIndexList;
    }

    public void RemoveAll(System.Predicate<Card> pred)
    {
        int[] removeIndexList = IndexListWhere(pred).ToArray();
        int removeCount = removeIndexList.Length;
        for(int i = 0; i < removeCount; i++)
        {
            RemoveAt(removeIndexList[i] - i);
        }
    }
    #endregion
}