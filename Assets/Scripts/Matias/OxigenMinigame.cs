using UnityEngine;
using UnityEngine.InputSystem;

public class OxygenMinigame : MonoBehaviour
{
    [Header("Bar References")]
    public Transform fillBar;
    public Transform targetZone;

    [Header("Settings")]
    [Range(0f, 1f)] public float fill = 0f;
    public float fillSpeed = 0.3f;

    [Range(0f, 1f)] public float greenMin = 0.55f;
    [Range(0f, 1f)] public float greenMax = 0.7f;

    public bool valveOpen = false;
    public bool finished = false;

    void Start()
    {
        ResetGame();
        PositionTargetZone();
    }

    void Update()
    {
        if (finished) return;

        if (WasPressedThisFrame())
        {
            valveOpen = !valveOpen;
        }

        if (valveOpen)
        {
            fill += fillSpeed * Time.deltaTime;

            if (fill >= 1f)
            {
                Fail();
                return;
            }
        }

        UpdateFillVisual();

        if (!valveOpen && fill >= greenMin && fill <= greenMax)
        {
            Success();
        }
    }

    void UpdateFillVisual()
    {
        fill = Mathf.Clamp01(fill);

        fillBar.localScale = new Vector3(fill, 0.11f, 1f);
        fillBar.localPosition = new Vector3(-0.434f, 1.283f, -0.01f);
    }

    void PositionTargetZone()
    {
        float size = greenMax - greenMin;
        float center = (greenMin + greenMax) * 0.5f;

        targetZone.localScale = new Vector3(0.15f, 0.15f, 1.0f);
        targetZone.localPosition = new Vector3(center - 0.5f, 0f, -0.01f);
    }

    void Success()
    {
        finished = true;
        valveOpen = false;
        Debug.Log("Oxigeno administrado correctamente");
    }

    void Fail()
    {
        Debug.Log("Exceso de oxigeno - reiniciando");
        ResetGame();
    }

    void ResetGame()
    {
        fill = 0f;
        valveOpen = false;
        finished = false;
        UpdateFillVisual();
    }

    bool WasPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        return false;
    }
}
