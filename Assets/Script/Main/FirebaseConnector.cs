using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseConnector
{
    DatabaseReference myReference;
    public Dictionary<string, object> dataMap;

    public FirebaseConnector(string referenceName)
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(
            "https://unitytablegame.firebaseio.com/");

        myReference = FirebaseDatabase.DefaultInstance.GetReference(referenceName);

        dataMap = new Dictionary<string, object>();
    }

    public void AsyncMap()
    {
        myReference.UpdateChildrenAsync(dataMap);
    }

    public void RemoveItem(string key)
    {
        dataMap.Remove(key);
        myReference.Child(key).RemoveValueAsync();
    }

    public string Read(string path)
    {
        string s = "";

        myReference.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Read Error!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                s = snapshot.GetRawJsonValue();
            }
        });
        return s;
    }
}
