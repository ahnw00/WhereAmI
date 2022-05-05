using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel, ConnectPanel;
    
    //public GameObject RespawnPanel;
    [SerializeField] Transform playerListParent;
    [SerializeField] GameObject playerListing;
    private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();
    bool isPlayerReady = false;
    

    void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        Spawn();
        ConnectPanel.SetActive(true);
        //player list
        /*Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListing, playerListParent).GetComponent<PlayerListing>().SetPlayerInfo(players[i]);
            //내가 방에 들어가면 방에있는 사람 목록 만큼 이름표 뜨게 하기
        }*/

        
        if (playerListEntries == null)
		{
			playerListEntries = new Dictionary<int, GameObject>();
		}

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
		{
			GameObject entry = Instantiate(playerListing, playerListParent);
			entry.GetComponent<PlayerListing>().SetPlayerInfo(p.ActorNumber, p.NickName);

			object isPlayerReady;
			if (p.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
			{
				entry.GetComponent<PlayerListing>().SetPlayerReady((bool)isPlayerReady);
			}

			playerListEntries.Add(p.ActorNumber, entry);
		}
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate("PlayerObject", new Vector3(Random.Range(60, 96), 80f, Random.Range(-3, 32)), Quaternion.identity);
    }

    private bool CheckPlayersReady()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return false;
		}

		foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
		{
			object isPlayerReady;

			if (p.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
			{
				if (!(bool)isPlayerReady)
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		return true;
	}

    //새 플레이어 들어오면 리스트 끝에 생성
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject entry = Instantiate(playerListing, playerListParent);
		entry.GetComponent<PlayerListing>().SetPlayerInfo(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playerListEntries == null)
		{
			playerListEntries = new Dictionary<int, GameObject>();
		}

		GameObject entry;
		if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
		{
			object isPlayerReady;
			if (changedProps.TryGetValue("isPlayerReady", out isPlayerReady))
			{
				entry.GetComponent<PlayerListing>().SetPlayerReady((bool)isPlayerReady);
			}
		}

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
		playerListEntries.Remove(otherPlayer.ActorNumber);
	}

    void Update() 
    { 
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) 
        {
            PhotonNetwork.Disconnect(); 
        }
        if(CheckPlayersReady())
        {
            Debug.Log("게임시작");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        ConnectPanel.SetActive(false);
    }
}