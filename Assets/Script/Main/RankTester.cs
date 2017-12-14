using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class RankTester : MonoBehaviour {

    private DatabaseReference timeRankDb;

    // Use this for initialization
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(
            "https://unitytablegame.firebaseio.com/");

        timeRankDb = FirebaseDatabase.DefaultInstance.GetReference("RankTest");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            AddKey("test", (int)Time.time);
        }
    }

    void AddKey(string name, int time)
    {
        string key = timeRankDb.Push().Key;
        Dictionary<string, object> itemMap = new Dictionary<string, object>();
        itemMap.Add("name", name);
        itemMap.Add("time", time);

        Dictionary<string, object> map = new Dictionary<string, object>();
        map.Add(key, itemMap);
        timeRankDb.UpdateChildrenAsync(map);
    }
}
