using System;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

public class Rope2DCreator : MonoBehaviour
{
    [SerializeField, Range(2, 50)] int segmentCount = 2;

    public Transform startPoint;
    public Transform endPoint;

    public HingeJoint segmentPrefab;

    [HideInInspector] public Transform[] segments;
    public Suture sutureManager;

    Vector2 GetSegmentPosition(int index)
    {
        Vector2 posA = startPoint.position;
        Vector2 posB = endPoint.position;

        float fraction = 1.0f / segmentCount;
        return Vector2.Lerp(posA, posB, fraction * index);
    }

    [Button]
    void GenerateRope()
    {
        segments = new Transform[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            var currJoint = Instantiate(segmentPrefab, GetSegmentPosition(i), Quaternion.identity, transform);
            segments[i] = currJoint.transform;

            if (i > 0)
            {
                int prevIndex = i - 1;
                currJoint.connectedBody = segments[prevIndex].GetComponent<Rigidbody>();
            }
            else
            {
                currJoint.connectedBody = startPoint.GetComponent<Rigidbody>();
            }

            if (i == segmentCount - 1)
            {
                currJoint.connectedBody = endPoint.GetComponent<Rigidbody>();
            }
        }
    }

    [Button]
    void DeleteSegments()
    {
        if (segments == null)
            return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        segments = null;
    }

    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null)
            Gizmos.color = Color.green;

        for (int i = 0; i <= segmentCount; i++)
        {
            Vector2 posAtIndex = GetSegmentPosition(i);
            Gizmos.DrawSphere(posAtIndex, 0.1f);
        }
    }

    private void Update()
    {
        if (startPoint == null || segments == null || segments.Length == 0) return;

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(cam.transform.position.z - startPoint.position.z);
        Vector3 worldMousePos = cam.ScreenToWorldPoint(screenPos);

        var firstSegment = segments[0];
        firstSegment.position = worldMousePos;
        
        if (sutureManager != null)
        {
            //sutureManager.CheckSutureStatus(segments);
        }
    }
}