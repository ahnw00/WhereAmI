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

    float speed = 4f;
    int numOfContactedCol;
    Vector3 curPos;



    void Awake()
    {
        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }


    void Update()
    {
        if (PV.IsMine)
        {
            // 앞뒤좌우 이동
            float horizontalAxis = Input.GetAxisRaw("Horizontal");
            float verticalAxis = Input.GetAxisRaw("Vertical");
            RB.velocity = new Vector3(speed * horizontalAxis, 0, speed * verticalAxis);

            // if (horizontalAxis != 0 || verticalAxis != 0)
            // {
            //     AN.SetBool("walk", true);
            // }
            // else AN.SetBool("walk", false);


            // ↑ 점프, 바닥체크
            Collider[] results = new Collider[1];
            numOfContactedCol = Physics.OverlapSphereNonAlloc((Vector3)transform.position + new Vector3(0, -0.5f, 0), 0.07f, results, 1 << LayerMask.NameToLayer("Ground"));
            if (Input.GetKeyDown(KeyCode.Space) && numOfContactedCol == 1) 
            {
                //AN.SetBool("jump", true);
                PV.RPC("JumpRPC", RpcTarget.All);
            }

        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    [PunRPC]
    void JumpRPC()
    {
        RB.AddForce(Vector3.up * 300);
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
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        }
    }
}