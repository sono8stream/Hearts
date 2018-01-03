using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public int myNo;
    public int stateNo = (int)PlayerState.Idle;
    public CardBox handCards;

    [SerializeField]
    Text scoreText;
    [SerializeField]
    GameMaster master;

    CardBox fieldBox;
    int selectIndex;
    int[] selectableIndexes;
    int totalScore;
    int nowGameScore;
    FirebaseConnector handDB;

    // Use this for initialization
    void Start()
    {
        fieldBox = master.fieldCards;
        stateNo = (int)PlayerState.Idle;

        /*FirebaseConnector con = new FirebaseConnector("player0");
        con.GetValueDictionary(
            "{\"hand1\":{\"card\":\"value1\"},\"hand2\":{\"card\":\"value2\"},"
            + "\"hand3\":{\"card1\":{\"card2\":\"value3\"}},\"name\":\"sono\"}");*/
    }

    // Update is called once per frame
    void Update()
    {
        switch (stateNo)
        {
            case (int)PlayerState.SetUp:
                PrepareNewPlay();
                stateNo = (int)PlayerState.Idle;
                break;
            case (int)PlayerState.BeginMyPhase:
                GetSelectableIndexes();
                handCards.Highlight(selectableIndexes);
                handCards.EmphasizeOne(selectableIndexes[selectIndex]);
                stateNo = (int)PlayerState.MyPhase;
                break;
            case (int)PlayerState.MyPhase:
                if (SelectCard()) stateNo = (int)PlayerState.EndMyPhase;
                break;
            case (int)PlayerState.EndMyPhase:
                selectIndex = 0;
                stateNo = (int)PlayerState.Idle;
                handCards.ListView();
                break;
            case (int)PlayerState.Idle:

                break;
        }
    }

    public void InitializeDB(int no,Firebase.Database.DatabaseReference handReference)
    {
        myNo = no;
        handDB = new FirebaseConnector(handReference);
        handDB.AddAsync("name", "hoge");
    }

    void PrepareNewPlay()
    {
        handCards.ListView();
        UpdateHandDB();
    }

    void GetSelectableIndexes()
    {
        if (fieldBox.Count == 0)
        {
            selectableIndexes = handCards.IndexListWhere((c) =>
            {
                return master.onHeartBreak || c.markNo != (int)MarkName.heart;
            }).ToArray();
            return;
        }

        List<int> tempIndexList = handCards.IndexListWhere((c) =>
        {
            return c.markNo == fieldBox[0].markNo;
        });
        if (tempIndexList.Count > 0)
        {
            selectableIndexes = tempIndexList.ToArray();
        }
        else
        {
            selectableIndexes = handCards.IndexListWhere((c) => { return true; }).ToArray();
        }
    }

    bool SelectCard()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            handCards.ListView();
            selectIndex = selectIndex == selectableIndexes.Length - 1 ? 0 : selectIndex + 1;
            handCards.EmphasizeOne(selectableIndexes[selectIndex]);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            handCards.ListView();
            selectIndex = selectIndex == 0 ? selectableIndexes.Length - 1 : selectIndex - 1;
            handCards.EmphasizeOne(selectableIndexes[selectIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            handCards.MoveTo(ref fieldBox, selectableIndexes[selectIndex]);
            fieldBox.ListView();
            UpdateHandDB();
            Debug.Log("Summon");
            return true;
        }
        return false;
    }

    void UpdateHandDB()
    {
        int dataCnt = handDB.dataMap.Count;
        int handCnt = handCards.Count;
        for (int i = 0; i < dataCnt || i < handCnt; i++)
        {
            if (i >= handCnt)
            {
                handDB.RemoveItem("card" + i.ToString());
            }
            else
            {
                Dictionary<string, object> itemMap = new Dictionary<string, object>();
                itemMap.Add("markNo", handCards[i].markNo);
                itemMap.Add("value", handCards[i].value);

                if(i>=dataCnt)
                {
                    handDB.dataMap.Add("card" + i.ToString(), itemMap);
                }
                else
                {
                    handDB.dataMap["card" + i.ToString()] = itemMap;
                }
            }
        }

        handDB.AsyncMap();
    }

    public void AddScore(int point)
    {
        nowGameScore += point;
        scoreText.text = nowGameScore.ToString();
    }
}

enum PlayerState
{
    SetUp = 0, BeginMyPhase, MyPhase, EndMyPhase, Idle
}