using UnityEngine;

public class Bisturi_N3_Manager : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;

    private bool started = false;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Si NO es herida → reset
                if (!hit.collider.CompareTag("Wound"))
                {
                    ResetGame();
                    return;
                }

                // Si es herida
                if (!started)
                {
                    // Comprobamos si empezamos cerca del punto A
                    if (Vector3.Distance(hit.point, puntoA.position) < 0.1f)
                    {
                        started = true;
                        Debug.Log("Inicio correcto");
                    }
                    else
                    {
                        ResetGame();
                    }
                }
                else
                {
                    // Ya empezamos → comprobamos llegada a B
                    if (Vector3.Distance(hit.point, puntoB.position) < 0.1f)
                    {
                        Debug.Log("¡Minijuego completado!");
                        started = false;
                    }
                }
            }
        }
    }

    void ResetGame()
    {
        started = false;
        Debug.Log("Fallaste, reiniciando");
    }
}
