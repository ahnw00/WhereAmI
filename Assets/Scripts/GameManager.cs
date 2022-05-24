using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public List<GameObject> playableObjects;
    [HideInInspector] public bool isRoundEnded;
    [SerializeField] private GameObject camWaitingPoint;
    private float lerpTime = 5f;

    public List<Player> players = new List<Player>();
    public int whichPlayerIsTagger;
    
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("CamToWaitPointCoroutine");
        }
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
        }
    }
}
