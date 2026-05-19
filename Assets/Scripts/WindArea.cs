using UnityEngine;

/// <summary>
/// Componente que define un área con viento que empuja objetos.
/// Adjuntar a un GameObject con Collider marcado como Trigger.
/// El GameObject debe estar en la Layer "Wind".
/// </summary>
public class WindArea : MonoBehaviour
{
    #region Configuración del Viento
    [Header("Dirección del Viento")]
    [Tooltip("Dirección en la que sopla el viento (será normalizada automáticamente)")]
    [SerializeField] private Vector3 windDirection = Vector3.up;

    [Tooltip("Usar la rotación del objeto para definir la dirección del viento")]
    [SerializeField] private bool useTransformForward = false;

    [Header("Visualización")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.5f, 0.8f, 1f, 0.3f);
    [SerializeField] private Color arrowColor = Color.cyan;

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private AudioSource windSound;

    #endregion

    #region Variables Privadas
    private Vector3 normalizedWindDirection;
    private Collider windCollider;
    #endregion

    #region Inicialización
    private void Start()
    {
        ValidateSetup();
        UpdateWindDirection();
    }

    /// <summary>
    /// Valida que el componente esté configurado correctamente
    /// </summary>
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

        // Verificar que está en la layer correcta
        if (LayerMask.LayerToName(gameObject.layer) != "Wind")
        {
            Debug.LogWarning($"WindArea en {gameObject.name} debe estar en la Layer 'Wind'!");
        }

        // Verificar que la dirección no es cero
        if (windDirection.magnitude < 0.001f && !useTransformForward)
        {
            Debug.LogWarning($"WindArea en {gameObject.name}: dirección del viento es cero! Usando Vector3.up por defecto");
            windDirection = Vector3.up;
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        // Actualizar dirección si usamos la rotación del transform
        if (useTransformForward)
        {
            UpdateWindDirection();
        }
    }
    #endregion

    #region Gestión de Dirección
    /// <summary>
    /// Actualiza y normaliza la dirección del viento
    /// </summary>
    private void UpdateWindDirection()
    {
        if (useTransformForward)
        {
            normalizedWindDirection = transform.forward;
        }
        else
        {
            normalizedWindDirection = windDirection.normalized;
        }
    }

    /// <summary>
    /// Obtiene la dirección normalizada del viento
    /// </summary>
    public Vector3 GetWindDirection()
    {
        return normalizedWindDirection;
    }

    /// <summary>
    /// Establece una nueva dirección de viento
    /// </summary>
    public void SetWindDirection(Vector3 newDirection)
    {
        windDirection = newDirection;
        UpdateWindDirection();
    }
    #endregion

    #region Triggers - Efectos Visuales y Sonoros
    private void OnTriggerEnter(Collider other)
    {
        // Activar partículas si existen
        if (particles != null && !particles.isPlaying)
        {
            particles.Play();
        }

        // Activar sonido si existe
        if (windSound != null && !windSound.isPlaying)
        {
            windSound.Play();
        }

        // Debug
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null)
        {
            Debug.Log($"GolfBall entró en WindArea: {gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Desactivar partículas si no hay más objetos
        if (particles != null && particles.isPlaying)
        {
            particles.Stop();
        }

        // Fade out del sonido
        if (windSound != null && windSound.isPlaying)
        {
            windSound.Stop();
        }

        // Debug
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null)
        {
            Debug.Log($"GolfBall salió de WindArea: {gameObject.name}");
        }
    }
    #endregion

    #region Gizmos - Visualización en el Editor
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Actualizar dirección para Gizmos
        Vector3 direction = useTransformForward ? transform.forward : windDirection.normalized;

        // Dibujar el área del trigger
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
        else if (col is CapsuleCollider capsuleCol)
        {
            // Aproximación con esferas
            Vector3 center = capsuleCol.center;
            float radius = capsuleCol.radius;
            Gizmos.DrawSphere(center, radius);
        }

        // Volver a world space para las flechas
        Gizmos.matrix = Matrix4x4.identity;

        // Dibujar flechas indicando dirección del viento
        DrawWindArrows(direction);
    }

    /// <summary>
    /// Dibuja flechas para visualizar la dirección del viento
    /// </summary>
    private void DrawWindArrows(Vector3 direction)
    {
        Gizmos.color = arrowColor;
        Vector3 center = transform.position;

        // Grid de flechas
        int gridSize = 3;
        float spacing = 0.7f;

        for (int i = -gridSize; i <= gridSize; i++)
        {
            for (int j = -gridSize; j <= gridSize; j++)
            {
                Vector3 offset = transform.right * i * spacing + transform.up * j * spacing;
                Vector3 arrowStart = center + offset;
                Vector3 arrowEnd = arrowStart + direction * 0.8f;

                // Línea principal de la flecha
                Gizmos.DrawLine(arrowStart, arrowEnd);

                // Punta de la flecha
                Vector3 arrowTip = arrowEnd;
                Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
                if (perpendicular1.magnitude < 0.1f)
                {
                    perpendicular1 = Vector3.Cross(direction, Vector3.right).normalized;
                }
                Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

                float arrowHeadSize = 0.15f;
                Gizmos.DrawLine(arrowTip, arrowTip - direction * arrowHeadSize + perpendicular1 * arrowHeadSize);
                Gizmos.DrawLine(arrowTip, arrowTip - direction * arrowHeadSize - perpendicular1 * arrowHeadSize);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Borde más visible cuando está seleccionado
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.cyan;
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
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Actualizar en tiempo real en el editor
        UpdateWindDirection();
    }
#endif
    #endregion

    #region Public API
    /// <summary>
    /// Rota la dirección del viento hacia una nueva dirección suavemente
    /// </summary>
    public void RotateWindDirection(Vector3 targetDirection, float rotationSpeed)
    {
        windDirection = Vector3.RotateTowards(windDirection, targetDirection, rotationSpeed * Time.deltaTime, 0f);
        UpdateWindDirection();
    }

    /// <summary>
    /// Invierte la dirección del viento
    /// </summary>
    public void InvertWindDirection()
    {
        windDirection = -windDirection;
        UpdateWindDirection();
    }

    /// <summary>
    /// Obtiene información de debug del área de viento
    /// </summary>
    public string GetDebugInfo()
    {
        return $"WindArea: {gameObject.name}\n" +
               $"Dirección: {normalizedWindDirection}\n" +
               $"Usando Transform: {useTransformForward}\n" +
               $"Layer: {LayerMask.LayerToName(gameObject.layer)}";
    }
    #endregion
}