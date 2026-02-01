using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningWound : MonoBehaviour
{
    [Header("Scene refs")]
    public Camera cam;
    public Collider targetCollider;
    public Transform cottonVisual;

    public AudioSource sfx;
    public PlayUISound uiSoundplayer;

    [Header("Gameplay")]
    [Range(1, 30)] public int stainCount = 8;
    [Range(0.01f, 0.25f)] public float stainRadiusUV = 0.06f; 
    public int passesRequired = 3;

    [Header("Cotton")]
    [Range(0.01f, 0.25f)] public float cottonRadiusUV = 0.06f;
    public float cottonHoverOffset = 0.01f;

    [Header("Visual placeholders")]
    public GameObject stainQuadPrefab;

    [Header("World placement")]
    public float stainWorldScale = 0.15f;   
    public float surfaceOffset = 0.002f;   

    [Header("Spawn zone")]
    [Range(0f, 1f)] public float topStartPercent = 0.8f; 

    [Header("Torso zone")]
    public Collider torsoCollider;

    [System.Serializable]
    public class Stain
    {
        public Vector2 centerUV; 
        public float radiusUV;
        public int passes;
        public bool wasInside;
        public GameObject visual;

        public Vector3 worldPos;
        public Vector3 worldNormal;

        public bool IsClean(int req) => passes >= req;
    }

    private readonly List<Stain> stains = new();

    void OnEnable() => Cursor.visible = false;
    void OnDisable() => Cursor.visible = true;

    void Start()
    {
        if (!cam) cam = Camera.main;

        GenerateStainsOnSurface();
        SpawnStainVisualsOnSurface();

        if (stains.Count == 0)
        {
            Debug.LogWarning("No se generaron stains. Revisá TorsoCollider / Raycasts.");
            enabled = false; // o reintentar
        }
    }

    void Update()
    {
        if (!cam || !targetCollider) return;

        Vector2 screenPos = GetPointerScreenPosition();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!targetCollider.Raycast(ray, out RaycastHit hit, 999f))
            return;

        Vector2 uv = GetUV(hit);

        if (cottonVisual)
            cottonVisual.position = hit.point + hit.normal * cottonHoverOffset;

        UpdateCleaning(uv);

        if (AllClean())
        {
            Debug.Log("Minijuego completado: todas las manchas limpias.");

            if (uiSoundplayer != null)
                uiSoundplayer.PlaySoundWin();

            if (MinigameManagerChoni.Instance != null)
                MinigameManagerChoni.Instance.MinigameFinished(1.5f);
            else
                Debug.LogWarning("MinigameManagerChoni.Instance es NULL");

            enabled = false;
        }
    }

    void GenerateStainsOnSurface()
    {
        stains.Clear();
        if (!targetCollider || !torsoCollider) return;

        Bounds torsoB = torsoCollider.bounds;

        int safety = 0;
        while (stains.Count < stainCount && safety++ < 20000)
        {
            // Punto random en XZ del torso, pero arrancamos desde ARRIBA
            Vector3 origin = new Vector3(
                Random.Range(torsoB.min.x, torsoB.max.x),
                torsoB.max.y + 0.5f,
                Random.Range(torsoB.min.z, torsoB.max.z)
            );

            Vector3 dir = Vector3.down;

            // 1) tiene que pegar en el torso zone (garantiza "zona torso")
            if (!torsoCollider.Raycast(new Ray(origin, dir), out RaycastHit torsoHit, 5f))
                continue;

            // 2) y además en la piel (targetCollider)
            if (!targetCollider.Raycast(new Ray(origin, dir), out RaycastHit hit, 5f))
                continue;

            // que no haya quedado muy lejos del torso hit
            if (Vector3.Distance(hit.point, torsoHit.point) > 0.30f)
                continue;

            // separar manchas entre sí
            bool ok = true;
            foreach (var s in stains)
            {
                if (Vector3.Distance(hit.point, s.worldPos) < stainWorldScale * 0.9f)
                {
                    ok = false;
                    break;
                }
            }
            if (!ok) continue;

            stains.Add(new Stain
            {
                centerUV = hit.textureCoord,
                radiusUV = stainRadiusUV,
                passes = 0,
                wasInside = false,
                worldPos = hit.point,
                worldNormal = hit.normal
            });
        }

        Debug.Log($"Stains generadas: {stains.Count}/{stainCount}");
    }



    void SpawnStainVisualsOnSurface()
    {
        if (!stainQuadPrefab || targetCollider == null) return;

        foreach (var s in stains)
        {
            GameObject quad = Instantiate(stainQuadPrefab);
            quad.name = "Stain";

            quad.transform.SetParent(targetCollider.transform, true); 

            quad.transform.position = s.worldPos + s.worldNormal * surfaceOffset;
            quad.transform.rotation = Quaternion.LookRotation(s.worldNormal);

            quad.transform.localScale = new Vector3(stainWorldScale, stainWorldScale, 1f);

            s.visual = quad;
        }
    }

    Vector2 GetUV(RaycastHit hit)
    {
        return hit.textureCoord;
    }

    void UpdateCleaning(Vector2 cottonUV)
    {
        float effectiveRadius = cottonRadiusUV;

        foreach (var s in stains)
        {
            if (s.IsClean(passesRequired))
                continue;

            float dist = Vector2.Distance(cottonUV, s.centerUV);
            bool inside = dist <= (s.radiusUV + effectiveRadius);

            if (inside && !s.wasInside)
            {
                s.passes++;
                if (s.passes > passesRequired) s.passes = passesRequired;

                if (s.IsClean(passesRequired) && s.visual != null)
                {
                    s.visual.SetActive(false);
                    if (sfx && sfx.clip) sfx.PlayOneShot(sfx.clip);
                }
            }

            s.wasInside = inside;
        }
    }

    bool AllClean()
    {
        if (stains.Count == 0) return false; // <- clave
        foreach (var s in stains)
            if (!s.IsClean(passesRequired)) return false;
        return true;
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}
