using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private Camera camera;
    [SerializeField] private GameObject player;
    private Transform player_tr;
    public bool isFollowingPlayer = true;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCameraPlayer();
    }

    void Update()
    {
        if (player_tr && camera && isFollowingPlayer)
        {
            camera.transform.LookAt(player_tr);
        }
    }

    public void FindCameraPlayer()
    {
        camera = GameObject.Find("FollowCamera")?.GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            player_tr = player.transform;
    }
}
