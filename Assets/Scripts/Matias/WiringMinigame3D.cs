using System.Collections.Generic;
using UnityEngine;

public class WiringMinigame3D : MonoBehaviour
{
    public AudioSource sfx;
    public PlayUISound uiSoundplayer;
    
    [Header("Scene refs")]
    public Camera cam;
    public Collider boardCollider;        
    public Transform cablesParent;       
    public GameObject cablePrefab;

    [Header("Rules")]
    public float snapDistance = 0.25f;

    CableSocket _dragStartSocket;
    CableQuad _activeCable;

    CableSocket[] _allSockets;

    readonly List<(CableSocket left, CableSocket right)> _connections = new();

    void Awake()
    {
        if (!cam) cam = Camera.main;
        _allSockets = Object.FindObjectsByType<CableSocket>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();

        if (_activeCable != null)
            DragUpdate();

        if (Input.GetMouseButtonUp(0))
            TryEndDrag();
    }

    void TryBeginDrag()
    {
        if (!RaycastSocket(out var socket)) {Debug.Log("NO SOCKET HIT"); return;}

        if (socket.side != SocketSide.Left) return;
        if (socket.occupied) return;

        Debug.Log("Mouse down");

        Debug.Log("SOCKET HIT: " + socket.name);
        
        sfx.pitch = Random.Range(1f - 0.1f, 1f + 0.1f);
        sfx.PlayOneShot(sfx.clip);

        _dragStartSocket = socket;

        var go = Instantiate(cablePrefab, cablesParent ? cablesParent : transform);
        _activeCable = go.GetComponent<CableQuad>();
        _activeCable.transform.position = Vector3.zero; 
        Vector3 planeNormal = GetBoardNormal();
        _activeCable.SetPlaneNormal(planeNormal);
    }

    void DragUpdate()
    {
        Vector3 n = GetBoardNormal();
        Vector3 start = _dragStartSocket.transform.position + n * 0.01f;
        Vector3 end = GetMousePointOnBoard() + n * 0.01f;
        var p = GetMousePointOnBoard();
        Debug.Log("Board point: " + p);
        _activeCable.SetEndpoints(start, end);
    }

    void TryEndDrag()
    {
        if (_activeCable == null || _dragStartSocket == null) return;

        // Punto donde está el mouse en el tablero (YA proyectado al plano)
        Vector3 mp3 = GetMousePointOnBoard();
        Vector2 mp = new Vector2(mp3.x, mp3.y);

        CableSocket best = null;
        float bestDist = float.MaxValue;

        foreach (var s in _allSockets)
        {
            if (s == null) continue;
            if (s.side != SocketSide.Right) continue;
            if (s.occupied) continue;
            if (s.type != _dragStartSocket.type) continue;

            Vector2 sp = new Vector2(s.transform.position.x, s.transform.position.y);
            float d = Vector2.Distance(mp, sp);

            if (d < bestDist)
            {
                bestDist = d;
                best = s;
            }
        }

        Debug.Log($"TryEndDrag: best={(best ? best.name : "NONE")} dist={bestDist:F3} snap={snapDistance:F3}");

        if (best != null && bestDist <= snapDistance)
        {
            Vector3 n = GetBoardNormal();
            Vector3 start = _dragStartSocket.transform.position + n * 0.01f;
            Vector3 end = best.transform.position + n * 0.01f;

            _activeCable.SetEndpoints(start, end);

            _dragStartSocket.occupied = true;
            best.occupied = true;
            _connections.Add((_dragStartSocket, best));

            _activeCable = null;
            _dragStartSocket = null;

            CheckWin();
        }
        else
        {
            Destroy(_activeCable.gameObject);
            _activeCable = null;
            _dragStartSocket = null;
        }
    }

    bool RaycastSocket(out CableSocket socket)
    {
        socket = null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 500f, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            var s = h.collider.GetComponentInParent<CableSocket>();
            if (s != null)
            {
                socket = s;
                return true;
            }
        }
        return false;
    }

    Vector3 GetMousePointOnBoard()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (boardCollider != null && boardCollider.Raycast(ray, out RaycastHit hit, 500f))
        {
            Vector3 p = hit.point;

            // Fuerza el punto al plano del tablero (misma Z)
            float zPlane = boardCollider.bounds.center.z; 
            p.z = zPlane;

            return p;
        }

        return ray.origin + ray.direction * 5f;
    }

    Vector3 GetBoardNormal()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (boardCollider.Raycast(ray, out RaycastHit hit, 500f))
            return hit.normal;

        return boardCollider.transform.forward;
    }

    void CheckWin()
    {
        foreach (var s in _allSockets)
        {
            if (s.side == SocketSide.Left && !s.occupied)
                return;
        }
        uiSoundplayer.PlaySoundWin();
        Debug.Log("MINIGAME WIN!");
    }
}
