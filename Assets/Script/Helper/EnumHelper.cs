using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class EnumHelper
{
    // Gender に対する拡張メソッドの定義
    public static string GetName<Type>(this Type type) where Type : struct
    {
        if (Enum.IsDefined(typeof(Type), type))
        {
            return Enum.GetName(typeof(Type), type);
        }
        else
        {
            return "";
        }
    }

    public static string Name<Type>(this int markNo) where Type : struct
    {
        if (Enum.IsDefined(typeof(Type), markNo))
        {
            return Enum.GetName(typeof(Type), markNo);
        }
        else
        {
            return "";
        }
    }

    public static int GetLength<Type>(this Type type) where Type : struct
    {
        return Enum.GetNames(typeof(Type)).Length;
    }

    public static int No<Type>(this Type type) where Type : struct
    {
        return (int)Enum.Parse(typeof(Type), type.ToString());
    }
}