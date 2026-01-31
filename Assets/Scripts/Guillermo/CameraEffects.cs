using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraEffects : MonoBehaviour
{
    [Header("Postprocess")] public PostProcessVolume ppVol;
    public GameObject plane;
    public Material mat_plane;

    [Header("Fog")] public Color fogColor = new Color(0.6f, 0.6f, 0.7f);
    public float fogStart = 10f;
    public float fogEnd = 50f;

    [Header("Height Fog")] public float fogHeight = 0f;
    public float heightDensity = 0.2f;

    [Header("Color Quantization")] public bool enableQuantization = false;
    [Range(2, 64)] public int colorSteps = 16;

    [Header("Saturation")] [Range(0.0f, 1.0f)]
    public float imageSaturation = 1.0f;

    [Header("Blink Effect")] public bool blinking = true;
    public bool doubleBlink = true;
    public float baseVignetteIntensity = 0.2f;
    public float blinkTime = 1.0f;
    public float blinkFrequency = 5.0f;
    public float randomBlinkOffset = 1.0f;

    [Header("Door Color Change")] public bool isAnimating = false;
    public float colorChangeDuration = 3.0f;
    public GameObject door_Camera;
    public bool doorColorFadeAnimating = false;

    [Header("Door Transition")] public Transform door_tr;
    public bool doorAnimationFinished = false;
    public Transform door_hinge;
    public bool door_to_open = false;
    public float door_opening_duration = 1.0f;
    public float door_length_offset = 1.0f;
    public float door_length_offset_curve_multiplier = 1.0f;
    public float transition_to_offset_duration = 3.0f;
    public float transition_wait_duration = 1.0f;
    public float transition_to_door_duration = 1.5f;
    public AnimationCurve curve_to_offset;
    public AnimationCurve curve_to_door;
    public AnimationCurve curve_open_door;
    private Material fogMat;
    private float timeForNextBlink = 0.0f;
    private Renderer renderer;
    private UnityEngine.Rendering.PostProcessing.Vignette vignette;
    private PostProcessProfile ppProfile;
    private CameraManager _cameraManager;

    private float startColorStepsForDoorAnim;

    public void DoDoorAnimation()
    {
        if (doorColorFadeAnimating) return;
        doorColorFadeAnimating = true;
        door_Camera.SetActive(true);
        startColorStepsForDoorAnim = colorSteps;
        StopAllCoroutines();
        StartCoroutine(doorCameraTransition());
        StartCoroutine(doorColorStepsAnim());
    }

    IEnumerator openDoor()
    {
        float t = 0f;

        Vector3 startEuler = door_hinge.eulerAngles;
        Vector3 endEuler = startEuler + new Vector3(0f, 110f, 0f);
        if (_cameraManager)
        {
            _cameraManager.isFollowingPlayer = false;
        }

        while (t < door_opening_duration)
        {
            float k = t / door_opening_duration;
            float kCurve = curve_open_door.Evaluate(k);

            Vector3 euler = Vector3.Lerp(startEuler, endEuler, kCurve);
            door_hinge.eulerAngles = euler;

            t += Time.deltaTime;
            yield return null;
        }

        door_hinge.eulerAngles = endEuler;
    }


    IEnumerator doorColorStepsAnim()
    {
        float t = 0f;
        while (t < colorChangeDuration)
        {
            float k = t / colorChangeDuration;

            // Lerp as float
            float lerped = Mathf.Lerp(startColorStepsForDoorAnim, 2f, k);
            float lerped_sat = Mathf.Lerp(1.0f, 0.0f, k);
            // Round to nearest int for smoother stepping
            colorSteps = Mathf.RoundToInt(lerped);
            imageSaturation = lerped_sat;

            t += Time.deltaTime;
            yield return null;
        }

        // Make sure we hit the final value
        colorSteps = 1;
    }

    IEnumerator doorCameraTransition()
    {
        float t = 0f;

        Vector3 init = transform.position;
        Vector3 end = door_tr.position + (-door_tr.right * (door_length_offset));

        // Control point for the spline
        Vector3 mid = ((init + end) * 0.5f) + (-door_tr.right *
                                               (door_length_offset * door_length_offset_curve_multiplier));

        Vector3 lookPos;

        while (t < transition_to_offset_duration)
        {
            float k = t / transition_to_offset_duration;

            // Apply animation curve (ease / stylization)
            float kCurve = curve_to_offset.Evaluate(k);

            // Quadratic Bézier spline
            Vector3 pos =
                Mathf.Pow(1f - kCurve, 2f) * init +
                2f * (1f - kCurve) * kCurve * mid +
                Mathf.Pow(kCurve, 2f) * end;

            transform.position = pos;

            lookPos.x = Mathf.Lerp(end.x, door_tr.position.x, k);
            lookPos.y = Mathf.Lerp(end.y, door_tr.position.y, k);
            lookPos.z = Mathf.Lerp(end.z, door_tr.position.z, k);

            transform.LookAt(lookPos);


            t += Time.deltaTime;
            yield return null;
        }

        // Snap exactly to end to avoid floating-point drift
        transform.position = end;
        transform.LookAt(door_tr);
        door_to_open = true;
        StartCoroutine(openDoor());
        t = 0.0f;

        while (t < transition_wait_duration)
        {
            transform.LookAt(door_tr);
            float k = t / transition_to_offset_duration;
            t += Time.deltaTime;
            yield return null;
        }

        transform.LookAt(door_tr);
        Quaternion q = transform.rotation;
        t = 0.0f;

        while (t < transition_to_door_duration)
        {
            float k = t / transition_to_door_duration;

            // Apply animation curve
            float kCurve = curve_to_door.Evaluate(k);

            // Lerp position
            Vector3 pos_to_door = Vector3.Lerp(end, door_tr.position + (door_tr.right * 2.0f), kCurve);
            transform.position = pos_to_door;

            // Lock rotation
            transform.rotation = q;

            t += Time.deltaTime;
            yield return null;
        }

// Ensure final state is clean
        transform.position = door_tr.position;
        transform.rotation = q;
        doorAnimationFinished = true;
    }


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

        fogMat.SetFloat("_Saturation", imageSaturation);


        Graphics.Blit(src, dest, fogMat);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        door_Camera.SetActive(false);

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

        _cameraManager = CameraManager.Instance;

        if (!_cameraManager)
        {
            Debug.Log("Camera Manager wasnt found!");
        }

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
        if (blinking && timeForNextBlink <= 0.0f && !doorColorFadeAnimating)
        {
            Blink();
            SetTimeNextBlink();
        }
    }
}