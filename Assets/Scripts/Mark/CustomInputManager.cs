using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomInputManager : MonoBehaviour
{
    //public static CustomInputManager instance_;
    public GameObject menu_;
    public GameObject ui_;
    
    private void Awake()
    {
        /*
        if (instance_ == null)
        {
            instance_ = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
         */
        
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            menu_.SetActive(true);
            ui_.SetActive(false);
        }
    }
}
