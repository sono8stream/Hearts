using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class EnumHelper
{

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

    /*public static int No<Type>(this Type type) where Type : struct
    {
        return (int)Enum.Parse(typeof(Type), type.ToString());
    }*/
}