using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraBlink : MonoBehaviour
{
    [Header("Postprocess")] public PostProcessVolume ppVol;
    public GameObject plane;
    public Material mat_plane;
    
    [Header("Blink Effect")] public bool blinking = true;
    public bool doubleBlink = true;
    public float baseVignetteIntensity = 0.2f;
    public float blinkTime = 1.0f;
    public float blinkFrequency = 5.0f;
    public float randomBlinkOffset = 1.0f;
    
    private float timeForNextBlink = 0.0f;
    private Renderer renderer;
    private UnityEngine.Rendering.PostProcessing.Vignette vignette;
    private PostProcessProfile ppProfile;

    private float startColorStepsForDoorAnim;

    void SetTimeNextBlink()
    {
        // Base frequency + random offset (both directions)
        timeForNextBlink = blinkFrequency + Random.Range(-randomBlinkOffset, randomBlinkOffset);
    }

    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ppVol)
        {
            ppProfile = ppVol.profile;
            vignette = ppProfile.GetSetting<Vignette>();
        }

        renderer = plane.GetComponent<Renderer>();
        mat_plane = renderer.material;

        // Set alpha of mat_plane to 0
        Color c = mat_plane.color;
        c.a = 0.0f;
        mat_plane.color = c;

        SetTimeNextBlink();
        
        // TODO
        // Search in all component children and set the fov of the camera in this component to the child components
        
    }


    void Blink()
    {
        StopAllCoroutines(); // prevent overlapping blinks
        StartCoroutine(BlinkRoutine());
    }


    IEnumerator BlinkRoutine()
    {
        int blinkCount = doubleBlink ? 2 : 1;

        for (int i = 0; i < blinkCount; i++)
        {
            yield return StartCoroutine(SingleBlink());

            // Small pause between blinks in a double blink
            if (doubleBlink && i == 0)
                yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator SingleBlink()
    {
        float halfTime = blinkTime * 0.5f;
        float t = 0f;

        // --- Closing eye ---
        while (t < halfTime)
        {
            float k = t / halfTime;

            if (vignette)
                vignette.intensity.value = Mathf.Lerp(baseVignetteIntensity, 1.0f, k);

            Color c = mat_plane.color;
            c.a = Mathf.Lerp(0f, 1f, k);
            mat_plane.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        // --- Opening eye ---
        t = 0f;
        while (t < halfTime)
        {
            float k = t / halfTime;

            if (vignette)
                vignette.intensity.value = Mathf.Lerp(1.0f, baseVignetteIntensity, k);

            Color c = mat_plane.color;
            c.a = Mathf.Lerp(1f, 0f, k);
            mat_plane.color = c;

            t += Time.deltaTime;
            yield return null;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Countdown to next blink
        timeForNextBlink -= Time.deltaTime;
        /*
        if (timeForNextBlink <= 0.0f)
        {
            //DoDoorAnimation();
        }
        */
        if (blinking && timeForNextBlink <= 0.0f)
        {
            Blink();
            SetTimeNextBlink();
        }
    }
}