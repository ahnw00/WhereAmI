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
    public Button GameStartButton;

    void Awake()
    {
        playerListing = FindObjectOfType<PlayerListing>();
    }

    public void ReadyTimer()
    {
        /*if (readyTimer >= 0 )
        {
            readyTimerText.gameObject.SetActive(true);
            readyTimer -= Time.deltaTime;
            readyTimerText.text = (int)readyTimer + " 초 뒤 게임이 시작됩니다";
        }
        else
        {
            readyTimerText.text = "게임 시작!";
            StartCoroutine(LoadingGameTimer());
        }*/
        GameStartButton.gameObject.SetActive(false);
        StartCoroutine(GameScheduler());
    }

    private IEnumerator GameScheduler()
    {
        readyTimer = 5f;
        isPlaying = true;
        readyTimerText.gameObject.SetActive(true);

        while (readyTimer > 0 && isPlaying)
        {
            readyTimer -= Time.deltaTime;
            readyTimerText.text = (int)readyTimer + " 초 뒤 게임이 시작됩니다";
            yield return null;

            if (readyTimer <= 0)
            {
            	readyTimerText.text = "게임 시작!";
                Invoke("GameStartOff", 2.0f);
            }
        }
    }

    void GameStartOff()
    {
        readyTimerText.gameObject.SetActive(false);
    }
}
