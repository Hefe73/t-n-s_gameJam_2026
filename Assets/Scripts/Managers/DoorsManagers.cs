using UnityEngine;
using UnityEngine.SceneManagement;
public class DoorsManagers : MonoBehaviour
{

    [SerializeField] private string sceneName;

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other) {
        SceneManager.LoadScene(sceneName);
    }
}
