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
    public CardBox heartBox;
    public string myName;
    public GameMaster master;
    public bool isPlayable;
    public FirebaseConnector myDB;

    [SerializeField]
    Text scoreText;
    [SerializeField]
    Text nameText;

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
                
                if (myDB.SnapData == null) return;

                handCards.allFront = isPlayable;
                heartBox.allFront = true;
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
                if (isPlayable)
                {
                    GetSelectableIndexes();
                    handCards.Highlight(selectableIndexes);
                    handCards.EmphasizeOne(selectableIndexes[selectIndex]);
                }
                else
                {
                    myDB.ReadQuery(selectDBname);
                }
                stateNo = (int)PlayerState.MyPhase;
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
                    if (int.Parse(myDB.SnapData.GetRawJsonValue()) == -1)
                    {
                        myDB.ReadQuery(selectDBname);
                    }
                    else
                    {
                        stateNo = (int)PlayerState.EndMyPhase;
                        handCards.MoveTo(ref fieldBox,
                            int.Parse(myDB.SnapData.GetRawJsonValue()));
                        fieldBox.ListView();
                        handCards.ListView();
                        myDB.Push(queryCountDBname, master.ClientNo.ToString());
                        myDB.ReadQuery(selectDBname);
                    }
                    myDB.RemoveReadData();
                }
                break;
            case (int)PlayerState.EndMyPhase:
                //return;
                if (myDB.SnapData == null) return;
                if (isPlayable)
                {
                    if (myDB.SnapData.ChildrenCount < master.PlayerCnt)
                    {
                        myDB.ReadQuery(queryCountDBname);
                    }
                    else
                    {
                        selectIndex = 0;
                        Debug.Log("idle");
                        stateNo = (int)PlayerState.Idle;
                        myDB.AddAsync(selectDBname, -1);
                        Debug.Log(myDB.SnapData.ChildrenCount);
                    }
                }
                else
                {
                    if (int.Parse(myDB.SnapData.GetRawJsonValue()) != -1)
                    {
                        myDB.ReadQuery(selectDBname);
                    }
                    else
                    {
                        stateNo = (int)PlayerState.Idle;
                    }
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
        nameText.text = name;
        if (isPlayable)
        {
            Debug.Log("playable!");
            myDB.AddAsync("Name", myName);
            myDB.AddAsync(selectDBname, -1);
        }
        transform.Find("Chatter").GetComponent<Chatter>().InitializeChatDB(
            playerReference.Child("Chat"), playable);
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
            handCards.Turn(selectableIndexes[selectIndex]);
            handCards.MoveTo(ref fieldBox, selectableIndexes[selectIndex]);
            fieldBox.ListView();
            handCards.ListView();
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
        //handCards.SyncCardObjects();
    }

    void UploadMyHandDB()
    {
        myDB.AddAsync(handDBname,handCards.CardListDictionary());
    }

    //scoreが25でtrue、shoot the moon
    public bool CalculateScore()
    {
        int score = heartBox.CountAll((c) => { return c.markNo == (int)MarkName.heart; });
        if (heartBox.IndexListWhere(
            (c) => { return c.IsMatch((int)MarkName.spade, 12); }).Count > 0)
        {
            score += 12;
        }
        heartBox.RemoveAll((x) => true);
        if (score == 25)
        {//Shoot the moon
            return true;
        }
        else
        {
            AddScore(score);
            return false;
        }
    }

    public void AddScore(int score)
    {
        nowGameScore += score;
        scoreText.text = nowGameScore.ToString();
    }
}

enum PlayerState
{
    SetUp = 0, BeginMyPhase, MyPhase, EndMyPhase, Idle
}