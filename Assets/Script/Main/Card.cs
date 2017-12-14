using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
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
            return rl.trumpSprites[markNo * 13 + value - 1];
        }
        else
        {
            return rl.trumpReverseSprite;
        }
    }
}

public enum MarkName
{
    spade = 0, heart, diamond, club
}
