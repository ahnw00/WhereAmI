using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public List<GameObject> playableObjects;
    [HideInInspector] public bool isRoundEnded;
    [SerializeField] private GameObject camWaitingPoint;
    private float lerpTime = 5f;

    public List<Player> players = new List<Player>();
    public List<int> RandomNums = new List<int>();
    public List<Vector3> RandomVecs = new List<Vector3>();
    public int whichPlayerIsTagger;
    [SerializeField] Button gameStartBtn;
    NetworkManager networkMng;
    [SerializeField] Text taggerChangeText;
    public Text youTaggerText; 
    
    void Start()
    {
        networkMng = FindObjectOfType<NetworkManager>();
        List<Player> PlayerList = new List<Player>(players);
        whichPlayerIsTagger = Random.Range(0, PlayerList.Count);
        gameStartBtn.onClick.AddListener(() =>{ ChangeTagger(whichPlayerIsTagger, true); });
    }

    public void ChangeTagger(int i, bool isTagger)
    {
        //Player 달고있는 애들 리스트화 시키기
        List<Player> PlayerList = new List<Player>(players);
        //해당 사람 술래로 지정
        players[i].GetComponent<PhotonView>().RPC("SetTagger", RpcTarget.All, isTagger); 
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

        taggerChangeText.gameObject.SetActive(true);
        youTaggerText.gameObject.SetActive(false);
        while(timer < 1)
        {
            timer += Time.deltaTime * speed;
            cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(75.6f, 78.3f, 12.14f), timer);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.Euler(0, 0, 0), timer);
            yield return null;

            if(timer >= 0.8)
            {
                taggerChangeText.gameObject.SetActive(false);
                //타이머 끝나면 랜덤 오브젝트 생성
                MakeNewObj();
                break;
            }
        }
    }

    //새 오브젝트 생성 & 이전꺼 파괴 & 카메라 원래대로 & 술래입니다 텍스트 띄워주기
    public void MakeNewObj()
    {
        for(int i = 0; i < players.Count; i++)
        {
            InstantiateObj(players[i], RandomNums[i], RandomVecs[i]);
            Destroy(players[i].gameObject.transform.GetChild(2).gameObject);
            foreach(PhotonView pv in FindObjectsOfType<PhotonView>())
            {
                if(pv.IsMine)
                {
                    pv.gameObject.GetComponent<Player>().ResetCamera();
                    if(pv.gameObject.GetComponent<Player>().GetIsTaggerValue())
                    {
                        youTaggerText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    youTaggerText.gameObject.SetActive(false);
                }
            }
        }
    }

    //새 랜덤 오브젝트 생성
    public void InstantiateObj(Player plyr, int randomNum, Vector3 ranVec)
    { 
        //playableObjects[랜덤 숫자]의 자식에 있는 오브젝트 프리펩만 생성 
        GameObject spawnedObj = Instantiate(playableObjects[randomNum].gameObject.transform.GetChild(2).gameObject, plyr.gameObject.transform.position, Quaternion.identity);
        //player오브젝트의 자식으로 넣어줌
        spawnedObj.transform.SetParent(plyr.gameObject.transform);
        plyr.playerTr = spawnedObj.transform;
        //위치도 랜덤 위치로 재설정
        plyr.gameObject.transform.localPosition = ranVec;
    }

    //playableObjects의 인덱스를 위한 랜덤 숫자 & 랜덤 위치값 생성 
    public void MakeRandomNum()
    {
        for(int i=0; i < players.Count; i++)
        {
            players[i].GetComponent<PhotonView>().RPC("ClearRandomList", RpcTarget.All);
        }
        for(int i=0; i < players.Count; i++)
        {
            int randomNum = Random.Range(0, playableObjects.Count);
            players[i].GetComponent<PhotonView>().RPC("addRandomNum", RpcTarget.All, randomNum);
            Vector3 randomVec = networkMng.RandomSpawnPoint();
            players[i].GetComponent<PhotonView>().RPC("addRandomVec", RpcTarget.All, randomVec);
        }
    } 

}
