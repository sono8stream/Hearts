using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public const string handDBname = "handCards";
    public const string selectDBname = "selectCardIndex";
    public const string queryCountDBname = "queryCounter";

    public int myNo;
    public int stateNo = (int)PlayerState.Idle;
    public CardBox handCards;
    public string myName;
    public GameMaster master;
    public bool isPlayable;
    public FirebaseConnector myDB;

    [SerializeField]
    Text scoreText;

    CardBox fieldBox;
    int selectIndex;
    int[] selectableIndexes;
    int totalScore;
    int nowGameScore;

    // Use this for initialization
    void Start()
    {
        fieldBox = master.fieldCards;
        //stateNo = (int)PlayerState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        //Firebase.Database.DataSnapshot snapshot = null;
        switch (stateNo)
        {
            case (int)PlayerState.SetUp:

                //myDB.GetReadData(ref snapshot);
                if (myDB.SnapData == null) return;

                DownloadMyHandDB(myDB.SnapData);
                myDB.RemoveReadData();
                if (handCards.Count == 0)
                {
                    myDB.ReadQuery(handDBname);
                    return;
                }
                Debug.Log("read!");

                PrepareNewPlay();
                stateNo = (int)PlayerState.Idle;
                break;
            case (int)PlayerState.BeginMyPhase:
                GetSelectableIndexes();
                handCards.Highlight(selectableIndexes);
                handCards.EmphasizeOne(selectableIndexes[selectIndex]);
                stateNo = (int)PlayerState.MyPhase;
                if (!isPlayable)
                {
                    myDB.ReadQuery(selectDBname);
                }
                break;
            case (int)PlayerState.MyPhase:
                if (isPlayable)
                {
                    if (SelectCard())
                    {
                        stateNo = (int)PlayerState.EndMyPhase;
                        myDB.AddAsync(selectDBname, selectableIndexes[selectIndex]);
                        myDB.Push(queryCountDBname, master.ClientNo.ToString());
                        myDB.ReadQuery(queryCountDBname);
                    }
                }
                else
                {
                    if (myDB.SnapData == null) return;
                    else if (int.Parse(myDB.SnapData.GetRawJsonValue()) == -1)
                    {
                        myDB.ReadQuery(selectDBname);
                    }
                    else
                    {
                        handCards.MoveTo(ref fieldBox,
                            int.Parse(myDB.SnapData.GetRawJsonValue()));
                        fieldBox.ListView();
                        myDB.Push(queryCountDBname, master.ClientNo.ToString());
                        stateNo = (int)PlayerState.Idle;
                    }
                    myDB.RemoveReadData();
                }
                break;
            case (int)PlayerState.EndMyPhase:
                //return;
                if (myDB.SnapData == null) return;
                else if (myDB.SnapData.ChildrenCount < master.PlayerCnt)
                {
                    myDB.ReadQuery(queryCountDBname);
                }
                else
                {
                    selectIndex = 0;
                    Debug.Log("idle");
                    stateNo = (int)PlayerState.Idle;
                    handCards.ListView();
                    myDB.AddAsync(selectDBname, -1);
                    Debug.Log(myDB.SnapData.ChildrenCount);
                    myDB.RemoveItem(queryCountDBname);
                }
                myDB.RemoveReadData();
                break;
            case (int)PlayerState.Idle:

                break;
        }
    }

    public void InitializeDB(int no, bool playable, string name,
        Firebase.Database.DatabaseReference playerReference)
    {
        myNo = no;
        isPlayable = playable;
        myName = name;
        myDB = new FirebaseConnector(playerReference, !playable);
        if (isPlayable)
        {
            Debug.Log("playable!");
            myDB.AddAsync("Name", myName);
            myDB.AddAsync(selectDBname, -1);
            //myDB.AddAsync(queryCountDBname,)
        }
    }

    void PrepareNewPlay()
    {
        handCards.ListView();
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
            UploadMyHandDB();
            Debug.Log("Summon");
            return true;
        }
        return false;
    }

    void DownloadMyHandDB(Firebase.Database.DataSnapshot handSnap)
    {
        int handCnt = (int)handSnap.ChildrenCount;
        handCards.RemoveAll(x => true);
        for (int i = 0; i < handCnt; i++)
        {
            Firebase.Database.DataSnapshot cardRef
                    = handSnap.Child(Card.dbName + i.ToString());
            int markNo = int.Parse(cardRef.Child("markNo").GetRawJsonValue());
            int value = int.Parse(cardRef.Child("value").GetRawJsonValue());
            //Debug.Log(string.Format("{0}:{1}", markNo, value));
            handCards.Add(new Card(markNo, value));
        }
        handCards.SyncCardObjects();
        handCards.TurnAll();
    }

    void UploadMyHandDB()
    {
        /*Dictionary<string, object> handMap = new Dictionary<string, object>();
        int handCnt = handCards.Count;
        for (int i = 0;i < handCnt; i++)
        {
            Dictionary<string, object> itemMap = new Dictionary<string, object>();
            itemMap.Add("markNo", handCards[i].markNo);
            itemMap.Add("value", handCards[i].value);
            handMap.Add("card" + i.ToString(), itemMap);
        }*/

        myDB.AddAsync(handDBname,/*handMap*/handCards.CardListDictionary());
        //myDB.AsyncMap();
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