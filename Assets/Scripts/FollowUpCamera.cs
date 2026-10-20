using UnityEngine;

public class FollowUpCamera : MonoBehaviour
{
    public Transform target;                         // El jugador
    public Vector3 offset = new Vector3(0, 5, -7);   // Arriba (Y) y atrás (Z)
    public float smoothSpeed = 10f;                  // Suavizado (0 = sin seguimiento)

    void LateUpdate()
    {
        // Posición deseada: offset relativo a la rotación del jugador
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Movimiento suave hacia esa posición
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Siempre mirar al jugador (un poco arriba de sus pies)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
