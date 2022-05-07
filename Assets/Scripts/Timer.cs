using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Timer : MonoBehaviourPunCallbacks
{
    public Text readyTimerText;
    float readyTimer =  5;
    bool isPlaying;

    PlayerListing playerListing;

    void Awake()
    {
        playerListing = FindObjectOfType<PlayerListing>();
    }

    public void ReadyTimer()
    {
        if (readyTimer >= 0 )
        {
            readyTimerText.gameObject.SetActive(true);
            readyTimer -= Time.deltaTime;
            readyTimerText.text = (int)readyTimer + " 초 뒤 게임이 시작됩니다";
        }
        else
        {
            readyTimerText.text = "게임 시작!";
            StartCoroutine(LoadingGameTimer());
        }
    }

    IEnumerator LoadingGameTimer()
    {
        yield return null;
        readyTimerText.gameObject.SetActive(false);
    }
}
