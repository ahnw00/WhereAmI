using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody RB;
    public Animator AN;
    public PhotonView PV;
    public TextMeshPro NickNameText;
    public Transform cameraArm; 
    public Transform playerTr;

    bool isJump = false;
    float speed = 4f;
    Vector3 curPos;
    Quaternion curRot;
    Vector2 moveInput;
    Vector2 mouseDelta;

    void Awake()
    {
        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
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
    }

    void Update()
    {
        if (PV.IsMine)
        {
            GetInput();
            LookAt(cameraArm);
            LookAt(NickNameText.transform);
            Move();
            Jump();

            // if (horizontalAxis != 0 || verticalAxis != 0)
            // {
            //     AN.SetBool("walk", true);
            // }
            // else AN.SetBool("walk", false);
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else 
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            //playerTr.rotation = Quaternion.Slerp(playerTr.rotation, Quaternion.Euler(curRotation), Time.deltaTime * 10);
            playerTr.rotation = Quaternion.Slerp(playerTr.rotation, curRot, Time.deltaTime * 10);
            LookAt(NickNameText.transform);
        }
    }

    public void OnCollisionEnter(Collision collision) // 충돌 감지, collision은 그 충돌체가 누구인지
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
        // isKeyDown[(int)DownKey.RUN] = Input.GetButton("Run");
        // isKeyDown[(int)DownKey.PICK] = Input.GetButtonDown("Pick");
        // isKeyDown[(int)DownKey.SWAP1] = Input.GetButtonDown("SwapGun");
        // isKeyDown[(int)DownKey.SWAP3] = Input.GetButtonDown("SwapKnife");

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

        // anim.SetBool("isWalk", moveInput.magnitude != 0);
        // anim.SetBool("isRun", isKeyDown[(int)DownKey.RUN]);
    }

    void Jump()
    {
        // ↑ 점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) 
        {
            isJump = true;
            //AN.SetBool("jump", true);
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

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);


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
}