using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tagger : MonoBehaviour
{
    [SerializeField] Camera cam;
    PhotonView PV;

     void Awake()
    {
        PV = GetComponent<PhotonView>();  
    }

    public void HitPlayer()
    {
        Ray ray = cam.ViewportPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //레이에 맞은 물체가 player인가? 맞다면 player 정보 갖고오기
            //hit.collider.gameObject.GetComponent<Player>()?
        }
    }
}
