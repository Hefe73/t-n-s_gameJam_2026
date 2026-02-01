using UnityEngine;

public class InstrumentPanel : MonoBehaviour
{
    public LevelData currentLevel;
    public InstrumentButton[] allInstrumentButtons;

    void Start()
    {
        foreach (var btn in allInstrumentButtons)
        {
            bool enabled = false;

            foreach (var inst in currentLevel.availableInstruments)
            {
                if (btn.iconImage.sprite == inst.icon)
                {
                    enabled = true;
                    break;
                }
            }

            btn.gameObject.SetActive(enabled);
        }
    }
}
