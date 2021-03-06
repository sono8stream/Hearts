﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public const string dbName = "card";

    public int markNo;
    public int value;
    public bool frontFace;

    public Card(int mNo, int v)
    {
        markNo = mNo;
        value = v;
        frontFace = false;
    }

    public Sprite GetSprite()
    {
        ResourceLoader rl = GameObject.Find("Resource").GetComponent<ResourceLoader>();
        if (frontFace)
        {
            int valTemp = value == 14 ? 0 : value-1;//aceかどうか
            return rl.trumpSprites[markNo * 13 + valTemp];
        }
        else
        {
            return rl.trumpReverseSprite;
        }
    }

    public bool IsMatch(int mNo,int val)
    {
        return markNo == mNo && value == val;
    }

    public Dictionary<string,object> DataDictionary()
    {
        Dictionary<string, object> itemMap = new Dictionary<string, object>();
        itemMap.Add("markNo", markNo);
        itemMap.Add("value", value);
        return itemMap;
    }
}

public enum MarkName
{
    spade = 0, heart, diamond, club
}
