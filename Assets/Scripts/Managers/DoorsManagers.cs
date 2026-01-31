using UnityEngine;
using UnityEngine.SceneManagement;
public class DoorsManagers : MonoBehaviour
{

    [SerializeField] private string sceneName;
    [SerializeField] private bool imLobby;


    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other) {
        if(imLobby)
            SceneManager.LoadScene(GameLoopManager.Instance.RoomsNameList[GameLoopManager.Instance.patientsHealed]);
        else
            SceneManager.LoadScene(sceneName);
    }
}
