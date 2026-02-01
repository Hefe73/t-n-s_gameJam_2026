using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] Transform cam;
    public float speed = 5f;
    public float rotateSpeed = 10f;

    public AudioSource footsteps;
    public float stepDistance = 0.67f;
    float distanceAccumulator;
    float h;
    float v;
    Vector3 lastPosition;
    bool hasLastPosition;

    public Animator animator; 


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

    public void PlaySound(float pitchRange)
    {
        footsteps.volume = Random.Range(0.85f, 1f);
        footsteps.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        footsteps.PlayOneShot(footsteps.clip);
    }


    void HandleFootsteps()
    {
        if (!hasLastPosition)
        {
            lastPosition = rb.position;
            hasLastPosition = true;
            return;
        }

        float distanceMoved = Vector3.Distance(rb.position, lastPosition);
        lastPosition = rb.position;

        if (distanceMoved < 0.00001f)
            return;

        distanceAccumulator += distanceMoved;

        if (distanceAccumulator >= stepDistance)
        {
            PlaySound(0.15f);
            distanceAccumulator = 0f;
        }
    }


    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        Vector3 camForward = Vector3.zero;
        Vector3 camRight = Vector3.zero;

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
            animator.SetBool("idle",false);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }

        if (moveDir.sqrMagnitude < 0.001f)
        {
            animator.SetBool("idle",true);
            hasLastPosition = false;
            distanceAccumulator = 0f;
        }
    }
}