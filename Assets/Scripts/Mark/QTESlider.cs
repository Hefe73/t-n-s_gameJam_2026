using UnityEngine;

public class QTESlider : MonoBehaviour
{
    [Header("References")] public SpriteRenderer barRenderer;
    public SpriteRenderer winZoneRenderer;

    [Header("Movement")] public float speed = 2f;

    private float direction = 1f;

    // WORLD SPACE half-widths
    private float barHalfWidth;
    private float pointerHalfWidth;
    private float winHalfWidth;

    private float minX;
    private float maxX;

    public AudioSource syringeSfx;
    public PlayUISound uiSoundPlayer;


    void Start()
    {
        barHalfWidth = barRenderer.sprite.bounds.extents.x * barRenderer.transform.lossyScale.x;
        pointerHalfWidth = GetComponent<SpriteRenderer>().sprite.bounds.extents.x * transform.lossyScale.x;
        winHalfWidth = winZoneRenderer.sprite.bounds.extents.x * winZoneRenderer.transform.lossyScale.x;

        minX = barRenderer.transform.position.x - barHalfWidth + pointerHalfWidth;
        maxX = barRenderer.transform.position.x + barHalfWidth - pointerHalfWidth;

        if (minX >= maxX)
        {
            Debug.LogError("POINTER IS WIDER THAN BAR — FIX YOUR SCALES");
            enabled = false;
        }
    }

    void Update()
    {
        MovePointer();
        CheckClick();
    }

    void MovePointer()
    {
        Vector3 pos = transform.position;
        pos.x += direction * speed * Time.deltaTime;

        if (pos.x <= minX)
        {
            pos.x = minX;
            direction = 1f;
        }
        else if (pos.x >= maxX)
        {
            pos.x = maxX;
            direction = -1f;
        }

        transform.position = pos;
    }

    void CheckClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        float pointerX = transform.position.x;
        float winX = winZoneRenderer.transform.position.x;

        float winMin = winX - winHalfWidth;
        float winMax = winX + winHalfWidth;

        if (pointerX >= winMin && pointerX <= winMax)
        {
            Debug.Log("SUCCESS");
            syringeSfx.PlayOneShot(syringeSfx.clip);
            //uiSoundPlayer.PlaySoundWin();
        }
        else
        {
            Debug.Log("FAIL");
            uiSoundPlayer.PlaySoundLoose();
        }
    }
}