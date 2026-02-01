using UnityEngine;
using UnityEngine.UI;

public class OxygenMinigameUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform barContainer;    
    public RectTransform fillRect;        
    public RectTransform targetRect;      
    public Button valveButton;             

    [Header("Target setup (copy from reference on start)")]
    public RectTransform targetReference;  
    public bool copyTargetFromReferenceOnEnable = true;

    [Header("Settings")]
    [Range(0f, 1f)] public float fill = 0f;
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
        if (barContainer != null)
            barWidth = barContainer.rect.width;

        SetFill01(0f);

        if (valveButton != null)
        {
            valveButton.onClick.RemoveListener(ToggleValve);
            valveButton.onClick.AddListener(ToggleValve);
        }
    }

    void OnEnable()
    {
        ResetGame();

        if (copyTargetFromReferenceOnEnable && targetReference != null && targetRect != null)
        {
            CopySizeAndPos(targetReference, targetRect);
        }
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
        t01 = Mathf.Clamp01(t01);

        if (barContainer != null)
            barWidth = barContainer.rect.width - 250.0f;

        float w = barWidth * t01;

        if (fillRect == null) return;

        var size = fillRect.sizeDelta;
        size.x = w;
        fillRect.sizeDelta = size;
    }

    void PositionTargetZone()
    {
        if (barContainer == null || targetRect == null) return;

        greenMin = Mathf.Clamp01(greenMin);
        greenMax = Mathf.Clamp01(greenMax);
        if (greenMax < greenMin) (greenMin, greenMax) = (greenMax, greenMin);

        barWidth = barContainer.rect.width;

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

        if (uiSoundPlayer != null) uiSoundPlayer.PlaySoundWin();
        Debug.Log("Oxigeno administrado correctamente");

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

    static void CopySizeAndPos(RectTransform src, RectTransform dst)
    {
        dst.anchoredPosition = src.anchoredPosition;
        dst.sizeDelta = src.sizeDelta;
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
