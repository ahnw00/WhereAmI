using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3D : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;

    [SerializeField] public GameObject nearObject;
    [SerializeField] public bool[] hasWeapon = new bool[3];

    public bool isJump;
    public float attackDelay;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        isJump = false;
        //StartCoroutine(FindEnemy());
    }

    /*IEnumerator FindEnemy()
    {
        while(true)
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position, transform.forward * 15f, Color.blue, 1f);

            if(Physics.Raycast(transform.position, transform.forward, out hit, 15f))
            {
                if(hit.transform.tag == "Enemy")
                {
                    nearEnemy = hit.transform.GetComponent<Enemy>();
                } 
            }
            else
            {
                nearEnemy = null;
            }

            yield return new WaitForEndOfFrame();
        }
        
    }*/

    public void OnCollisionEnter(Collision collision) // 충돌 감지, collision은 그 충돌체가 누구인지
    {
        if(collision.gameObject.tag == "Floor")
        {
            isJump = false;
        }
    }
    public void OnTriggerStay(Collider other) 
    {
        if(other.tag == "Weapon") //weapon 대신 랜덤아이템
        {
            nearObject = other.gameObject;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}