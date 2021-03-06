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
    [SerializeField] GameObject waterTiles;
    [SerializeField] GameObject barrierPlanes;
    [SerializeField] AudioSource timerSource;

    PlayerListing playerListing;
    public Button GameStartButton;
    GameManager gameMng;

    void Awake()
    {
        playerListing = FindObjectOfType<PlayerListing>();
        gameMng = FindObjectOfType<GameManager>();
    }

    public void ReadyTimer()
    {
        GameStartButton.gameObject.SetActive(false);
        StartCoroutine(GameScheduler());
    }

    private IEnumerator GameScheduler()
    {
        readyTimer = 5f;
        isPlaying = true;
        readyTimerText.gameObject.SetActive(true);
        timerSource.Play();

        while (readyTimer > 0 && isPlaying)
        {
            readyTimer -= Time.deltaTime / PhotonNetwork.PlayerList.Length;
            readyTimerText.text = (int)readyTimer + 1 + " 초 뒤 게임이 시작됩니다";
            yield return null;

            if (readyTimer <= 0)
            {
            	readyTimerText.text = "게임 시작!";
                Invoke("GameStartOff", 2.0f);
            }
        }
        TurnOffMeshColliders(waterTiles);
        TurnOffMeshColliders(barrierPlanes);
        NickNamesOff();
    }

    void TurnOffMeshColliders(GameObject go)
    {
        foreach(MeshCollider mc in go.GetComponentsInChildren<MeshCollider>())
        {
            mc.enabled = false;
        }
    }

    void NickNamesOff()
    {
        foreach(Player player in gameMng.players)
        {
            player.TextOff();
        }
    }

    void GameStartOff()
    {
        readyTimerText.gameObject.SetActive(false);
        YouTaggerTextOn();
    }

    void YouTaggerTextOn()
    {
        foreach(PhotonView pv in FindObjectsOfType<PhotonView>())
        {
            if(pv.IsMine && pv.gameObject.GetComponent<Player>().GetIsTaggerValue())
            {
                gameMng.youTaggerText.gameObject.SetActive(true);
                break;
            }
        } 
    }
}
