using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tagger : MonoBehaviour
{
    [SerializeField] Camera cam;
    PhotonView PV;
    GameManager gameMng;


     void Awake()
    {
        PV = GetComponent<PhotonView>();  
        gameMng = FindObjectOfType<GameManager>();
    }


    public void HitPlayer(Camera cam)
    {
        Ray ray = cam.ViewportPointToRay(Input.mousePosition);
        RaycastHit hitObj;
        Debug.DrawRay(transform.position, ray.direction * 150, Color.red);

        if (Physics.Raycast(ray, out hitObj))
        {
            
            Debug.Log(hitObj.transform.gameObject);
            //레이에 맞은 물체가 player인가? 맞다면 player 정보 갖고오기
            if(hitObj.collider.gameObject.GetComponent<Player>())
            {
                for(int i = 0; i < gameMng.players.Count; i++)
                {
                    if(gameMng.players[i] == hitObj.collider.gameObject.GetComponent<Player>())
                    {
                        gameMng.nextTagger = i;
                        break;
                    }
                }
            }
        }
    }
}
