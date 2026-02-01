using UnityEngine;

public class FollowCircle : MonoBehaviour
{
    public float speed = 3f;
    public float directionChangeInterval = 1.5f;

    // Maximum distance from starting position (XZ plane)
    public float maxDistanceFromStart = 3f;

    public float timeToWin = 2f;
    public float timeToStart = 1f;

    private Vector3 moveDirection;
    private float timer;
    private float timeInside;
    private bool bIsFinished;
    private bool bIsStarted;

    private SphereCollider sc_;
    private Vector3 startPosition;

    public AudioSource steroscopeSfx;
    public PlayUISound uiSoundPlayer;

    void Start()
    {
        sc_ = GetComponentInChildren<SphereCollider>();
        startPosition = transform.position;

        PickNewDirection();

        timer = 0f;
        timeInside = 0f;
        bIsStarted = false;
        bIsFinished = false;
    }

    void Update()
    {
        if (bIsFinished) return;

        // Delay before start
        if (!bIsStarted)
        {
            timer += Time.deltaTime;
            if (timer >= timeToStart)
            {
                steroscopeSfx.loop = true;
                steroscopeSfx.Play();
                bIsStarted = true;
                timer = 0f;
            }
            return;
        }

        // Move in world space
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Change direction occasionally
        timer += Time.deltaTime;
        if (timer >= directionChangeInterval)
        {
            PickNewDirection();
            timer = 0f;
        }

        KeepInsideRadius();

        // Mouse → world (correct depth)
        Camera cam = Camera.main;
        float depth = Vector3.Dot(
            sc_.transform.position - cam.transform.position,
            cam.transform.forward
        );

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = depth;
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        // Sphere world data
        Vector3 center = sc_.transform.TransformPoint(sc_.center);
        float radius = sc_.radius * Mathf.Max(
            sc_.transform.lossyScale.x,
            sc_.transform.lossyScale.y,
            sc_.transform.lossyScale.z
        );

        // Detection
        if (Vector3.Distance(mouseWorldPos, center) <= radius)
        {
            timeInside += Time.deltaTime;

            if (timeInside >= timeToWin)
            {
                bIsFinished = true;
                steroscopeSfx.Stop();
                uiSoundPlayer.PlaySoundWin();
                MinigameManagerLaFalsa.Instance.MinigameFinished(2.0f);
            }
        }
        else
        {
            bIsFinished = true;
            Debug.Log("You lost");
        }
    }

    void PickNewDirection()
    {
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(dir2D.x, 0f, dir2D.y);
    }

    void KeepInsideRadius()
    {
        Vector3 offset = transform.position - startPosition;

        // Ignore Y (movement happens in XZ)
        offset.y = 0f;

        if (offset.magnitude > maxDistanceFromStart)
        {
            // Push back inside + reflect direction
            Vector3 normal = offset.normalized;
            moveDirection = Vector3.Reflect(moveDirection, normal).normalized;

            transform.position = startPosition + normal * maxDistanceFromStart;
        }
    }
}
