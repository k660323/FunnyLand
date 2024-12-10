using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TSoundManager : MonoBehaviour
{
    AudioSource bgmPlayer;
    AudioSource[] sfxPlayers;
    int nextPlayer;

    public AudioClip startClip;
    public AudioClip overClip;
    public AudioClip[] hitClips;
    public AudioClip failClip;
    static TSoundManager instance;
    public static TSoundManager Instacne
    {
        get
        {
            Init();
            return instance;
        }
    }

    private static void Init()
    {
        if (instance == null)
        {
            var obj = FindObjectOfType<TSoundManager>();
            if (obj == null)
            {
                GameObject go = new GameObject();
                instance = go.AddComponent<TSoundManager>();
            }
            else
            {
                instance = obj;
            }
        }
    }

    void Start()
    {
        Init();
        if (instance != this)
            Destroy(gameObject);

        bgmPlayer = GetComponent<AudioSource>();
        sfxPlayers = new AudioSource[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            sfxPlayers[i] = transform.GetChild(i).GetComponent<AudioSource>();
        } 
    }

    public void BgmStart()
    {
        bgmPlayer.Play();
    }

    public void BgmStop()
    {
        bgmPlayer.Stop();
    }

    public void PlayerSound(string name)
    {
        switch(name)
        {
            case "Start":
                sfxPlayers[nextPlayer].clip = instance.startClip;
                break;
            case "Over":
                sfxPlayers[nextPlayer].clip = instance.overClip;
                break;
            case "Hit":
                int ran = Random.Range(0, hitClips.Length);
                sfxPlayers[nextPlayer].clip = instance.hitClips[ran];
                break;
            case "Fail":
                sfxPlayers[nextPlayer].clip = instance.failClip;
                break;
        }
        sfxPlayers[nextPlayer].Play();
        nextPlayer = (nextPlayer + 1) % sfxPlayers.Length;
    }
}
