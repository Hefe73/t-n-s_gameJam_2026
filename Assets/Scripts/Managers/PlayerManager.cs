using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    [SerializeField] GameObject Player;
    public float speed = 5f;
    public float h;
    public float v;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            //Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0f, v);
        transform.Translate(movement * speed * Time.deltaTime, Space.Self);

  
        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
