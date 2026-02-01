using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningWound : MonoBehaviour
{
    [Header("Scene refs")]
    public Camera cam;
    public Collider targetCollider;
    public Transform cottonVisual;

    [Header("Cleaning")]
    public float cottonHoverOffset = 0.01f;
    public float cottonRadiusWorld = 0.12f; 

    [Header("Stains (manual)")]
    public List<StainTarget> stains = new();

    [Header("Audio/UI")]
    public AudioSource sfx;
    public PlayUISound uiSoundplayer;

    readonly HashSet<StainTarget> _inside = new();

    void OnEnable() => Cursor.visible = false;
    void OnDisable() => Cursor.visible = true;

    void Start()
    {
        if (!cam) cam = Camera.main;

        if (stains == null || stains.Count == 0)
        {
            Debug.LogWarning("No hay manchas asignadas en la lista 'stains'.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!cam || !targetCollider) return;

        Vector2 screenPos = GetPointerScreenPosition();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!targetCollider.Raycast(ray, out RaycastHit hit, 999f))
            return;

        if (cottonVisual)
            cottonVisual.position = hit.point + hit.normal * cottonHoverOffset;

        UpdateCleaningWorld(hit.point);

        if (AllClean())
        {
            Debug.Log("Minijuego completado: todas las manchas limpias.");

            if (uiSoundplayer) uiSoundplayer.PlaySoundWin();

            if (MinigameManagerChoni.Instance != null)
                MinigameManagerChoni.Instance.MinigameFinished(1.5f);
            else
                Debug.LogWarning("MinigameManagerChoni.Instance es NULL");

            enabled = false;
        }
    }

    void UpdateCleaningWorld(Vector3 cottonWorldPos)
    {
        for (int i = 0; i < stains.Count; i++)
        {
            var st = stains[i];
            if (st == null || st.IsClean) continue;

            bool inside = st.ContainsPoint(cottonWorldPos, cottonRadiusWorld);

            if (inside && !_inside.Contains(st))
            {
                st.AddPass();
                _inside.Add(st);

                Debug.Log($"Pass +1 en {st.name}: {st.passes}/{st.passesRequired}");

                if (sfx && sfx.clip) sfx.PlayOneShot(sfx.clip);
            }
            else if (!inside && _inside.Contains(st))
            {
                _inside.Remove(st);
            }
        }
    }
    bool AllClean()
    {
        for (int i = 0; i < stains.Count; i++)
        {
            var st = stains[i];
            if (st != null && !st.IsClean) return false;
        }
        return true;
    }

    Vector2 GetPointerScreenPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}
