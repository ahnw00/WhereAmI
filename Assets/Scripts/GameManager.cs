using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public List<GameObject> playableObjects;
    [HideInInspector] public bool isRoundEnded;
    [SerializeField] private GameObject camWaitingPoint;
    private float lerpTime = 5f;

    public List<Player> players = new List<Player>();
    public List<int> RandomNums = new List<int>();
    public List<Vector3> RandomVecs = new List<Vector3>();
    public int whichPlayerIsTagger;
    [SerializeField] private Button gameStartBtn;
    public int nextTagger;
    NetworkManager networkMng;
    
    void Start()
    {
        networkMng = FindObjectOfType<NetworkManager>();
        gameStartBtn.onClick.AddListener(() =>{ PickFirstTagger(); });
    }

    public void PickFirstTagger()
    {
        //PlayerController달고있는 애들 리스트화 시키기
        List<Player> PlayerList = new List<Player>(players);
        //그중에 하나 술래로 정해주기
        whichPlayerIsTagger = Random.Range(0, PlayerList.Count);
        //플레이어 몇명인지 , 누가 술래인지 체크
        Debug.Log("We have " + PlayerList.Count);
        Debug.Log("술래 Number is " + whichPlayerIsTagger);
        //해당 사람이 술래로 지정
        players[whichPlayerIsTagger].GetComponent<PhotonView>().RPC("SetTagger", RpcTarget.All, true);
    }

    public void WhenTaggerHitPlayer()
    {
        StartCoroutine("CamToWaitPointCoroutine");
    }

    private IEnumerator CamToWaitPointCoroutine()
    {
        Camera cam = FindObjectOfType<Camera>();
        cam.transform.SetParent(camWaitingPoint.transform);

        float timer = 0;
        float speed = 1 / lerpTime;

        while(timer < 1)
        {
            timer += Time.deltaTime * speed;
            cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(75.6f, 78.3f, -10), timer);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.Euler(0, 0, 0), timer);
            yield return null;

            if(timer >= 1)
            {
                MakeNewObj();
            }
        }
    }
    public void MakeNewObj()
    {
        List<Player> PlayerList = new List<Player>(players);
        int i = 0;
        foreach(Player plyr in PlayerList)
        {
            InstantiateObj(plyr, RandomNums[i], RandomVecs[i]);
            Destroy(plyr.gameObject.transform.GetChild(2).gameObject);
            plyr.GetComponent<PhotonView>().RPC("ResetCamera", RpcTarget.All);
            i++;
        }
    }

    public void MakeRandomNum()
    {
        List<Player> PlayerList = new List<Player>(players);
        for(int i=0; i<PlayerList.Count; i++)
        {
            int randomNum = Random.Range(0, playableObjects.Count);
            players[i].GetComponent<PhotonView>().RPC("addRandumNum", RpcTarget.All, randomNum);
            Vector3 randomVec = networkMng.RandomSpawnPoint();
            players[i].GetComponent<PhotonView>().RPC("addRandumVec", RpcTarget.All, randomVec);
        }
    }

    public void InstantiateObj(Player plyr, int randomNum, Vector3 ranVec)
    {
        GameObject spawnedObj = Instantiate(playableObjects[randomNum].gameObject.transform.GetChild(2).gameObject, plyr.gameObject.transform.position, Quaternion.identity);
        spawnedObj.transform.SetParent(plyr.gameObject.transform);
        plyr.playerTr = spawnedObj.transform;
        plyr.gameObject.transform.localPosition = ranVec;
    }
    
}
