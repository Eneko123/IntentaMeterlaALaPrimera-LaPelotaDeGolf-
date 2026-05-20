using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Obtiene las direcciones relativas a la cámara
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        up.Normalize();
        right.Normalize();

        // Calculamos el vector de movimiento
        Vector3 move = up * v + right * h;

        // Evita que la velocidad diagonal sea mayor
        move = Vector3.ClampMagnitude(move, 1f);

        // Aplicamos el desplazamiento con deltaTime para movimiento independiente de FPS
        transform.position += move * speed * Time.deltaTime;
    }
}