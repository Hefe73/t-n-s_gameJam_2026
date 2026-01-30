using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance {get; private set;}

    private void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //[SerializeField] private name cameraTarget;
    [SerializeField] private Camera camera;
    [SerializeField] private GameObject player;

    [SerializeField] private Transform player_tr;
    private Vector3 camera_rot;
    void Start()
    {
        camera = GameObject.Find("FollowCamera").GetComponent<Camera>(); 
        player = GameObject.FindGameObjectWithTag("Player");




    }

    // Update is called once per frame
    void Update()
    {
        if(player){

            player_tr = player.transform;
        }

        camera.transform.LookAt(player_tr);
    }
}
