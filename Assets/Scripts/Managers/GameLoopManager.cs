using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoopManager : MonoBehaviour
{

    public static GameLoopManager Instance { get; private set; }

    [SerializeField] public int patientsHealed;
    [SerializeField] public string[] RoomsNameList;
    [SerializeField] public float GAS;
    [SerializeField] public float decreaseSpeed;
    public bool bIsOperating_;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bIsOperating_)
        {
            GAS -= Time.deltaTime * decreaseSpeed;
        }
    }

    void PatientHealed(){
        patientsHealed++;
        bIsOperating_ = false;
    }

    void StartOperation()
    {
        bIsOperating_ = true;
    }
    
    public void SetOxygen(float value)
    {
        GAS = Mathf.Clamp(value, 0.0f, 100.0f);
    }

    public void AddOxygen(float amount)
    {
        SetOxygen(GAS + amount);
    }
}
