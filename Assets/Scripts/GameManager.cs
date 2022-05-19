using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> playableObjects;

    // Start is called before the first frame update
    void Start()
    {
        function();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void function()
    {
        playableObjects[Random.Range(0, 100)].AddComponent<PhotonView>();
    }
}
