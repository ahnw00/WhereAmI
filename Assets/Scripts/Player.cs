using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody RB;
    public PhotonView PV;
    public TextMeshPro nickNameText;
    public Transform cameraArm; 
    public Transform playerTr;

    bool isJump = false;
    float speed = 4f;
    Vector3 curPos;
    Quaternion curRot;
    Vector2 moveInput;
    Vector2 mouseDelta;

    NetworkManager networkMng;
    Timer timer;
    GameManager gameMng;
    [SerializeField] bool isTagger = false;
    Tagger tagger; 

    void Awake()
    {
        networkMng = FindObjectOfType<NetworkManager>();
        timer = FindObjectOfType<Timer>();
        gameMng = FindObjectOfType<GameManager>();
        tagger = FindObjectOfType<Tagger>();
        // 닉네임
        nickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        nickNameText.color = PV.IsMine ? Color.green : Color.red;

        //gameMng.players 리스트에 플레이어 추가
        gameMng.players.Add(this);
    }

    void Start()
    {
        if(PV.IsMine)
        {
            Camera mainCamera = FindObjectOfType<Camera>();
            mainCamera.transform.SetParent(cameraArm);
            mainCamera.transform.localPosition = new Vector3(0, 1.2f, -3f);
            playerTr.localRotation = Quaternion.Euler(new Vector3(10, 0, 0));
        }
        //PV가 Master Client면 게임시작 버튼 켜주기
        if(PhotonNetwork.IsMasterClient)
        {
            timer.GameStartButton.gameObject.SetActive(true);
            timer.GameStartButton.onClick.AddListener(() =>{ ReadyTimer(); });
        }
    }
    
    void Update()
    {
        if (PV.IsMine)
        {
            GetInput();
            LookAt(cameraArm);
            LookAt(nickNameText.transform);
            Move();
            Jump();
            //술래가 됐다!
            if (isTagger && Input.GetMouseButtonDown(0))
            {
                HitPlayer();
            }
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else 
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            playerTr.rotation = Quaternion.Slerp(playerTr.rotation, curRot, Time.deltaTime * 10);
            LookAt(nickNameText.transform);
        }
    }

    // 충돌 감지, collision은 그 충돌체가 누구인지
    public void OnCollisionEnter(Collision collision) 
    {
        if(collision.gameObject.tag == "Ground")
        {
            isJump = false;
        }
    }

    private void GetInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y);

        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");

        mouseDelta = new Vector2(x, y);

        // isKeyDown[(int)DownKey.JUMP] = Input.GetButtonDown("Jump");

        //attackDelay += Time.deltaTime;
    }

    private void LookAt(Transform target)
    {
        // 현재 내 카메라의 각도를 구해오고
        Vector3 cameraAngle = cameraArm.rotation.eulerAngles;

        float x = cameraAngle.x - mouseDelta.y;

        x = x < 180f ? Mathf.Clamp(x, -1f, 70f) : Mathf.Clamp(x, 335f, 361f);

        target.rotation = Quaternion.Euler(new Vector3(x, cameraAngle.y + mouseDelta.x, cameraAngle.z));
    }

    private void Move()
    {
        Vector3 lookforward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
        Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;

        playerTr.forward = lookforward;

        Vector3 moveVec = lookforward * moveInput.y + lookRight * moveInput.x;

        transform.position += moveVec * Time.deltaTime * speed;
    }

    void Jump()
    {
        // ↑ 점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) 
        {
            isJump = true;
            PV.RPC("JumpRPC", RpcTarget.All);
        }
    }


    [PunRPC]
    void JumpRPC()
    {
        RB.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }

    // public void Hit()
    // {
    //     HealthImage.fillAmount -= 0.1f;
    //     if (HealthImage.fillAmount <= 0)
    //     {
    //         GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
    //         PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
    //     }
    // }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(playerTr.rotation);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            curRot = (Quaternion)stream.ReceiveNext();
        }
    }
//-------------------------게임 시작 버튼 & 타이머 함수-----------------------------------
    //게임시작 버튼에 딸린 함수 (타이머 On)
    void ReadyTimer()
    {
        if(networkMng.CheckPlayersReady() && networkMng.playerListEntries.Count > 0)
        {
            PV.RPC("ReadyTimerFunc", RpcTarget.All);   //준비 타이머 On
            if(PV.IsMine)
            {
                PV.RPC("PlayerReadyButtonFunc", RpcTarget.All);  //모든 플레이어한테서 버튼 꺼주기
            } 
        }
    }

    [PunRPC]
    public void ReadyTimerFunc()
    {
        timer.ReadyTimer();
    }

    [PunRPC]
    public void PlayerReadyButtonFunc()
    {
        foreach (GameObject entry in networkMng.playerListEntries.Values)
	    {
		    entry.transform.GetChild(2).gameObject.SetActive(false);
	    }
    }  

//-----------------------------술래 설정 함수-------------------------------------
    [PunRPC]
    void SetTagger(bool _isTagger)
    {
        //술래를 정해주는 RPC
        isTagger = _isTagger;
        Debug.Log("Tagger " + isTagger);
    }

    public void HitPlayer()
    {
        Camera mainCamera = FindObjectOfType<Camera>();
        RaycastHit hitObj;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        ray.origin = mainCamera.ViewportToWorldPoint(new Vector3(0.5f,0.5f,0f));

        Debug.DrawRay(ray.origin, ray.direction * 150, Color.red, 3f);
        if (Physics.Raycast(ray.origin, ray.direction, out hitObj, Mathf.Infinity))
        {
            //레이에 맞은 물체가 player인가? 맞다면 player 정보 갖고오기
            if(hitObj.collider.gameObject.transform.parent.transform.GetComponent<Player>())
            {
                for(int i = 0; i < gameMng.players.Count; i++)
                {
                    if(gameMng.players[i] == hitObj.collider.gameObject.transform.parent.transform.GetComponent<Player>())
                    {
                        Debug.Log(hitObj.transform.gameObject);
                        
                        gameMng.nextTagger = i;
                        Debug.Log(gameMng.nextTagger);
                        gameMng.MakeRandomNum();
                        PV.RPC("WhenTaggerHitPlayerFunc", RpcTarget.All); 
                        break;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void WhenTaggerHitPlayerFunc()
    {
        gameMng.WhenTaggerHitPlayer();
    }

    [PunRPC]
    public void addRandumNum(int ranNum)
    {
        gameMng.RandomNums.Add(ranNum);
    }

    [PunRPC]
    public void addRandumVec(Vector3 ranVec)
    {
        gameMng.RandomVecs.Add(ranVec);
    }

    [PunRPC]
    public void ResetCamera()
    {
        Camera cam = FindObjectOfType<Camera>();
        cam.transform.SetParent(cameraArm.transform);
        cam.transform.localPosition = new Vector3(0, 1.2f, -3);
        cam.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
//--------------------------오브젝트 파괴 함수------------------------------------
    [PunRPC]
    void DestroyRPC() => Destroy(this.gameObject);

    public void DestroyObj()
    {
        PV.RPC("DestroyPlayerObject", RpcTarget.All);
    }

    [PunRPC]
    private void DestroyPlayerObject()
    {
        if(photonView.IsMine)
        {
            Destroy(this.gameObject);
        }
    }
}