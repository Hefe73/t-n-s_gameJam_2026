using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InstrumentButton : MonoBehaviour
{
    public InstrumentCursor cursorManager;
    public Image iconImage;

    [Header("Hotspot offset (anchored)")]
    public Vector2 hotspotOffset = Vector2.zero;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (cursorManager == null || iconImage == null || iconImage.sprite == null)
            return;

        cursorManager.SelectInstrument(iconImage.sprite, hotspotOffset);
    }
}
