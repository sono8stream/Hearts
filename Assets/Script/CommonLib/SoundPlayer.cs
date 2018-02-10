using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public const string objectName="SoundPlayer";

    [SerializeField]
    BGMinfo[] BGMarray;
    [SerializeField]
    AudioClip[] SEarray;

    AudioSource audioSource;
    BGMinfo currentBGM;
    int maxSEcnt;
    List<float> SElenList;

    // Use this for initialization
    void Awake()
    {
        gameObject.name = objectName;
        audioSource = GetComponent<AudioSource>();
        maxSEcnt = 5;
        SElenList = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {   //Update SElenList
        UpdateSElist();
        LoopBGM();
    }

    public void PlayBGM(BGMname name, float delay = 0)
    {
        if ((int)name >= BGMarray.Length) return;

        currentBGM = BGMarray[(int)name];
        Debug.Log(audioSource.clip);
        audioSource.clip = currentBGM.clip;
        audioSource.PlayDelayed(delay);
    }

    public void PlaySE(SEname name)
    {
        if (SElenList.Count > maxSEcnt) return;
        if ((int)name >= SEarray.Length) return;

        AudioClip clip = SEarray[(int)name];
        audioSource.PlayOneShot(clip);
        SElenList.Add(clip.length);
    }

    void UpdateSElist()
    {
        List<float> tempLenList = new List<float>();
        foreach (float len in SElenList)
        {
            float newLen = len - Time.deltaTime;
            if (newLen > 0)
            {
                tempLenList.Add(newLen);
            }
        }
        SElenList = tempLenList;
    }

    void LoopBGM()
    {
        if (audioSource.clip == null) return;

        if (currentBGM.loopEndSec > 0 && audioSource.time >= currentBGM.loopEndSec)
        {
            audioSource.time = currentBGM.loopBeginSec;
        }
    }

    ///enum名からresources.loadしたかったけど遅そうなのでやめた
    /*void LoadBGMs()
    {
        int len = BGMname.Wait.GetLength();
        AudioClip[] tempArray = new AudioClip[len];
        for(int i = 0; i < len; i++)
        {
            tempArray[i]=resource
        }
    }

    void LoadSEs()
    {

    }*/
}

public enum BGMname
{
    Wait,Main
}

public enum SEname
{
    Enter, Turn, Message
}