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

    void Awake()
    {
        networkMng = FindObjectOfType<NetworkManager>();
        timer = FindObjectOfType<Timer>();
        gameMng = FindObjectOfType<GameManager>();
        // 닉네임
        nickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        nickNameText.color = PV.IsMine ? Color.green : Color.red;

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PhotonView P = this.gameObject.GetComponent<PhotonView>();
            //if(P.ViewID == PhotonNetwork.PlayerList[i].UserId)
        }
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
        gameMng.players[gameObject.GetComponent<PhotonView>().ViewID / 1000 -1] = this;
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

//-----------------------------술래 관련 함수-------------------------------------
    [PunRPC]
    void SetTagger(bool _isTagger)
    {   //술래 불 값 바꿔주기
        isTagger = _isTagger;
        Debug.Log("Tagger " + isTagger);
    }
    
    //술래가 hit 할 때 쓰는 함수
    //술래의 PV에서만 실행하는 함수로, 얘에서만 랜덤값 만드는 함수 돌려서 
    //그 값을 나머지 PV에 뿌려줌 (이렇게 안하면..랜덤값이 다 제각각이여) 
    public void HitPlayer()
    {
        Camera mainCamera = FindObjectOfType<Camera>();
        RaycastHit hitObj;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //레이 쏠 때 시작 위치를 화면 정중앙으로 설정
        ray.origin = mainCamera.ViewportToWorldPoint(new Vector3(0.5f,0.5f,0f));
        Debug.DrawRay(ray.origin, ray.direction * 150, Color.red, 3f);
        //화면 정중앙에서 마우스 클릭 위치의 좌표값 방향으로 레이 쏘기
        if (Physics.Raycast(ray.origin, ray.direction, out hitObj, Mathf.Infinity))
        {
            //레이에 맞은 물체가 player 스크립트 갖고 있으면
            if(hitObj.collider.gameObject.transform.parent.transform.GetComponent<Player>())
            {   //player 리스트 안에서
                for(int i = 0; i < gameMng.players.Count; i++)
                {   //레이 맞은 오브젝트의 player와 같은 것 찾기
                    if(gameMng.players[i] == hitObj.collider.gameObject.transform.parent.transform.GetComponent<Player>())
                    {
                        Debug.Log(hitObj.transform.gameObject);
                        //다음 술래를 맞은 오브젝트로
                        gameMng.nextTagger = i;
                        Debug.Log(gameMng.nextTagger);
                        //각각 플레이어의 오브젝트 랜덤 생성을 위한 번호 & 랜덤 벡터값 생성 
                        gameMng.MakeRandomNum();
                        //카메라 조정 코루틴 & 새 랜덤 오브젝트로 교체
                        PV.RPC("WhenTaggerHitPlayerFunc", RpcTarget.All); 
                        break;
                    }
                }
            }
        }
    }
    //카메라 코루틴 함수 불러오기
    [PunRPC]
    public void WhenTaggerHitPlayerFunc()
    {
        gameMng.WhenTaggerHitPlayer();
    }
    //생성된 랜덤 숫자를 리스트에 넣어줌 
    [PunRPC]
    public void addRandomNum(int ranNum)
    {
        gameMng.RandomNums.Add(ranNum);
    }
    //생성된 랜덤 벡터를 리스트에 넣어줌
    [PunRPC]
    public void addRandomVec(Vector3 ranVec)
    {
        gameMng.RandomVecs.Add(ranVec);
    }
    //카메라 다시 1인칭으로 바꿔주기
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