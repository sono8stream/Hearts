using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class NetworkAdaptor{

    DatabaseReference cardList;

    public NetworkAdaptor()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(
            "https://unitytablegame.firebaseio.com/");

        cardList = FirebaseDatabase.DefaultInstance.GetReference("Player");
        Debug.Log(cardList);
    }
}
