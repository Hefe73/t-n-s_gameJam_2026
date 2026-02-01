using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningWound : MonoBehaviour
{
    [Header("Scene refs")]
    public Camera cam;
    public Collider targetCollider;     
    public Transform cottonVisual; 

    [Header("Gameplay")]
    [Range(1, 30)] public int stainCount = 8;
    [Range(0.01f, 0.25f)] public float stainRadiusUV = 0.06f;
    public int passesRequired = 3;

    [Header("Cotton")]
    [Range(0.01f, 0.25f)] public float cottonRadiusUV = 0.06f; 
    public float cottonHoverOffset = 0.01f; 

    [Header("Visual placeholders")]
    public GameObject stainQuadPrefab; 

    [System.Serializable]
    public class Stain
    {
        public Vector2 centerUV;
        public float radiusUV;
        public int passes;
        public bool wasInside;
        public GameObject visual;

        public bool IsClean(int req) => passes >= req;
    }

    private readonly List<Stain> stains = new();

    void Start()
    {
        if (!cam) cam = Camera.main;
        GenerateStains();
        SpawnStainVisuals();
    }

    void GenerateStains()
    {
        stains.Clear();

        int safety = 0;
        while (stains.Count < stainCount && safety++ < 5000)
        {
            Vector2 uv = new Vector2(Random.value, Random.value);

            bool ok = true;
            foreach (var s in stains)
            {
                float minDist = (s.radiusUV + stainRadiusUV) * 0.9f;
                if (Vector2.Distance(uv, s.centerUV) < minDist)
                {
                    ok = false;
                    break;
                }
            }

            if (!ok) continue;

            stains.Add(new Stain
            {
                centerUV = uv,
                radiusUV = stainRadiusUV,
                passes = 0,
                wasInside = false
            });
        }
    }

    void Update()
    {
        if (!cam || !targetCollider) return;
        OnEnable();
        Vector2 screenPos = GetPointerScreenPosition();
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (!targetCollider.Raycast(ray, out RaycastHit hit, 999f))
            return;

        Vector2 uv = GetUV(hit);

        if (cottonVisual)
        {
            cottonVisual.position = hit.point + hit.normal * cottonHoverOffset;
        }

        UpdateCleaning(uv);

        if (AllClean())
        {
            Debug.Log("Minijuego completado: todas las manchas limpias.");
            enabled = false;
        }
    }

    Vector2 GetUV(RaycastHit hit)
    {
        if (hit.collider is MeshCollider)
            return hit.textureCoord;

        Transform t = hit.collider.transform;
        Vector3 local = t.InverseTransformPoint(hit.point);

        float u = local.x + 0.5f;
        float v = local.y + 0.5f;

        return new Vector2(u, v);
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

                // ✅ Si ya quedó limpia, ocultamos/eliminamos el quad visual
                if (s.IsClean(passesRequired) && s.visual != null)
                {
                    // Opción A: ocultar (más barato y reversible)
                    s.visual.SetActive(false);

                    // Opción B: destruir (definitivo)
                    // Destroy(s.visual);
                    // s.visual = null;
                }
            }

            s.wasInside = inside;
        }
    }


    bool AllClean()
    {
        foreach (var s in stains)
            if (!s.IsClean(passesRequired)) return false;

        return true;
    }


    void OnDrawGizmosSelected()
    {
        if (targetCollider == null) return;

        foreach (var s in stains)
        {
            if (s == null) continue;

            Gizmos.color = (s.passes >= passesRequired) ? Color.green : Color.red;

            // Centro en mundo (asumiendo Quad simple)
            Transform t = targetCollider.transform;
            Vector3 localCenter = new Vector3(s.centerUV.x - 0.5f, s.centerUV.y - 0.5f, 0f);
            Vector3 worldCenter = t.TransformPoint(localCenter);

            // Radio aprox en mundo (Quad 1x1)
            float worldRadius = s.radiusUV * t.lossyScale.x;

            DrawGizmoCircle(worldCenter, t.forward, worldRadius, 32);
        }
    }

    static void DrawGizmoCircle(Vector3 center, Vector3 normal, float radius, int segments)
    {
        // Armamos dos ejes perpendiculares
        Vector3 axisA = Vector3.Cross(normal, Vector3.up);
        if (axisA.sqrMagnitude < 0.0001f)
            axisA = Vector3.Cross(normal, Vector3.right);
        axisA.Normalize();
        Vector3 axisB = Vector3.Cross(normal, axisA).normalized;

        Vector3 prev = center + axisA * radius;
        float step = 360f / segments;

        for (int i = 1; i <= segments; i++)
        {
            float ang = step * i * Mathf.Deg2Rad;
            Vector3 next = center + (axisA * Mathf.Cos(ang) + axisB * Mathf.Sin(ang)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    void SpawnStainVisuals()
    {
        if (!stainQuadPrefab || targetCollider == null) return;

        Transform parent = targetCollider.transform;

        foreach (var s in stains)
        {
            Vector3 localPos = new Vector3(
                s.centerUV.x - 0.5f,
                s.centerUV.y - 0.5f,
                -0.5f
            );

            GameObject quad = Instantiate(stainQuadPrefab, parent);
            quad.transform.localPosition = localPos + Vector3.forward * 0.001f;
            quad.transform.localRotation = Quaternion.identity;

            float size = s.radiusUV * 2f;
            quad.transform.localScale = new Vector3(size, size, 1f);

            s.visual = quad;
        }
    }

     private Vector2 GetPointerScreenPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void OnEnable()
    {
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }
}
