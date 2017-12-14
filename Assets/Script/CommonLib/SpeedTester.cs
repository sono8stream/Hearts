using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTester
{

    // Use this for initialization
    public SpeedTester()
    {
    }

    public static void Test(System.Action action1, System.Action action2)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Reset();
        sw.Start();
        action1();

        sw.Stop();
        Debug.Log(sw.Elapsed);

        sw.Reset();
        sw.Start();
        action2();

        sw.Stop();
        Debug.Log(sw.Elapsed);
    }
}
