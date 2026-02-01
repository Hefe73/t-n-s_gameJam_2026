using System.Collections.Generic;
using UnityEngine;

public class Minigame3 : MonoBehaviour
{
    public Transform[] positions;
    public float errorFreedom = 0.5f;
    public float planeZ = 10f;
    public bool won = false;

    [Header("Speed Requirement")] public bool requireMinimumSpeed = false;
    public float minimumSpeed = 1.0f; // world units per second

    private List<Vector3> pathPoints = new List<Vector3>();

    private bool started = false;
    private int progress = 0;

    private Vector3 lastMouseWorldPos;
    private bool hasLastMousePos = false;

    public AudioSource cut_sound;
    public PlayUISound uiSoundplayer;

    public int soundCutIntervals = 4;
    private int lastcutInterval = 0;

    void Start()
    {
        BuildPath();
    }

    void BuildPath()
    {
        pathPoints.Clear();
        if (positions == null || positions.Length < 2)
            return;

        float maxStep = errorFreedom * 0.5f;

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 a = positions[i].position;
            Vector3 b = positions[i + 1].position;

            if (i == 0)
                pathPoints.Add(a);

            float dist = Vector3.Distance(a, b);
            int steps = Mathf.CeilToInt(dist / maxStep);

            for (int s = 1; s <= steps; s++)
            {
                float t = s / (float)steps;
                pathPoints.Add(Vector3.Lerp(a, b, t));
            }
        }
    }

    bool MouseWorldPosition(out Vector3 pos)
    {
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, planeZ));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            pos = ray.GetPoint(enter);
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    void Lose()
    {
        Debug.Log("LOSE");
        started = false;
        progress = 0;
        hasLastMousePos = false;
        lastcutInterval = 0;
        if (uiSoundplayer)
        {
            uiSoundplayer.PlaySoundLoose();
        }
    }

    void Win()
    {
        Debug.Log("WIN");
        started = false;
        progress = 0;
        hasLastMousePos = false;
        if (uiSoundplayer)
        {
            uiSoundplayer.PlaySoundWin();
        }
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            BuildPath();
            started = true;
            progress = 0;
            hasLastMousePos = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (!started) return;

            if (progress >= pathPoints.Count)
                Win();
            else
                Lose();
        }

        if (progress - lastcutInterval > soundCutIntervals)
        {
            lastcutInterval = progress;
            cut_sound.volume = Random.Range(0.55f, 0.8f);
        cut_sound.pitch = Random.Range(1f - 0.2f, 1f + 0.2f);
        cut_sound.PlayOneShot(cut_sound.clip);
        }

        if (!started || !Input.GetMouseButton(1))
            return;

        if (!MouseWorldPosition(out Vector3 mouseWorld))
            return;

        // ---------- SPEED CHECK ----------
        if (requireMinimumSpeed)
        {
            if (hasLastMousePos)
            {
                float distance = Vector3.Distance(mouseWorld, lastMouseWorldPos);
                float speed = distance / Time.deltaTime;

                if (speed < minimumSpeed)
                {
                    Lose();
                    return;
                }
            }

            lastMouseWorldPos = mouseWorld;
            hasLastMousePos = true;
        }

        // ---------- PATH VALIDATION ----------
        bool valid = false;

        if (progress > 0 &&
            Vector3.Distance(mouseWorld, pathPoints[progress - 1]) <= errorFreedom)
            valid = true;

        if (progress < pathPoints.Count &&
            Vector3.Distance(mouseWorld, pathPoints[progress]) <= errorFreedom)
            valid = true;

        if (!valid)
        {
            Lose();
            return;
        }

        if (progress < pathPoints.Count &&
            Vector3.Distance(mouseWorld, pathPoints[progress]) <= errorFreedom)
        {
            progress++;
        }
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var p in pathPoints)
        {
            Gizmos.DrawSphere(p, 0.05f);
        }

        if (started && progress < pathPoints.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pathPoints[progress], 0.08f);
        }

        if (Application.isPlaying && MouseWorldPosition(out Vector3 mouse))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(mouse, 0.06f);
        }
    }
}