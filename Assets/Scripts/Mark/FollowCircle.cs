using UnityEngine;

public class FollowCircle : MonoBehaviour
{

    public float speed = 3f;
    public float directionChangeInterval = 1.5f;
    
    public Vector2 minPosition; 
    public Vector2 maxPosition; 
    public float timeToWin;
    public float timeToStart;
    
    private Vector2 moveDirection;
    private float timer;
    private float timeInside;
    private bool bIsFinished;
    private bool bIsStarted;
    private CircleCollider2D cc_;
    
    public AudioSource steroscopeSfx;
    public PlayUISound uiSoundPlayer;
    
    void Start()
    {
        PickNewDirection();
        cc_ = gameObject.GetComponentInChildren<CircleCollider2D>();
        timer = 0.0f;
        timeInside = 0.0f;
        bIsStarted = false;
        bIsFinished = false;
    }

    void Update()
    {
        if (bIsFinished) return;

        if (!bIsStarted)
        {
            timer += Time.deltaTime;
            if (timer >= timeToStart)
            {
               steroscopeSfx.PlayOneShot(steroscopeSfx.clip);
                bIsStarted = true;
                timer = 0.0f;
            }

            return;
        }
        
        transform.Translate(moveDirection * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= directionChangeInterval)
        {
            PickNewDirection();
            timer = 0f;
        }
        KeepInsideScreen();
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(
            Camera.main.transform.position.z - transform.position.z
        );
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        if (cc_.OverlapPoint(mouseWorldPos))
        {
            timeInside += Time.deltaTime;
            if (timeInside >= timeToWin)
            {
                Debug.Log("Game finished!");
                bIsFinished = true;
                steroscopeSfx.Stop();
                uiSoundPlayer.PlaySoundWin();
            }
        }
        else
        {
            Debug.Log("Game Lost");
            uiSoundPlayer.PlaySoundLoose();
            bIsFinished = true;
            steroscopeSfx.Stop();
        }
    }

    void PickNewDirection()
    {
        moveDirection = Vector2.Lerp(
            moveDirection,
            Random.insideUnitCircle.normalized,
            0.3f
        ).normalized;

    }

    void KeepInsideScreen()
    {
        Vector3 pos = transform.position;
        Vector3 viewPos = Camera.main.WorldToViewportPoint(pos);

        if (pos.x < minPosition.x || pos.x > maxPosition.x)
            moveDirection.x *= -1;

        if (pos.y < minPosition.y || pos.y > maxPosition.y)
            moveDirection.y *= -1;
    }
}
