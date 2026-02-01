using UnityEngine;

public class NpcCameraEffect : MonoBehaviour
{
    [Header("Fog")] public Color fogColor = new Color(0.6f, 0.6f, 0.7f);
    public float fogStart = 10f;
    public float fogEnd = 50f;

    [Header("Height Fog")] public float fogHeight = 0f;
    public float heightDensity = 0.2f;

    [Header("Color Quantization")] public bool enableQuantization = false;
    [Range(2, 64)] public int colorSteps = 16;

    [Header("Saturation")] [Range(0.0f, 1.0f)]
    public float imageSaturation = 1.0f;
    private Material fogMat;
    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        if (!fogMat)
            fogMat = new Material(Shader.Find("Unlit/FogShaderNPCs"));
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
