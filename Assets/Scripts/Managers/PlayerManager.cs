using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] Transform cam; 
    public float speed = 5f;
    public float rotateSpeed = 10f;

    float h;
    float v;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

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
    }

    void FixedUpdate()
    {
        Vector3 camForward = Vector3.zero;
        Vector3 camRight   = Vector3.zero;

        if (cam != null)
        {
            camForward = cam.forward;
            camForward.y = 0f;
            camForward.Normalize();

            camRight = cam.right;
            camRight.y = 0f;
            camRight.Normalize();
        }
        else
        {
            camForward = Vector3.forward;
            camRight = Vector3.right;
        }

        Vector3 moveDir = camForward * v + camRight * h;

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 targetPos = rb.position + moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
    }
}
