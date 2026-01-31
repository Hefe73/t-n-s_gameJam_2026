using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Transform cam; // Asigna la cámara en el inspector (o se buscará Camera.main / "FollowCamera")
    public float speed = 5f;
    public float rotateSpeed = 10f;

    float h;
    float v;

    void Start()
    {
        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else
            {
                GameObject found = GameObject.Find("FollowCamera");
                if (found != null) cam = found.transform;
            }
        }
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        transform.position += moveDir * speed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }
}

