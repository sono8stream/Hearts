using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseConnector
{
    public Dictionary<string, object> dataMap;

    DatabaseReference myReference;
    public DatabaseReference MyReference { get { return myReference; } }

    Dictionary<string, object> readDataMap;
    string readData;
    bool readCompleted;

    void Initialize()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(
            "https://unitytablegame.firebaseio.com/");
        dataMap = new Dictionary<string, object>();
        readData = null;
    }

    public FirebaseConnector(string referenceName)
    {
        Initialize();
        myReference = FirebaseDatabase.DefaultInstance.GetReference(referenceName);
    }

    public FirebaseConnector(DatabaseReference reference)
    {
        Initialize();
        myReference = reference;
    }

    public void AddAsync(string key, object value)
    {
        dataMap.Add(key, value);
        myReference.UpdateChildrenAsync(dataMap);
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

    public void Read(string path)
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
                DataSnapshot snapshot = task.Result;

                readData = snapshot.GetRawJsonValue();
                Debug.Log(readData);
                readCompleted = true;

                DeserializeReadData();
            }
        });
    }

    public bool GetReadData(ref string data)
    {
        if (!readCompleted) return false;

        readCompleted = false;
        data = readData;
        return true;
    }

    /// <summary>
    /// すべてstringかDictionary<string,Dictionary<string,...>>の形に
    /// </summary>
    void DeserializeReadData()
    {
        Dictionary<string, object> desDictionary = GetValueDictionary(readData);
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
        char beginChar, char endChar,bool excludeBoundary)
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
                        if(excludeBoundary)
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
