using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraEffects : MonoBehaviour
{
    public PostProcessVolume ppVol;
    public GameObject plane;
    public Material mat_plane;

    [Header("Fog")]
    public Color fogColor = new Color(0.6f, 0.6f, 0.7f);
    public float fogStart = 10f;
    public float fogEnd = 50f;

    [Header("Height Fog")]
    public float fogHeight = 0f;
    public float heightDensity = 0.2f;

    [Header("Color Quantization")]
    public bool enableQuantization = false;
    [Range(2, 64)] public int colorSteps = 16;

    private Material fogMat;
    
    public bool blinking = true;
    public bool doubleBlink = true;
    public float baseVignetteIntensity = 0.2f;
    public float blinkTime = 1.0f;
    public float blinkFrequency = 5.0f;
    public float randomBlinkOffset = 1.0f;

    private float timeForNextBlink = 0.0f;
    private Renderer renderer;
    private UnityEngine.Rendering.PostProcessing.Vignette vignette;
    private PostProcessProfile ppProfile;

    void SetTimeNextBlink()
    {
        // Base frequency + random offset (both directions)
        timeForNextBlink = blinkFrequency + Random.Range(-randomBlinkOffset, randomBlinkOffset);
    }
    
    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        if (!fogMat)
            fogMat = new Material(Shader.Find("Unlit/FogShader"));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
{
    if (!fogMat)
    {
        Graphics.Blit(src, dest);
        return;
    }

    Camera cam = GetComponent<Camera>();

    Matrix4x4 view = cam.worldToCameraMatrix;
    Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
    Matrix4x4 invVP = (proj * view).inverse;

    fogMat.SetMatrix("_InverseViewProjection", invVP);

    fogMat.SetColor("_FogColor", fogColor);
    fogMat.SetFloat("_FogStart", fogStart);
    fogMat.SetFloat("_FogEnd", fogEnd);

    fogMat.SetFloat("_FogHeight", fogHeight);
    fogMat.SetFloat("_FogHeightDensity", heightDensity);

    fogMat.SetFloat("_EnableQuantization", enableQuantization ? 1f : 0f);
    fogMat.SetFloat("_ColorSteps", colorSteps);

    Graphics.Blit(src, dest, fogMat);
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
                yield return new WaitForSeconds(0.08f); // tweak if needed
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
        if (blinking && timeForNextBlink <= 0.0f)
        {
            Blink();
            SetTimeNextBlink();
        }
    }
}