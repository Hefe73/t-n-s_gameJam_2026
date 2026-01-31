using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoopManager : MonoBehaviour
{

    public static GameLoopManager Instance { get; private set; }

    [SerializeField] public int patientsHealed;
    [SerializeField] public string[] RoomsNameList;
    [SerializeField] public float GAS;

    [SerializeField] Slider gas_slider;

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
        gas_slider.value = GAS;
    }

    void PatientHealed(){
        patientsHealed++;
    }
}
