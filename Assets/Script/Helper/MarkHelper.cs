using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class MarkHelper
{
    // Gender に対する拡張メソッドの定義
    public static string GetName(this MarkName markName)
    {
        if (Enum.IsDefined(typeof(MarkName), markName))
        {
            return Enum.GetName(typeof(MarkName), markName);
        }
        else
        {
            return "";
        }
    }

    public static string GetMarkName(this int markNo)
    {
        if (Enum.IsDefined(typeof(MarkName), markNo))
        {
            return Enum.GetName(typeof(MarkName), markNo);
        }
        else
        {
            return "";
        }
    }

    public static int GetLength(this MarkName markName)
    {
        return Enum.GetNames(typeof(MarkName)).Length;
    }

    public static int GetNo(this MarkName markName)
    {
        return (int)markName;
    }
}
