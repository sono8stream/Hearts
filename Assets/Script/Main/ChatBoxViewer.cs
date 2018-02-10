using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBoxViewer : MonoBehaviour
{
    int currentPos;

    public void OnPointerEnter()//前面に移動
    {
        currentPos = transform.GetSiblingIndex();
        transform.SetSiblingIndex(transform.parent.childCount - 1);
    }

    public void OnPointerExit()//もとの位置に戻る
    {
        transform.SetSiblingIndex(currentPos);
    }
}
