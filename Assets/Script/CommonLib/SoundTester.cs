using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTester : MonoBehaviour
{
    SoundPlayer player;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<SoundPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            player.PlaySE(SEname.Turn);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            player.PlayBGM(BGMname.Main);
        }
    }
}