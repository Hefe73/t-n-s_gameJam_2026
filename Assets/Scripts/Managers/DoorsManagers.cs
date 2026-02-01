using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DoorsManagers : MonoBehaviour
{

    [SerializeField] private string sceneName;
    [SerializeField] private bool imLobby;
    public CameraEffects camEffects;
    private bool alreadyDoingAnimation = false;
    
    // Update is called once per frame 
    void Update()
    {
        if (camEffects && camEffects.doorAnimationFinished )
        {
            alreadyDoingAnimation = false;
            CameraManager cam = CameraManager.Instance;
            cam.isFollowingPlayer = true;
            if (imLobby)
            {
                SceneManager.LoadScene(GameLoopManager.Instance.RoomsNameList[GameLoopManager.Instance.patientsHealed]);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

        }
    }

    private void Start()
    {

        if (!camEffects)
        {
            camEffects = FindFirstObjectByType<CameraEffects>();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (camEffects && !alreadyDoingAnimation)
        {
            alreadyDoingAnimation = true;
            camEffects.DoDoorAnimation();
        }
        else
        {
            CameraManager cam = CameraManager.Instance;
            cam.isFollowingPlayer = true;
            if (imLobby)
            {
                SceneManager.LoadScene(GameLoopManager.Instance.RoomsNameList[GameLoopManager.Instance.patientsHealed]);
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.bIsOperating_ = true;
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName);
                GameLoopManager.Instance.bIsOperating_ = false;
            }

            
        }
    }
}
