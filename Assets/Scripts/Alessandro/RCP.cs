using UnityEngine;
using TMPro;

public class RCP : MonoBehaviour
{

    public TextMeshPro QTEInstructions;
    public int maxQTEs = 5;
    public GameObject Hands;
    public float timeLimitPerQTE = 2.0f;
    public TextMeshPro timerText;
    public TextMeshPro errorText;

    private int currentQTE = 0; 
    private char currentKey;
    private float timeRemaining;
    private bool isQTEActive = false;
    private bool isGameActive = true;
    public int errors = 2;
    private int noops = 1;
    static float noopReturnTimer = 0f;

    private char[] possibleKeys = { 'A', 'S', 'D', 'W', 'E', 'Q' };

    void Start()
    {
        StartNewQTE();
    }

    void Update()
    {
  
        if (!isGameActive || !isQTEActive) return;
        
        MoveHands();
        timeRemaining -= Time.deltaTime;
        timerText.text = "Tiempo: " + Mathf.Ceil(timeRemaining) + "s";

        if (timeRemaining <= 0)
        {
            Debug.Log("¡Fallaste el QTE! Se acabó el tiempo.");
            HandleFailure();
            return;
        }

        if (errors != 0)
        {
            errorText.text = "Errores restantes: " + errors;
        }
            
        if (Input.anyKeyDown)
        {
            CheckInput();
        }
    }

    private void StartNewQTE()
    {
        if (currentQTE >= maxQTEs)
        {
            WinGame();
            return;
        }
        
        currentKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        QTEInstructions.text = currentKey.ToString();
        timeRemaining = timeLimitPerQTE;
        isQTEActive = true;

        Debug.Log($"Nuevo QTE iniciado. Presiona: {currentKey}");
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(currentKey.ToString().ToLower()))
        {
            QTEInstructions.color = Color.green;
            Debug.Log($"Tecla {currentKey} presionada correctamente.");
            currentQTE++;
            isQTEActive = false;
            StartNewQTE();
        }
        else
        {
            QTEInstructions.color = Color.red;
            errors--;
            errorText.text = "Errores restantes: " + errors;
        }

        if (errors <= 0)
        {
            Debug.Log($"Tecla equivocada presionada. ¡Fallaste el QTE!");
            HandleFailure();
        }
    }

    private void HandleFailure()
    {
        isGameActive = false;
        QTEInstructions.text = "¡Fallaste!";
        timerText.text = "Perdiste";
        Hands.SetActive(false);
    }

    private void WinGame()
    {
        isGameActive = false;
        QTEInstructions.text = "¡Ganaste!";
        timerText.text = "";
        MinigameManagerLaFalsa.Instance.MinigameFinished(3.0f);
        Debug.Log("¡Has completado todos los QTEs con éxito!");
    }
    
    private void MoveHands()
    {
        const float noopReturnDelay = 0.5f;

        if (Input.anyKeyDown && noops > 0)
        {
            noops--;
            Hands.transform.position -= new Vector3(0f, 0.02f, 0f);
            noopReturnTimer = noopReturnDelay;
        }
        
        if (noops == 0)
        {
            if (noopReturnTimer > 0f)
            {
                noopReturnTimer -= Time.deltaTime;
            }
            else if (!Input.anyKey)
            {
                noops++;
                Hands.transform.position += new Vector3(0f, 0.02f, 0f);
                QTEInstructions.color = Color.white;
            }
        }
    }
}