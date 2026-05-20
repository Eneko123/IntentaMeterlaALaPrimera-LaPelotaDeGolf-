using UnityEngine;

// Componente que define un area con viento que empuja objetos.
public class WindArea : MonoBehaviour
{
    [Header("Dirección del Viento")]
    [Tooltip("Dirección en la que sopla el viento (será normalizada automáticamente)")]
    [SerializeField] private Vector3 windDirection = Vector3.up;

    [Tooltip("Usar la rotación del objeto para definir la dirección del viento")]
    [SerializeField] private bool useTransformForward = false;

    private Vector3 normalizedWindDirection;
    private Collider windCollider;

    private void Start()
    {
        ValidateSetup();
    }

    public Vector3 GetWindDirection() => windDirection;

    // Valida que el componente este configurado correctamente
    private void ValidateSetup()
    {
        windCollider = GetComponent<Collider>();

        if (windCollider == null)
        {
            Debug.LogError($"WindArea en {gameObject.name} necesita un Collider!");
        }
        else if (!windCollider.isTrigger)
        {
            Debug.LogWarning($"WindArea en {gameObject.name}: El Collider debe ser Trigger!");
            windCollider.isTrigger = true;
        }

        // Verificar que esta en la layer correcta
        if (LayerMask.LayerToName(gameObject.layer) != "Wind")
        {
            Debug.LogWarning($"WindArea en {gameObject.name} debe estar en la Layer 'Wind'!");
        }

        // Verificar que la direccion no es cero
        if (windDirection.magnitude < 0.001f && !useTransformForward)
        {
            Debug.LogWarning($"WindArea en {gameObject.name}: dirección del viento es cero! Usando Vector3.up por defecto");
            windDirection = Vector3.up;
        }
    }
}