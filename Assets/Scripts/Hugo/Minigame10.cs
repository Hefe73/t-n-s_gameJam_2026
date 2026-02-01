using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame10 : MonoBehaviour
{
    [SerializeField] private GameObject boneBreak_pb;
    [SerializeField] private Transform[] spawnPoints = new Transform[8];

    [Header("Drag settings")]
    [SerializeField] private Transform safeZoneCenter; // centro de la zona segura
    [SerializeField] private float safeZoneRadius = 1.0f; // radio de zona segura
    [SerializeField] private LayerMask draggableLayer = ~0; // por defecto todo; ajusta en inspector
    [SerializeField] private float returnDuration = 0.25f; // tiempo para volver a la pos original

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // estado del arrastre
    private GameObject grabbedObject = null;
    private Vector3 grabbedOriginalPos;
    private Vector3 grabOffset; // offset entre punto de hit y el centro del objeto

    void Start()
    {
        int numR = Random.Range(4, 8); // 4..7
        for (int i = 0; i < numR; i++)
        {
            GameObject go = Instantiate(boneBreak_pb, spawnPoints[i].position, Quaternion.Euler(-0, 0, 0));
            originalPositions[go] = go.transform.position;
        }
    }

    void Update()
    {
        // Mouse down: intentar coger objeto bajo el cursor
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabObject();
        }

        // Mantener: mover objeto si hay uno agarrado
        if (Input.GetMouseButton(0) && grabbedObject != null)
        {
            DragGrabbedObject();
        }

        // Soltar: comprobar si está en zona segura o volver
        if (Input.GetMouseButtonUp(0) && grabbedObject != null)
        {
            ReleaseGrabbedObject();
        }
    }

    private void TryGrabObject()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            grabbedObject = hit.collider.gameObject;
            // guardar pos original
            if (!originalPositions.ContainsKey(grabbedObject))
                originalPositions[grabbedObject] = grabbedObject.transform.position;
            grabbedOriginalPos = originalPositions[grabbedObject];

            // offset para que no salte el centro del objeto al punto de hit
            grabOffset = grabbedObject.transform.position - hit.point;

            // si tiene Rigidbody, desactivar física temporalmente
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    private void DragGrabbedObject()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Movemos el objeto sobre un plano horizontal a la altura Y original del objeto
        Plane plane = new Plane(Vector3.up, new Vector3(0, grabbedOriginalPos.y, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            grabbedObject.transform.position = worldPoint + grabOffset;
        }
    }

    private void ReleaseGrabbedObject()
    {
        // comprobar si dentro de zona segura
        bool insideSafe = false;
        if (safeZoneCenter != null)
        {
            float dist = Vector3.Distance(grabbedObject.transform.position, safeZoneCenter.position);
            if (dist <= safeZoneRadius) insideSafe = true;
        }

        // restaurar física si procede
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (!insideSafe)
        {
            // volver a la pos original suavemente
            Vector3 target = grabbedOriginalPos;
            StartCoroutine(ReturnToOriginal(grabbedObject, target, returnDuration));
        }
        // si está dentro de la zona segura, lo dejamos donde está (puedes opcionalmente 'snapear' a safeZoneCenter)

        grabbedObject = null;
    }

    private IEnumerator ReturnToOriginal(GameObject obj, Vector3 targetPos, float duration)
    {
        float t = 0f;
        Vector3 start = obj.transform.position;
        if (duration <= 0f)
        {
            obj.transform.position = targetPos;
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / duration);
            obj.transform.position = Vector3.Lerp(start, targetPos, frac);
            yield return null;
        }
        obj.transform.position = targetPos;
    }

    // Visualización en editor de la zona segura
    private void OnDrawGizmosSelected()
    {
        if (safeZoneCenter != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(safeZoneCenter.position, safeZoneRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(safeZoneCenter.position, safeZoneRadius);
        }
    }
}
