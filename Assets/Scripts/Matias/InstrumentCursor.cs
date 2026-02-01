using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InstrumentCursor : MonoBehaviour
{
    public Image cursorIcon;
    public Canvas canvas;

    private RectTransform iconRect;
    private Sprite currentInstrument;

    private Vector2 currentHotspotOffset = Vector2.zero;

    void Awake()
    {
        iconRect = cursorIcon.GetComponent<RectTransform>();
    }

    void Start()
    {
        cursorIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!cursorIcon.gameObject.activeSelf) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, cam, out Vector2 localPoint);

        iconRect.anchoredPosition = localPoint + currentHotspotOffset;
    }

    public void SelectInstrument(Sprite instrumentSprite, Vector2 hotspotOffset)
    {
        if (instrumentSprite == null) return;

        currentInstrument = instrumentSprite;
        currentHotspotOffset = hotspotOffset;

        cursorIcon.sprite = instrumentSprite;
        cursorIcon.color = Color.white;
        cursorIcon.SetNativeSize();
        cursorIcon.gameObject.SetActive(true);

        cursorIcon.transform.SetAsLastSibling();
        Cursor.visible = false;
    }

    public void ClearInstrument()
    {
        currentInstrument = null;
        currentHotspotOffset = Vector2.zero;
        cursorIcon.gameObject.SetActive(false);
        Cursor.visible = true;
    }

    public Sprite GetCurrentInstrument() => currentInstrument;
}
