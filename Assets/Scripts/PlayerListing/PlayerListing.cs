using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerListing : MonoBehaviour
{
    public Text playerNameText;
    public Text playerStatusText;
    public int ownerId;
    private bool isPlayerReady;
    public Button PlayerReadyButton;
    NetworkManager networkMng;

    private void Awake() 
    {
        networkMng = FindObjectOfType<NetworkManager>();
    }
    
    public void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
        {
            PlayerReadyButton.gameObject.SetActive(false);
        }
        else
        {
			ExitGames.Client.Photon.Hashtable readyStatus = new ExitGames.Client.Photon.Hashtable(){ {"isPlayerReady", isPlayerReady}};
            PhotonNetwork.LocalPlayer.SetCustomProperties(readyStatus);

            PlayerReadyButton.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);

                ExitGames.Client.Photon.Hashtable newStatus = new ExitGames.Client.Photon.Hashtable() { { "isPlayerReady", isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(newStatus);
            });
        }
    }
    
    public void Update() 
    {
       if(Input.GetKeyDown(KeyCode.R))
       {
            isPlayerReady = !isPlayerReady;
            SetPlayerReady(isPlayerReady);

            ExitGames.Client.Photon.Hashtable newStatus = new ExitGames.Client.Photon.Hashtable() { { "isPlayerReady", isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(newStatus);
       } 
    }

    public void SetPlayerInfo(int playerId, string playerName)
    {
        //Player = player;
        playerNameText.text = playerName;
        ownerId = playerId;
        playerStatusText.text = "준비 중";
        playerStatusText.color = Color.red;
    }

    public void SetPlayerReady(bool playerReady)
    {
        playerStatusText.text = playerReady ? "준비 완료" : "준비 중";
        playerStatusText.color = playerReady ? Color.green : Color.red;
    }

    public void RemovePlayerReadyStatus()
    {
        foreach (GameObject entry in networkMng.playerListEntries.Values)
	    {
		    entry.transform.GetChild(1).gameObject.GetComponent<Text>().text = "";
	    }
    }
} 
