using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Script para controlar un ScrollRect de créditos usando un Slider.
// - Adjuntar a un GameObject (puede ser el mismo Slider) y asignar referencias en el Inspector.
// - Si "snapToSections" está activado, el scroll hará 'snap' a la sección más cercana al soltar el slider.

public class CreditsScroll : MonoBehaviour
{
    [Header("Referencias UI")]
    public ScrollRect scrollRect;     // ScrollRect que contiene el contenido de los créditos
    public Slider slider;             // Slider que controla la posición del ScrollRect
    public RectTransform content;     // Content del ScrollRect (para referencia)

    [Header("Secciones")]
    [Min(1)]
    public int sections = 1;          // Número de secciones/entradas a las que se quiere hacer scroll

    [Header("Comportamiento")]
    public bool snapToSections = true; // Si true, cuando se deje de arrastrar hace snap a la sección más cercana
    public float snapSpeed = 8f;       // Velocidad del snap (mayor = más rápido)

    // Estado interno
    bool isDraggingSlider = false;
    Coroutine snapCoroutine = null;
    bool isUpdatingFromSlider = false;

    void Start()
    {
        // Seguridad
        if (scrollRect == null) Debug.LogError("CreditsScroller: assign a ScrollRect in the inspector.");
        if (slider == null) Debug.LogError("CreditsScroller: assign a Slider in the inspector.");

        // Listener: cuando el slider cambie -> actualizar el ScrollRect
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderChanged);

        // Listener: cuando el ScrollRect cambie (por ejemplo scrolleo con la rueda) -> actualizar slider
        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrollChanged);

        // Inicializar valores
        UpdateSliderFromScroll();
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    // Cuando el usuario mueve el slider
    void OnSliderChanged(float value)
    {
        if (scrollRect == null) return;

        // Evitar bucles: marcar que la actualización viene del slider
        isUpdatingFromSlider = true;

        // Invertimos porque Slider 0->1 (izq->der) y verticalNormalizedPosition 0->1 (bottom->top)
        float v = 1f - value;
        scrollRect.verticalNormalizedPosition = v;

        isUpdatingFromSlider = false;
    }

    // Cuando el ScrollRect cambia (ej. arrastre directo) actualizamos el slider
    void OnScrollChanged(Vector2 _)
    {
        if (slider == null || scrollRect == null) return;
        if (isUpdatingFromSlider) return; // evitar loop

        slider.SetValueWithoutNotify(1f - scrollRect.verticalNormalizedPosition);
    }

    // Llamar desde EventTrigger (PointerDown) o si pones este script en el Slider, recibirá automáticamente estos eventos
    public void OnBeginDragSlider()
    {
        isDraggingSlider = true;
        if (snapCoroutine != null) { StopCoroutine(snapCoroutine); snapCoroutine = null; }
    }

    // Llamar desde EventTrigger (PointerUp)
    public void OnEndDragSlider()
    {
        isDraggingSlider = false;
        if (snapToSections)
        {
            if (snapCoroutine != null) StopCoroutine(snapCoroutine);
            snapCoroutine = StartCoroutine(SnapToNearest());
        }
    }

    // Calcula la posición normalizada de la sección más cercana y hace Lerp hasta ella
    IEnumerator SnapToNearest()
    {
        if (scrollRect == null) yield break;
        if (sections <= 1) yield break;

        float segment = 1f / (sections - 1); // distancia normalizada entre secciones

        // índice de la sección actual (basado en la posición invertida)
        float currentInv = 1f - scrollRect.verticalNormalizedPosition;
        int nearestIndex = Mathf.RoundToInt(currentInv / segment);
        nearestIndex = Mathf.Clamp(nearestIndex, 0, sections - 1);

        float target = 1f - (nearestIndex * segment);

        while (Mathf.Abs(scrollRect.verticalNormalizedPosition - target) > 0.001f)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, target, Time.deltaTime * snapSpeed);
            // actualizar slider sin disparar eventos
            slider.SetValueWithoutNotify(1f - scrollRect.verticalNormalizedPosition);
            yield return null;
        }

        // asegurar valor exacto al final
        scrollRect.verticalNormalizedPosition = target;
        slider.SetValueWithoutNotify(1f - target);
        snapCoroutine = null;
    }

    // Usa esto para sincronizar al iniciar (ej. si el ScrollRect cambió por otra razón)
    public void UpdateSliderFromScroll()
    {
        if (slider == null || scrollRect == null) return;
        slider.SetValueWithoutNotify(1f - scrollRect.verticalNormalizedPosition);
    }

#if UNITY_EDITOR
    // Métodos de ayuda para que, si el script está en el mismo GameObject que el Slider,
    // reciba automáticamente eventos PointerDown/Up. En builds estos métodos también funcionan si
    // el script está en el GameObject del Slider.
    public void OnPointerDown(BaseEventData data)
    {
        OnBeginDragSlider();
    }
    public void OnPointerUp(BaseEventData data)
    {
        OnEndDragSlider();
    }
#endif
}

