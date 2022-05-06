using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Timer : MonoBehaviourPunCallbacks
{
    public Text readyTimerText;
    float readyTimer = 5;
    bool isPlaying;

    public void ReadyTimer()
    {
        readyTimerText.gameObject.SetActive(true);
        if (readyTimer > 0 )
        {
            readyTimer -= Time.deltaTime;
            readyTimerText.text = Mathf.Round(readyTimer) + " 초 뒤 게임이 시작됩니다!";
        }
    }
}
