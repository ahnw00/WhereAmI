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
    
    //플레이어 리스팅 관련 선언
    [SerializeField] Transform playerListParent;
    [SerializeField] GameObject playerListing;
    public Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();
    bool isPlayerReady = false;
    NetworkManager networkMng;
    GameManager gameManager;


    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        networkMng = FindObjectOfType<NetworkManager>();
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

    //방에 입장할 때
    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        RandomSpawn();
        ConnectPanel.SetActive(true);
        
        //플레이어 리스트
        if (playerListEntries == null)
		{
			playerListEntries = new Dictionary<int, GameObject>();
		}

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
		{   //플레이어 리스트 내용물 생성
			GameObject entry = Instantiate(playerListing, playerListParent);
			entry.GetComponent<PlayerListing>().SetPlayerInfo(p.ActorNumber, p.NickName);
            //플레이어 준비 상태 
			object isPlayerReady;
			if (p.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
			{
				entry.GetComponent<PlayerListing>().SetPlayerReady((bool)isPlayerReady);
			}
            //입장한 플레이어 리스트에 추가
			playerListEntries.Add(p.ActorNumber, entry);
            
		}
    }

    //랜덤 아바타 스폰 함수
    public void RandomSpawn()
    {
        int randomNum = Random.Range(0, gameManager.playableObjects.Count);
        string randomObj = gameManager.playableObjects[randomNum].name;
        GameObject spawnedObj = PhotonNetwork.Instantiate(randomObj, RandomSpawnPoint(), Quaternion.identity);
    }


    //스폰 위치 랜덤 지정 함수
    public Vector3 RandomSpawnPoint()
    {
        if(Random.value > 0.5f)
        {
            return new Vector3(Random.Range(60, 96), 80f, Random.Range(-3, 9));
        }
        else
        {
            return new Vector3(Random.Range(60, 96), 80f, Random.Range(20, 32));
        }
    }

    //전체 플레이어 준비 완료 됐는지 체크
    public bool CheckPlayersReady()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return false;
		}
        //각 플레이어들의 isPlayerReady가 참인지 체크
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

    //새 플레이어 들어올 때 호출되는 함수
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject entry = Instantiate(playerListing, playerListParent);
		entry.GetComponent<PlayerListing>().SetPlayerInfo(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);
    }

    //customProperty에 변화 있을 때 호출되는 함수
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
			{   //SetPlayerReady 함수 불러서 준비중/준비완료 표기 바꾸기
				entry.GetComponent<PlayerListing>().SetPlayerReady((bool)isPlayerReady);
			}
		}
    }

    //플레이어가 방 떠날 때 호출되는 함수
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
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        ConnectPanel.SetActive(false);
    }
}