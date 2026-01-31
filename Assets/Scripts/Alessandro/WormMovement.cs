using UnityEngine;

public class WormMovement : MonoBehaviour
{
    private Transform[] points;
    private int currentPointIndex = 0; 
    private int targetPointIndex = 0;
    public float speed;
    private System.Action onMissedCallback; 

    private bool hasMissed = false;

    public void Initialize(Transform[] points, System.Action onMissedCallback)
    {
        this.points = points;
        this.onMissedCallback = onMissedCallback;
        
        currentPointIndex = Random.Range(0, points.Length);
    }

    void Update()
    {
        if (points == null || hasMissed) return;
        
        Vector3 targetPosition = points[currentPointIndex].position;
        targetPointIndex = (currentPointIndex + 1) % points.Length;
        if ((targetPointIndex == currentPointIndex))
        {
            currentPointIndex = (currentPointIndex + 1) % points.Length;
            targetPosition = points[currentPointIndex].position;
        }
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        float zigzagAmplitude = 0.5f;
        float zigzagFrequency = 10f; 
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
        float sin = Mathf.Sin(Time.time * zigzagFrequency);
        Vector3 zigzag = perpendicular * sin * zigzagAmplitude * Time.deltaTime;
        Vector3 movementStep = direction * speed * Time.deltaTime + zigzag;
        
        transform.position += movementStep;
        
        if ((transform.position - targetPosition).magnitude < 0.1f && currentPointIndex != targetPointIndex)
        {
            hasMissed = true;
            onMissedCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}