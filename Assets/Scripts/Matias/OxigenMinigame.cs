using UnityEngine;
using UnityEngine.UI;

public class OxygenMinigameUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform barContainer;   // el RectTransform del fondo de la barra (BarBG)
    public RectTransform fillRect;       // el RectTransform del Fill
    public RectTransform targetRect;     // el RectTransform del TargetZone
    public Button valveButton;           // botón de la válvula (ValveButton)

    [Header("Settings")]
    [Range(0f, 5f)] public float fill = 0f;
    public float fillSpeed = 0.3f;

    [Range(0f, 1f)] public float greenMin = 0.55f;
    [Range(0f, 1f)] public float greenMax = 0.7f;

    public bool valveOpen = false;
    public bool finished = false;

    [Header("Audio")]
    public AudioSource valveSoundOpen;
    public AudioSource valveSoundClose;
    public AudioSource fillAir;
    public PlayUISound uiSoundPlayer;

    float barWidth;

    void Awake()
    {
        barWidth = barContainer.rect.width;

        SetFill01(0f);

        if (valveButton != null)
            valveButton.onClick.AddListener(ToggleValve);
    }

    void OnEnable()
    {
        ResetGame();
        PositionTargetZone();
    }

    void Update()
    {
        if (finished) return;

        if (valveOpen)
        {
            fill += fillSpeed * Time.deltaTime;

            if (fill >= 1f)
            {
                fill = 1f;
                StopAir();
                PlayClose();
                Fail();
                return;
            }

            UpdateFillVisual();
        }

        if (!valveOpen && fill >= greenMin && fill <= greenMax)
        {
            Success();
        }
    }

    public void ToggleValve()
    {
        if (finished) return;

        valveOpen = !valveOpen;

        if (valveOpen)
        {
            PlayOpen();
            if (fillAir != null) fillAir.Play();
        }
        else
        {
            StopAir();
            PlayClose();
        }
    }

    void UpdateFillVisual()
    {
        fill = Mathf.Clamp01(fill);
        SetFill01(fill);
    }

    void SetFill01(float t01)
    {
        float w = barWidth * Mathf.Clamp01(t01);
        var size = fillRect.sizeDelta;
        size.x = w;
        fillRect.sizeDelta = size;
    }

    void PositionTargetZone()
    {
        greenMin = Mathf.Clamp01(greenMin);
        greenMax = Mathf.Clamp01(greenMax);
        if (greenMax < greenMin) (greenMin, greenMax) = (greenMax, greenMin);

        float size01 = greenMax - greenMin;
        float zoneWidth = barWidth * size01;

        var s = targetRect.sizeDelta;
        s.x = zoneWidth;
        targetRect.sizeDelta = s;

        var p = targetRect.anchoredPosition;
        p.x = greenMin * barWidth;
        targetRect.anchoredPosition = p;
    }

    void Success()
    {
        finished = true;
        valveOpen = false;
        StopAir();

        var mgr = MinigameManagerLaFalsa.Instance;
        if (mgr == null)
        {
            Debug.LogError("MinigameManagerLaFalsa.Instance es NULL. ¿Hay un MinigameManagerLaFalsa en la escena y se inicializa Instance?");
            return;
        }

        mgr.MinigameFinished(2.0f);
    }

    void Fail()
    {
        Debug.Log("Exceso de oxigeno - reiniciando");
        if (uiSoundPlayer != null) uiSoundPlayer.PlaySoundLoose();
        ResetGame();
    }

    void ResetGame()
    {
        fill = 0f;
        valveOpen = false;
        finished = false;
        SetFill01(0f);
        StopAir();
    }

    void PlayOpen()
    {
        if (valveSoundOpen != null && valveSoundOpen.clip != null)
            valveSoundOpen.PlayOneShot(valveSoundOpen.clip);
    }

    void PlayClose()
    {
        if (valveSoundClose != null && valveSoundClose.clip != null)
            valveSoundClose.PlayOneShot(valveSoundClose.clip);
    }

    void StopAir()
    {
        if (fillAir != null && fillAir.isPlaying) fillAir.Stop();
    }
}
