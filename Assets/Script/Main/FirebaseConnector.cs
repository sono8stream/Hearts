using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseConnector
{
    public Dictionary<string, object> dataMap;
    public Action DatabaseErrored;

    DatabaseReference myReference;
    public DatabaseReference MyReference { get { return myReference; } }
    DataSnapshot snapData;
    public DataSnapshot SnapData { get { return snapData; } }

    Dictionary<string, object> readDataMap;
    bool readCompleted;
    bool isReadOnly;
    int accessLim;//エラー判定するまでのアクセス限界数
    int accessCounter;

    void Initialize()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(
            "https://unitytablegame.firebaseio.com/");
        dataMap = new Dictionary<string, object>();
        snapData = null;
        accessLim = 30;
        accessCounter = 0;
    }

    public FirebaseConnector(string referenceName, bool readOnly = false)
    {
        Initialize();
        myReference = FirebaseDatabase.DefaultInstance.GetReference(referenceName);
        isReadOnly = readOnly;
    }

    public FirebaseConnector(DatabaseReference reference, bool readOnly = false)
    {
        Initialize();
        myReference = reference;
        isReadOnly = readOnly;
    }

    public void AddAsync(string key, object value)
    {
        if (isReadOnly) return;

        Dictionary<string, object> dataMap = new Dictionary<string, object>();
        dataMap.Add(key, value);
        myReference.UpdateChildrenAsync(dataMap);
        Debug.Log(key);
        Debug.Log(dataMap);
    }

    public void Push(string key,string name)
    {
        bool readOnly = isReadOnly;
        isReadOnly = false;
        AddAsync(string.Format("{0}/{1}", key, name), "OK");
        isReadOnly = readOnly;
    }

    public void AsyncMap()
    {
        myReference.UpdateChildrenAsync(dataMap);

    }

    public void RemoveItem(string path)
    {
        DatabaseReference reference = path == null ? myReference : myReference.Child(path);
        reference.RemoveValueAsync();
    }

    public void ReadQuery(string path,bool nullable=false)
    {
        DatabaseReference reference = path == null ? myReference : myReference.Child(path);
        readCompleted = false;

        reference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Read Error!");
            }
            else if (task.IsCompleted)
            {
                if (!nullable && task.Result.Value == null)
                {
                    ReadQuery(path);
                    accessCounter++;
                    Debug.Log(accessCounter);
                    if (accessCounter >= accessLim)
                    {
                        DatabaseErrored();
                        //Application.Quit();
                    }
                    return;
                }
                accessCounter = 0;
                snapData = task.Result;
                readCompleted = true;
                //Debug.Log(snapData);
                //DeserializeReadData();
            }
        });
    }

    public void RemoveReadData()
    {
        snapData = null;
    }

    /*public bool GetReadData()
    {
        if (!readCompleted) return false;

        readCompleted = false;
        snap = readData;
        Debug.Log(snap.Value);
        return true;
    }*/

    public string[] GetChildrenValueString(DataSnapshot snap)
    {
        List<string> name = new List<string>();
        foreach (DataSnapshot s in snap.Children)
        {
            string nameJson = s.GetRawJsonValue();
            name.Add(nameJson.Substring(1, nameJson.Length - 2));
        }
        return name.ToArray();
    }

    /// <summary>
    /// すべてstringかDictionary<string,Dictionary<string,...>>の形に
    /// </summary>
    void DeserializeReadData()
    {
        Dictionary<string, object> desDictionary
            = GetValueDictionary(snapData.GetRawJsonValue());
    }

    public Dictionary<string, object> GetValueDictionary(string keyStr)
    {
        if (keyStr[0] == '"')//ただのstringキー
        {
            return null;
        }

        Dictionary<string, object> tempDictionary = new Dictionary<string, object>();

        bool onKey = true;
        string key = "";
        string valueStr = "";
        string rawData = keyStr.Substring(1, keyStr.Length - 2);
        Debug.Log(rawData);
        for (int i = 0; i < rawData.Length; i++)
        {
            if (onKey)
            {
                if (rawData[i] != '"') continue;

                key = ExtractFirstString(rawData, ref i, '"', '"', true);
                onKey = false;
            }
            else//onValue
            {
                if (rawData[i] == '"')
                {
                    valueStr = ExtractFirstString(rawData, ref i, '"', '"', true);
                    Debug.Log(key + ":" + valueStr);
                    tempDictionary.Add(key, valueStr);
                    onKey = true;
                }
                else if (rawData[i] == '{')
                {
                    valueStr = ExtractFirstString(rawData, ref i, '{', '}', false);
                    tempDictionary.Add(key, GetValueDictionary(valueStr));
                    onKey = true;
                }
            }
        }

        return tempDictionary;
    }

    string ExtractFirstString(string str, ref int iter,
        char beginChar, char endChar, bool excludeBoundary)
    {
        bool onSelecting = false;
        int firstIndex = iter;
        int nestDepth = 0;

        for (; iter < str.Length; iter++)
        {
            if (onSelecting)
            {
                if (str[iter] == endChar)
                {
                    nestDepth--;
                    if (nestDepth == 0)
                    {
                        int charCnt = iter - firstIndex + 1;
                        if (excludeBoundary)
                        {
                            charCnt--;
                        }
                        return str.Substring(firstIndex, charCnt);
                    }
                }
                else if (str[iter] == beginChar)//beginChar,endCharが同一の場合もあるのでこの順
                {
                    nestDepth++;
                }
            }
            else
            {
                firstIndex = iter;
                if (excludeBoundary)
                {
                    firstIndex += 1;
                }
                nestDepth = 1;
                onSelecting = true;
            }
        }

        return "";
    }
}
