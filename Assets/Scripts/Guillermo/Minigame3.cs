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

    private bool canBeClickError = true;

    public AudioSource cut_sound;
    public PlayUISound uiSoundplayer;

    public int soundCutIntervals = 4;
    private int lastcutInterval = 0;

    [Header("Debug Visualization")] public bool showDebugInGame = true;
    public Color debugPathColor = Color.cyan;
    public Color debugCurrentColor = Color.green;
    public Color debugMouseColor = Color.red;
    public float debugSphereSize = 0.06f;

    private List<GameObject> debugSpheres = new List<GameObject>();
    private GameObject debugMouseSphere;

    InstrumentCursor instrumentCursor;
    bool hasInstrumentCursor = false;


    void Start()
    {
        BuildPath();

        instrumentCursor = FindFirstObjectByType<InstrumentCursor>();
        hasInstrumentCursor = instrumentCursor != null;

        if (hasInstrumentCursor)
            Debug.Log("InstrumentCursor detected in scene.");
        else
            Debug.Log("No InstrumentCursor detected, ignoring instrument requirement.");
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

        CreateDebugSpheres();
    }

    void ClearDebugSpheres()
    {
        foreach (var go in debugSpheres)
            if (go)
                Destroy(go);

        debugSpheres.Clear();
    }

    void CreateDebugSpheres()
    {
        if (!showDebugInGame) return;

        ClearDebugSpheres();

        foreach (var p in pathPoints)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.position = p;
            s.transform.localScale = Vector3.one * debugSphereSize;

            var r = s.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Unlit/Color"));
            r.material.color = debugPathColor;

            Destroy(s.GetComponent<Collider>()); // debug only
            debugSpheres.Add(s);
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

        //ClearDebugSpheres();
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

        MinigameManagerXoxo.Instance.MinigameFinished(2.0f);
        ClearDebugSpheres();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BuildPath();
            started = true;
            progress = 0;
            hasLastMousePos = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!started) return;

            if (progress >= pathPoints.Count)
                Win();
            else
            {
                if (!canBeClickError)
                {
                    Lose();
                }
            }
        }

        if (progress - lastcutInterval > soundCutIntervals)
        {
            lastcutInterval = progress;
            cut_sound.volume = Random.Range(0.55f, 0.8f);
            cut_sound.pitch = Random.Range(1f - 0.2f, 1f + 0.2f);
            cut_sound.PlayOneShot(cut_sound.clip);
        }

        if (!started || !Input.GetMouseButton(0))
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
        if (showDebugInGame && Application.isPlaying)
        {
            if (MouseWorldPosition(out Vector3 mouse))
            {
                if (debugMouseSphere == null)
                {
                    debugMouseSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugMouseSphere.transform.localScale = Vector3.one * debugSphereSize;

                    var r = debugMouseSphere.GetComponent<Renderer>();
                    r.material = new Material(Shader.Find("Unlit/Color"));
                    r.material.color = debugMouseColor;

                    Destroy(debugMouseSphere.GetComponent<Collider>());
                }

                debugMouseSphere.transform.position = mouse;
            }
        }

        if (!valid)
        {
            if (canBeClickError && hasInstrumentCursor)
            {
                canBeClickError = false;
            }
            else
            {
                Lose();
            }

            return;
        }


        if (progress < pathPoints.Count &&
            Vector3.Distance(mouseWorld, pathPoints[progress]) <= errorFreedom)
        {
            // If InstrumentCursor exists, require an instrument
            if (hasInstrumentCursor)
            {
                if (instrumentCursor.currentInstrument != null)
                {
                    progress++;
                }
                // else: instrument missing → do NOT advance
            }
            else
            {
                // No InstrumentCursor in scene → always allow
                progress++;
            }
        }


        if (showDebugInGame && progress < debugSpheres.Count)
        {
            for (int i = 0; i < debugSpheres.Count; i++)
            {
                var r = debugSpheres[i].GetComponent<Renderer>();
                r.material.color = (i == progress) ? debugCurrentColor : debugPathColor;
            }
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
            Gizmos.DrawSphere(pathPoints[progress], 0.02f);
        }

        if (Application.isPlaying && MouseWorldPosition(out Vector3 mouse))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(mouse, 0.06f);
        }
    }
}