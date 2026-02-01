using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NPC_Interaction : MonoBehaviour
{

    [SerializeField] private GameObject pressE;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        pressE.SetActive(true);
    }

    void OnTriggerExit(Collider other) {
        pressE.SetActive(false);
    }
}
