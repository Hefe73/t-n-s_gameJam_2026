using UnityEngine;

public class StainTarget : MonoBehaviour
{
    public int passesRequired = 3;
    public int passes = 0;
    public GameObject visual;

    Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (visual == null) visual = gameObject;
    }

    public bool IsClean => passes >= passesRequired;

    public bool ContainsPoint(Vector3 worldPoint, float extraRadius)
    {
        if (_col == null) return false;

        // punto más cercano del collider al algodón
        Vector3 closest = _col.ClosestPoint(worldPoint);
        return (closest - worldPoint).sqrMagnitude <= extraRadius * extraRadius;
    }

    public void AddPass()
    {
        if (IsClean) return;

        passes++;
        if (passes >= passesRequired)
        {
            if (visual != null) visual.SetActive(false);
        }
    }
}
