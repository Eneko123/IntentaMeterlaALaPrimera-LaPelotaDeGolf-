using UnityEngine;

// Componente base para superficies que modifican el comportamiento de la pelota.
[RequireComponent(typeof(Collider))]
public class GroundSurface : MonoBehaviour
{
    #region Enums
    public enum SurfaceType
    {
        Normal,         // Desaceleración estándar
        Slow,          // Alta fricción
        Fast,          // Baja fricción / acelera
        Custom         // Valores personalizados
    }

    public enum PhysicsMode
    {
        Deceleration,   // Reduce velocidad (fricción)
        Acceleration,   // Aumenta velocidad
        SpeedMultiplier // Multiplica velocidad actual
    }
    #endregion

    #region Configuración
    [Header("Tipo de Superficie")]
    // Preset de comportamiento (o Custom para valores manuales)
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Normal;

    // Física Personalizada (Solo si Type = Custom)
    [SerializeField] private PhysicsMode physicsMode = PhysicsMode.Deceleration;

    // Valor de fricción/aceleración/multiplicador según el modo
    [SerializeField] private float physicsValue = 0.02f;

    [Header("Configuración Avanzada")]
    // Aplicar efecto solo si velocidad supera este umbral
    [SerializeField] private float minVelocityThreshold = 0.005f;

    // Aplicar efecto solo si velocidad está por debajo de este límite
    [SerializeField] private float maxVelocityThreshold = 100f;

    // Multiplicador adicional del efecto (1.0 = normal
    [SerializeField] private float effectMultiplier = 1f;
    #endregion

    #region Variables Privadas
    private float cachedPhysicsValue;
    private PhysicsMode cachedPhysicsMode;
    #endregion

    #region Inicialización
    void Start()
    {
        ValidateSetup();
        UpdateCachedValues();
    }

    private void ValidateSetup()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"GroundSurface en {gameObject.name} necesita un Collider!");
        }

        if (physicsValue < 0)
        {
            Debug.LogWarning($"GroundSurface en {gameObject.name}: physicsValue negativo puede causar comportamiento extraño");
        }
    }

    private void UpdateCachedValues()
    {
        // Aplicar presets o usar valores custom
        switch (surfaceType)
        {
            case SurfaceType.Normal:
                cachedPhysicsMode = PhysicsMode.Deceleration;
                cachedPhysicsValue = 0.02f;
                break;
            case SurfaceType.Slow:
                cachedPhysicsMode = PhysicsMode.Deceleration;
                cachedPhysicsValue = 0.04f;
                break;
            case SurfaceType.Fast:
                cachedPhysicsMode = PhysicsMode.Acceleration;
                cachedPhysicsValue = 0.01f;
                break;
            case SurfaceType.Custom:
                cachedPhysicsMode = physicsMode;
                cachedPhysicsValue = physicsValue;
                break;
        }

        // Aplicar multiplicador
        cachedPhysicsValue *= effectMultiplier;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCachedValues();
        }
    }
    #endregion

    #region API Publica
    // Aplica el efecto de la superficie a la pelota
    public void ApplyEffect(Rigidbody ballRb)
    {
        if (ballRb == null) return;

        float currentSpeed = ballRb.linearVelocity.magnitude;

        // Verificar umbrales
        if (currentSpeed < minVelocityThreshold || currentSpeed > maxVelocityThreshold)
            return;

        Vector3 velocity = ballRb.linearVelocity;

        switch (cachedPhysicsMode)
        {
            case PhysicsMode.Deceleration:
                // Friccion: reduce velocidad proporcionalmente
                ballRb.linearVelocity -= velocity * cachedPhysicsValue;
                break;

            case PhysicsMode.Acceleration:
                // Acelera en dirección del movimiento
                ballRb.linearVelocity += velocity * cachedPhysicsValue;
                break;

            case PhysicsMode.SpeedMultiplier:
                // Multiplica velocidad directamente
                ballRb.linearVelocity *= (1f + cachedPhysicsValue);
                break;
        }
    }

    public PhysicsMode GetPhysicsMode() => cachedPhysicsMode;
    public float GetPhysicsValue() => cachedPhysicsValue;
    public string GetSurfaceTypeName() => surfaceType.ToString();
    #endregion
}