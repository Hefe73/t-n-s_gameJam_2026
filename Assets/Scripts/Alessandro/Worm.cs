using Unity.VisualScripting;
using UnityEngine;

public class Worm : MonoBehaviour
{
    public Sprite wormImage;
    public GameObject wormPrefab;
    public int maxWorms = 3;
    public float timeLimit = 10f;
    public Transform[] points;

    private int wormsAlive = 0;
    private int wormsKilled = 0;
    private int wormsMissed = 0;
    private float timer;
    private Camera cam;

    void Start()
    {
        // Validar referencias iniciales
        if (points == null || points.Length == 0)
        {
            Debug.LogError("No se asignaron puntos de destino en el inspector.");
            return;
        }

        if (wormPrefab == null)
        {
            Debug.LogError("No se asignó ningún prefab de gusano en el inspector.");
            return;
        }

        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError(
                "No se encontró la cámara principal (asegúrate de que haya una cámara con la etiqueta 'MainCamera').");
            return;
        }

        timer = timeLimit;
        SpawnInitialWorms();
    }
    
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f && (wormsAlive + wormsMissed + wormsKilled) < maxWorms)
        {
            SpawnInitialWorms();
            timer = timeLimit;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Worm"))
            {
                Destroy(hit.collider.gameObject);
                wormsKilled++;
                wormsAlive--;

                CheckGameStatus();
            }
        }
    }

    void SpawnInitialWorms()
    {
        // Asegurarnos de tener puntos
        if (points == null || points.Length == 0)
        {
            Debug.LogError("Los puntos de destino no están configurados. No se pueden generar gusanos.");
            return;
        }

        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject newWorm = Instantiate(wormPrefab, spawnPos, Quaternion.identity);

        newWorm.tag = "Worm";
        var sr = newWorm.GetComponent<SpriteRenderer>() ?? newWorm.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sprite = wormImage;

        WormMovement wormMov = newWorm.GetComponent<WormMovement>();
        wormMov.Initialize(points, OnWormMissed);
        wormsAlive++;
    }

    Vector3 GetRandomSpawnPosition()
    {
        int randomIndex = Random.Range(0, points.Length);
        //Vector3 offset = new Vector3(0.5f, 0.5f, 0f);
        return points[randomIndex].position; // + offset;
    }

    void OnWormMissed()
    {
        wormsAlive--;
        wormsMissed++;

        CheckGameStatus();
    }

    void CheckGameStatus()
    {
        if (wormsKilled == maxWorms)
        {
            Debug.Log("¡Has ganado!");
            EndGame(true);
        }
        else if (wormsMissed + wormsAlive == maxWorms)
        {
            Debug.Log("¡Has perdido! Algunos gusanos escaparon.");
            EndGame(false);
        }
        else if (wormsMissed + wormsKilled == maxWorms)
        {
            Debug.Log("¡Has perdido! Algunos gusanos escaparon.");
            EndGame(false);
        }
    }

    void EndGame(bool hasWon)
    {
        if (hasWon)
        {
            Debug.Log("¡Felicidades! Has ganado.");
        }
        else
        {
            Debug.Log("¡Perdiste!");
        }
    }
}