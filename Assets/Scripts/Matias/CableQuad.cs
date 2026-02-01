using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CableQuad : MonoBehaviour
{
    public float width = 0.08f;

    Mesh _mesh;
    Vector3[] _verts;
    Vector2[] _uvs;
    int[] _tris;

    Vector3 _start;
    Vector3 _end;
    Vector3 _normal; 
    
    void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "CableQuadMesh";
        GetComponent<MeshFilter>().mesh = _mesh;

        _verts = new Vector3[4];
        _uvs = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1),
        };
        _tris = new int[6] { 0, 1, 2, 2, 1, 3 };

        _mesh.vertices = _verts;
        _mesh.uv = _uvs;
        _mesh.triangles = _tris;
    }

    public void SetPlaneNormal(Vector3 normal) => _normal = normal.normalized;

    public void SetEndpoints(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
        Rebuild();
    }

    void Rebuild()
    {
        Vector3 dir = (_end - _start);
        float len = dir.magnitude;
        if (len < 0.0001f) len = 0.0001f;
        dir /= len;

        Vector3 perp = Vector3.Cross(_normal, dir).normalized * (width * 0.5f);

        Vector3 v0 = _start - perp;
        Vector3 v1 = _start + perp;
        Vector3 v2 = _end   - perp;
        Vector3 v3 = _end   + perp;

        _verts[0] = transform.InverseTransformPoint(v0);
        _verts[1] = transform.InverseTransformPoint(v1);
        _verts[2] = transform.InverseTransformPoint(v2);
        _verts[3] = transform.InverseTransformPoint(v3);

        _mesh.vertices = _verts;
        _mesh.RecalculateBounds();
    }
}
