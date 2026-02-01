using UnityEngine;
using UnityEngine.EventSystems;

public class OperationArea : MonoBehaviour, IPointerClickHandler
{
    public InstrumentCursor cursorManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        var instrument = cursorManager.GetCurrentInstrument();
        if (instrument == null) return;

        Debug.Log("Usando instrumento: " + instrument.name);

    }
}
