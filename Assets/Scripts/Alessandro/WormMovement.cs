
using UnityEngine;

public class WormMovement : MonoBehaviour, IMinigameStartable
{
    bool gameActive = false;

    public void StartMinigame()
    {
        gameActive = true;
    }
    private Transform[] points;
    private int currentPointIndex = 0;
    private int targetPointIndex = 0;
    public float speed;
    private System.Action onMissedCallback;
    private bool hasMissed = false;

    private SpriteRenderer spriteRenderer;
    private const float flipThreshold = 0.01f;

    [Header("Flip (squirm)")]
    [Tooltip("Distancia recorrida antes de alternar el flip para simular ondulación")]
    public float flipDistance = 0.2f;
    [Tooltip("Si true, también forzar flip cuando la dirección X cambie de signo")]
    public bool flipOnDirectionChange = true;

    private float distanceSinceLastFlip = 0f;
    private float previousDirectionX = 0f;

    private void Awake()
    {
        // Obtener el SpriteRenderer de la raíz o hijos
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(Transform[] points, System.Action onMissedCallback)
    {
        this.points = points;
        this.onMissedCallback = onMissedCallback;

        currentPointIndex = Random.Range(0, points.Length);

        // Elegir un objetivo distinto al actual
        do
        {
            targetPointIndex = Random.Range(0, points.Length);
        }
        while (targetPointIndex == currentPointIndex);
    }

    void Update()
    {
        if (!gameActive)
            return;
        if (points == null || hasMissed) return;

        Vector3 targetPosition = points[targetPointIndex].position;

        Vector3 direction = (targetPosition - transform.position).normalized;

        float zigzagAmplitude = 0.5f;
        float zigzagFrequency = 10f;
        Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x);
        float sin = Mathf.Sin(Time.time * zigzagFrequency);
        Vector3 zigzag = perpendicular * sin * zigzagAmplitude * Time.deltaTime;

        Vector3 movementStep = direction * speed * Time.deltaTime + zigzag;

        // Mover
        transform.position += movementStep;

        // --- Flip: alternar cada X unidades recorridas para simular ondulación ---
        float travelled = movementStep.magnitude;
        distanceSinceLastFlip += travelled;

        if (spriteRenderer != null)
        {
            // Flip por distancia recorrida (ondulación)
            if (distanceSinceLastFlip >= flipDistance && flipDistance > 0f)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
                distanceSinceLastFlip = 0f;
            }

            // Opción: forzar flip según dirección X cuando cambie de signo
            if (flipOnDirectionChange)
            {
                if (Mathf.Abs(direction.x) > flipThreshold)
                {
                    if (previousDirectionX != 0f && Mathf.Sign(direction.x) != Mathf.Sign(previousDirectionX))
                    {
                        // asegurar que quede orientado coherentemente al cambiar de lado
                        spriteRenderer.flipX = direction.x < 0f;
                        distanceSinceLastFlip = 0f; // reiniciar contador para evitar flips inmediatos dobles
                    }

                    previousDirectionX = direction.x;
                }
            }
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            AvoidReturningToPreviousPoint();
        }
    }

    private void AvoidReturningToPreviousPoint()
    {
        int previousPointIndex = targetPointIndex;

        do
        {
            targetPointIndex = Random.Range(0, points.Length);
        }
        while (targetPointIndex == previousPointIndex);

        if (targetPointIndex == currentPointIndex)
        {
            hasMissed = true;
            onMissedCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}
