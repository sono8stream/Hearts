using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public int stateNo = (int)PlayerState.Idle;
    public CardBox handCards;

    [SerializeField]
    Text scoreText;

    GameMaster master;
    CardBox fieldBox;
    int selectIndex;
    int[] selectableIndexes;
    int totalScore;
    int nowGameScore;

    // Use this for initialization
    void Start()
    {
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        fieldBox = master.fieldCards;
    }

    // Update is called once per frame
    void Update()
    {
        switch (stateNo)
        {
            case (int)PlayerState.SetUp:
                break;
            case (int)PlayerState.BeginMyPhase:
                GetSelectableIndexes();
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
            return;
        }

        selectableIndexes = handCards.IndexListWhere((c) =>
        {
            return master.onHeartBreak || c.markNo != (int)MarkName.heart;
        }).ToArray();

        if (selectableIndexes.Length == 0)
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
            Debug.Log("Summon");
            return true;
        }
        return false;
    }

    void DiscardField(int index)
    {

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