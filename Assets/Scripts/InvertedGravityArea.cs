using UnityEngine;

/// <summary>
/// Componente que define un área con gravedad invertida.
/// Adjuntar a un GameObject con Collider marcado como Trigger.
/// El GameObject debe estar en la Layer "InvertedGravity".
/// </summary>
public class InvertedGravityArea : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Multiplicador de la gravedad invertida (2 = gravedad normal hacia arriba)")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.3f, 0.8f, 0.3f);
    [SerializeField] private ParticleSystem particles;

    private void Start()
    {
        // Verificar que tiene un trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"InvertedGravityArea en {gameObject.name} necesita un Collider!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"InvertedGravityArea en {gameObject.name}: El Collider debe ser Trigger!");
            col.isTrigger = true;
        }

        // Verificar que está en la layer correcta
        if (LayerMask.LayerToName(gameObject.layer) != "InvertedGravity")
        {
            Debug.LogWarning($"InvertedGravityArea en {gameObject.name} debe estar en la Layer 'InvertedGravity'!");
        }
    }

    public float GetGravityMultiplier()
    {
        return gravityMultiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Activar partículas si existen
        if (particles != null && !particles.isPlaying)
        {
            particles.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Desactivar partículas si no hay más objetos
        if (particles != null && particles.isPlaying)
        {
            particles.Stop();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Dibujar el área con color distintivo
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider boxCol)
        {
            Gizmos.DrawCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
        }

        // Dibujar flechas hacia arriba para indicar gravedad invertida
        Gizmos.color = Color.magenta;
        Vector3 center = transform.position;
        float spacing = 0.5f;

        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector3 pos = center + new Vector3(i * spacing, 0, j * spacing);
                Gizmos.DrawLine(pos, pos + Vector3.up * 0.8f);

                // Punta de flecha
                Gizmos.DrawLine(pos + Vector3.up * 0.8f, pos + Vector3.up * 0.6f + Vector3.right * 0.1f);
                Gizmos.DrawLine(pos + Vector3.up * 0.8f, pos + Vector3.up * 0.6f + Vector3.left * 0.1f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Borde más visible cuando está seleccionado
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider boxCol)
        {
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
        }
    }
}