using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    //public GameObject RespawnPanel;

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
    }

    public void Spawn()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(60, 96), 80f, Random.Range(-3, 32)), Quaternion.Euler(new Vector3(-90, 0, -90)));
        Camera mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.SetParent(player.transform);
        mainCamera.transform.localPosition = new Vector3(-15, 0, 10);
        mainCamera.transform.localRotation = Quaternion.Euler(0, 110f, 90f);
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
        //RespawnPanel.SetActive(false);
    }
}
