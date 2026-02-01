using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Suture : MonoBehaviour
{
    public Transform[] suturePoints;
    public GameObject markerPrefab;
    public TextMeshPro timerText;
    public float timeLimit = 10f;

    private GameObject activeMarker;
    private bool[] isSutured;
    private int currentPointIndex = 0;
    private LineRenderer lineRenderer;
    private float remainingTime;
    private bool gameActive = true;

    public AudioSource sutureSfx;
    public PlayUISound uiSoundPlayer;

    private void Start()
    {
        isSutured = new bool[suturePoints.Length];
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        remainingTime = timeLimit;
        UpdateMarker();
    }

    private void Update()
    {
        if (!gameActive) return;

        UpdateTimer();

        if (Input.GetMouseButtonDown(0))
        {
            CheckSuturePointClick();
        }
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        timerText.text = "Tiempo Restante: " + Mathf.Ceil(remainingTime).ToString() + "s";

        if (remainingTime <= 0 && gameActive)
        {
            Debug.Log("¡Se acabó el tiempo! Fallaste en completar la sutura.");
            OnTimeUp();
        }
    }

    private void CheckSuturePointClick()
    {
        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Transform clickedPoint = hit.collider.transform;

            if (clickedPoint == suturePoints[currentPointIndex])
            {
                sutureSfx.volume = Random.Range(0.85f, 1f);
                sutureSfx.pitch = Random.Range(1f - 0.15f, 1f + 0.15f);
                sutureSfx.PlayOneShot(sutureSfx.clip);
                
                isSutured[currentPointIndex] = true;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(currentPointIndex, clickedPoint.position);

                currentPointIndex++;
                UpdateMarker();

                Debug.Log($"Punto {currentPointIndex}/{suturePoints.Length} suturado exitosamente");

                if (currentPointIndex == suturePoints.Length)
                {
                    Debug.Log("¡Has completado la sutura!");
                    OnSutureComplete();
                }
            }
        }
    }

    private void UpdateMarker()
    {
        if (activeMarker != null)
        {
            Destroy(activeMarker);
        }

        if (currentPointIndex < 1)
        {
            activeMarker = Instantiate(markerPrefab);

            activeMarker.transform.SetParent(suturePoints[0]);

            activeMarker.transform.localPosition = new Vector3(-0.54f, 0.6f, 0f);
            activeMarker.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
            activeMarker.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        }
    }

    private void OnSutureComplete()
    {
        Debug.Log("¡Minijuego completado con éxito!");
        gameActive = false;
        timerText.text = "¡Sutura Completa!";
        uiSoundPlayer.PlaySoundWin();
    }

    private void OnTimeUp()
    {
        gameActive = false;
        timerText.text = "¡Se acabó el tiempo!";
        uiSoundPlayer.PlaySoundLoose();
    }
}