using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame10 : MonoBehaviour
{
    bool gameActive = false;

    
    [SerializeField] private GameObject boneBreak_pb;
    [SerializeField] private Transform[] spawnPoints = new Transform[8];

    [Header("Drag settings")]
    [SerializeField] private Transform safeZoneCenter; // centro de la zona segura
    [SerializeField] private float safeZoneRadius = 1.0f; // radio de zona segura
    [SerializeField] private LayerMask draggableLayer = ~0; // por defecto todo; ajusta en inspector
    [SerializeField] private float returnDuration = 0.25f; // tiempo para volver a la pos original
    [SerializeField] private float moveToSafeDuration = 0.5f; // tiempo para moverse al centro de la zona segura
    [SerializeField] private bool snapToCenterOnSafe = false; // si true, al llegar "snapea" exactamente al centro

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private HashSet<GameObject> securedObjects = new HashSet<GameObject>(); // objetos ya asegurados (no se pueden agarrar)

    // estado del arrastre
    private GameObject grabbedObject = null;
    private Vector3 grabbedOriginalPos;
    private Vector3 grabOffset; // offset entre punto de hit y el centro del objeto

    void Start()
    {
        int numR = Random.Range(4, 8); // 4..7
        for (int i = 0; i < numR; i++)
        {
            GameObject go = Instantiate(boneBreak_pb, spawnPoints[i].position, Quaternion.Euler(0, 0, 0));
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
        
        //if()
    }

    private void TryGrabObject()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            GameObject candidate = hit.collider.gameObject;

            // Si ya está asegurado, no se puede coger
            if (securedObjects.Contains(candidate)) return;

            grabbedObject = candidate;

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
        if (grabbedObject == null)
            return;

        // comprobar si dentro de zona segura
        bool insideSafe = false;
        if (safeZoneCenter != null)
        {
            float dist = Vector3.Distance(grabbedObject.transform.position, safeZoneCenter.position);
            if (dist <= safeZoneRadius) insideSafe = true;
        }

        // restaurar física si procede (si no vamos a animar hacia la zona)
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (insideSafe)
        {
            // Inicia movimiento hacia la zona segura
            Vector3 target = snapToCenterOnSafe && safeZoneCenter != null ? safeZoneCenter.position : GetRandomPointNearSafeCenter();
            // Marca como asegurado inmediatamente para que no pueda volver a agarrarse durante la animación
            securedObjects.Add(grabbedObject);
            // Desactivamos collider y hacemos kinematic para evitar interacciones físicas mientras se mueve
            Collider col = grabbedObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            if (rb != null) rb.isKinematic = true;

            StartCoroutine(MoveToSafeZone(grabbedObject, target, moveToSafeDuration));
        }
        else
        {
            // volver a la pos original suavemente
            Vector3 target = grabbedOriginalPos;
            StartCoroutine(ReturnToOriginal(grabbedObject, target, returnDuration));
        }

        grabbedObject = null;
    }

    private Vector3 GetRandomPointNearSafeCenter()
    {
        if (safeZoneCenter == null) return Vector3.zero;
        // generamos un punto aleatorio pequeño alrededor del centro para que no todos apunten exactamente al mismo punto
        Vector2 rnd = Random.insideUnitCircle * (safeZoneRadius * 0.5f);
        Vector3 offset = new Vector3(rnd.x, 0f, rnd.y);
        return safeZoneCenter.position + offset;
    }

    private IEnumerator MoveToSafeZone(GameObject obj, Vector3 targetPos, float duration)
    {
        if (obj == null)
            yield break;

        float t = 0f;
        Vector3 start = obj.transform.position;

        if (duration <= 0f)
        {
            obj.transform.position = targetPos;
            FinalizeSafeObject(obj);
            yield break;
        }

        // opcional: pequeña animación de rotación mientras se mueve
        Quaternion startRot = obj.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation((safeZoneCenter != null ? (safeZoneCenter.position - start) : (Vector3.zero - start)).normalized, Vector3.up);

        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / duration);
            // ease out
            float ease = 1f - Mathf.Pow(1f - frac, 3f);
            if (obj != null)
            {
                obj.transform.position = Vector3.Lerp(start, targetPos, ease);
                obj.transform.rotation = Quaternion.Slerp(startRot, endRot, ease);
            }
            yield return null;
        }

        if (obj != null)
        {
            obj.transform.position = targetPos;
            FinalizeSafeObject(obj);
        }
    }

    // Lo que ocurre cuando el objeto ya ha llegado a la zona segura:
    // - opcionalmente parentearlo al safeZoneCenter (o dejarlo suelto)
    // - desactivar colisionador (ya se hizo antes), dejar rb isKinematic
    private void FinalizeSafeObject(GameObject obj)
    {
        if (obj == null) return;

        // mantenerlo kinematic y sin collider (ya debería estar así)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // parent al safe zone (opcional para organizar jerarquía)
        if (safeZoneCenter != null)
        {
            obj.transform.SetParent(safeZoneCenter, true);
        }

        // aquí podrías reproducir un sonido, incrementer contador de piezas aseguradas, o destruir el objeto
        // Destroy(obj); // o StartCoroutine(DelayedDestroy(obj, 1f));
    }

    private IEnumerator ReturnToOriginal(GameObject obj, Vector3 targetPos, float duration)
    {
        if (obj == null)
            yield break;

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
        if (obj != null) obj.transform.position = targetPos;
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
