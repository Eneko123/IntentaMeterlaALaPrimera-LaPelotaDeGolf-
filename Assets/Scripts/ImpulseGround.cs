using UnityEngine;

// Permite configurar la dirección y magnitud del impulso de forma flexible.
[RequireComponent(typeof(Collider))]
public class ImpulseGround : MonoBehaviour
{
    #region Enums
    public enum ImpulseDirection
    {
        Right,      // +X
        Left,       // -X
        Up,         // +Y
        Down,       // -Y
        Custom      // Dirección personalizada
    }
    #endregion

    #region Parametros de Configuración
    [Header("Configuración del Impulso")]
    // Dirección predefinida del impulso
    [SerializeField] private ImpulseDirection impulseDirection = ImpulseDirection.Up;

    // Dirección personalizada del impulso (solo si impulseDirection es Custom
    [SerializeField] private Vector3 customDirection = Vector3.up;

    // Magnitud del impulso aplicado
    [SerializeField] private float impulseForce = 5f;

    // Usar dirección local del objeto en lugar de dirección global
    [SerializeField] private bool useLocalDirection = false;

    // Tiempo mínimo entre impulsos consecutivos (en segundos)
    [SerializeField] private float cooldownTime = 0.5f;

    // Aplicar el impulso solo una vez por contacto
    [SerializeField] private bool singleUse = false;

    #endregion

    #region Variables Privadas
    private float lastImpulseTime = -999f;
    private bool hasBeenUsed = false;
    private Collider areaCollider;
    private Vector3 cachedImpulseVector;
    #endregion

    #region Inicialización
    void Start()
    {
        ValidateSetup();
        areaCollider = GetComponent<Collider>();
        UpdateImpulseVector();
    }

    // Valida la configuración del componente
    private void ValidateSetup()
    {
        // Verificar que tenga un collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"ImpulseGround en {gameObject.name} necesita un Collider!");
        }

        // Advertir sobre el layer
        string layerName = LayerMask.LayerToName(gameObject.layer);
        if (layerName != "ImpulseGroundX" && layerName != "ImpulseGroundY")
        {
            Debug.LogWarning($"ImpulseGround en {gameObject.name} debería estar en layer 'ImpulseGroundX' o 'ImpulseGroundY'. " +
                           $"Layer actual: {layerName}");
        }

        // Validar fuerza
        if (impulseForce <= 0)
        {
            Debug.LogWarning($"ImpulseGround en {gameObject.name}: impulseForce debe ser mayor que 0");
            impulseForce = 5f;
        }
    }

    // Actualiza el vector de impulso basado en la configuración
    private void UpdateImpulseVector()
    {
        Vector3 direction = Vector3.zero;

        switch (impulseDirection)
        {
            case ImpulseDirection.Right:
                direction = Vector3.right;
                break;
            case ImpulseDirection.Left:
                direction = Vector3.left;
                break;
            case ImpulseDirection.Up:
                direction = Vector3.up;
                break;
            case ImpulseDirection.Down:
                direction = Vector3.down;
                break;
            case ImpulseDirection.Custom:
                direction = customDirection.normalized;
                break;
        }

        // Aplicar transformación local si está habilitada
        if (useLocalDirection)
        {
            direction = transform.TransformDirection(direction);
        }

        cachedImpulseVector = direction * impulseForce;
    }

    // Actualiza el vector de impulso en el editor cuando cambian los valores
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateImpulseVector();
        }
    }
    #endregion

    #region Propiedades Públicas
    // Obtiene el vector de impulso completo
    public Vector3 GetImpulseVector()
    {
        return cachedImpulseVector;
    }

    // Obtiene la magnitud del impulso
    public float GetImpulseForce()
    {
        return impulseForce;
    }

    // Verifica si el impulso esta listo para aplicarse
    public bool CanApplyImpulse()
    {
        if (singleUse && hasBeenUsed)
            return false;

        return Time.time - lastImpulseTime >= cooldownTime;
    }

    // Registra que se aplico un impulso
    public void RegisterImpulse()
    {
        lastImpulseTime = Time.time;
        if (singleUse)
        {
            hasBeenUsed = true;
        }
    }
    #endregion

    #region Detección de Colisiones
    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si es la pelota de golf
        GolfBall ball = collision.gameObject.GetComponent<GolfBall>();
        if (ball != null && ball.rb != null)
        {
            OnBallContact(ball);
        }
    }

    // Maneja el contacto con la pelota
    private void OnBallContact(GolfBall ball)
    {
        if (!CanApplyImpulse())
            return;

        // Aplicar el impulso directamente
        ball.rb.AddForce(cachedImpulseVector, ForceMode.Impulse);

        // Registrar el impulso
        RegisterImpulse();
    }
    #endregion
}