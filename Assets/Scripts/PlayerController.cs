using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum DownKey
{
    JUMP,
    RUN,
    PICK,
    SWAP1,
    SWAP2,
    SWAP3
}


public class PlayerController : Player3D
{
    public Transform cameraArm; 
    public Transform playerTr;

    bool[] isKeyDown = new bool[6]; // 점프 키 + 달리기 키 + 줍기 키 + 무기 키 x 3

    Vector2 moveInput;
    Vector2 mouseDelta;

    void Update()
    {
        GetInput(); // 입력받고
        LookAt(); // 보고
        Move(); // 움직인다
        Jump(); // 점프
        //Pick(); // 줍기
        //Swap(); // 무기 변경
    }
    
    private void GetInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y);

        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");

        mouseDelta = new Vector2(x, y);

        isKeyDown[(int)DownKey.JUMP] = Input.GetButtonDown("Jump");
        isKeyDown[(int)DownKey.RUN] = Input.GetButton("Run");
        isKeyDown[(int)DownKey.PICK] = Input.GetButtonDown("Pick");
        isKeyDown[(int)DownKey.SWAP1] = Input.GetButtonDown("SwapGun");
        isKeyDown[(int)DownKey.SWAP3] = Input.GetButtonDown("SwapKnife");

        attackDelay += Time.deltaTime;
    }
    private void LookAt()
    {
        // 현재 내 카메라의 각도를 구해오고
        Vector3 cameraAngle = cameraArm.rotation.eulerAngles;

        float x = cameraAngle.x - mouseDelta.y;

        x = x < 180f ? Mathf.Clamp(x, -1f, 70f) : Mathf.Clamp(x, 335f, 361f);

        cameraArm.rotation = Quaternion.Euler(new Vector3(x, cameraAngle.y + mouseDelta.x, cameraAngle.z));
    }

    private void Move()
    {
        Vector3 lookforward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
        Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;

        playerTr.forward = lookforward;

        Vector3 moveVec = lookforward * moveInput.y + lookRight * moveInput.x;

        transform.position += moveVec * Time.deltaTime * 10f;

        anim.SetBool("isWalk", moveInput.magnitude != 0);
        anim.SetBool("isRun", isKeyDown[(int)DownKey.RUN]);
    }

    private void Jump()
    {
        if(isKeyDown[(int)DownKey.JUMP] && !isJump)
        {
            anim.SetTrigger("Jump");
            rigid.AddForce(Vector3.up * 10f, ForceMode.Impulse);
            isJump = true;
        }
    }

    /*private void Pick()
    {
        if(isKeyDown[(int)DownKey.PICK] && nearObject != null)
        {
            Weapon w = nearObject.GetComponent<Weapon>();
            hasWeapon[(int)w.weaponType] = true;

            playerWeapons[(int)w.weaponType].damage = w.damage;
            playerWeapons[(int)w.weaponType].weaponType = w.weaponType;
            playerWeapons[(int)w.weaponType].attackDelay = w.attackDelay;

            Destroy(nearObject);
        }
    }*/

    // private void Swap()
    // {
    //     if (isKeyDown[(int)DownKey.SWAP1] && hasWeapon[(int)WeaponType.GUN])
    //     {
    //         // 총으로 변경
    //         playerWeapons[(int)WeaponType.KNIFE].UnsetWeapon();
    //         playerWeapons[(int)WeaponType.GUN].SetWeapon();
    //         currWeapon = playerWeapons[(int)WeaponType.GUN];
    //     }
    //     else if(isKeyDown[(int)DownKey.SWAP3] && hasWeapon[(int)WeaponType.KNIFE])
    //     {
    //         // 칼로 변경
    //         playerWeapons[(int)WeaponType.GUN].UnsetWeapon();
    //         playerWeapons[(int)WeaponType.KNIFE].SetWeapon();
    //         currWeapon = playerWeapons[(int)WeaponType.KNIFE];
    //     }
    // }

    // public void Attack()
    // {
    //     if(nearEnemy != null && currWeapon != null && currWeapon.attackDelay < attackDelay)
    //     {
    //         anim.SetTrigger("Attack");
    //         nearEnemy.EnemyHit(currWeapon.damage);
    //         attackDelay = 0f;
    //         Debug.Log("공격하자~");
    //     }
    // }
}

