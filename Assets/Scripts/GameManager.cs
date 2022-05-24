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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("CamToWaitPointCoroutine");
        }
    }

    public void RandomFirstTugger()
    {

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
