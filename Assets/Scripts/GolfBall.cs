using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

/// <summary>
/// Componente principal de la pelota de golf que maneja física, colisiones y efectos del entorno.
/// Cumple con los requisitos del proyecto:
/// - Aplicación de fuerzas e impulsos
/// - Comprobación de velocidad (límite máximo, desaceleración basada en velocidad)
/// - Objetos físicos interactivos (diferentes tipos de suelo y áreas de efecto)
/// </summary>
public class GolfBall : MonoBehaviour
{
    #region Componentes y Referencias
    internal Rigidbody rb;
    internal LineRenderer _line;

    [Header("Referencias")]
    [SerializeField] private GolfStick _golfStick;

    #endregion

    #region Parámetros de Física del Suelo
    [Header("Física del Suelo")]
    [SerializeField] private float _NormalGroundDeceleration = 0.02f;
    [SerializeField] private float _SlowGroundDeceleration = 0.04f;
    [SerializeField] private float _FastGroundAcceleration = 0.01f;
    [SerializeField] private float _impulse = 5f;

    #endregion

    #region Parámetros de Áreas de Efecto
    [Header("Áreas de Efecto")]
    [SerializeField] private float _windForce = 2f;

    [Header("Límites de Velocidad")]
    [SerializeField] private float _maxVelocity = 30f;
    [SerializeField] private float _minVelocityThreshold = 0.005f;

    #endregion

    #region Parámetros de Trayectoria
    [Header("Visualización de Trayectoria")]
    [SerializeField] private int resolution = 30;
    [SerializeField] private float timeStep = 0.1f;
    [SerializeField] private Transform launchPoint;

    #endregion

    #region Estados de Colisión
    // Estados de contacto con diferentes superficies
    private GroundContactState groundState;

    // Estados de áreas de efecto
    private EnvironmentEffectState environmentState;

    #endregion

    #region Estructuras de Estado
    /// <summary>
    /// Estructura que agrupa todos los estados de contacto con el suelo
    /// </summary>
    private struct GroundContactState
    {
        public bool onNormalGround;
        public bool onSlowGround;
        public bool onFastGround;
        public bool onImpulseGroundX;
        public bool onImpulseGroundY;

        public void Reset()
        {
            onNormalGround = false;
            onSlowGround = false;
            onFastGround = false;
            onImpulseGroundX = false;
            onImpulseGroundY = false;
        }

        public bool IsGrounded()
        {
            return onNormalGround || onSlowGround || onFastGround ||
                   onImpulseGroundX || onImpulseGroundY;
        }
    }

    /// <summary>
    /// Estructura que agrupa todos los estados de efectos ambientales
    /// </summary>
    private struct EnvironmentEffectState
    {
        public bool inWindArea;
        public Vector3 windDirection;
        public bool inInvertedGravityArea;
        public float gravityMultiplier;

        public void Reset()
        {
            inWindArea = false;
            windDirection = Vector3.zero;
            inInvertedGravityArea = false;
            gravityMultiplier = 2f;
        }
    }

    #endregion

    #region Inicialización
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _line = GetComponent<LineRenderer>();

        groundState = new GroundContactState();
        groundState.Reset();

        environmentState = new EnvironmentEffectState();
        environmentState.Reset();

        ValidateComponents();
    }

    /// <summary>
    /// Valida que todos los componentes necesarios estén presentes
    /// </summary>
    private void ValidateComponents()
    {
        if (rb == null)
        {
            Debug.LogError($"GolfBall en {gameObject.name} necesita un Rigidbody!");
        }

        if (_line == null)
        {
            Debug.LogWarning($"GolfBall en {gameObject.name} no tiene LineRenderer - la trayectoria no se mostrará");
        }

        if (_golfStick == null)
        {
            Debug.LogWarning($"GolfBall en {gameObject.name} no tiene referencia a GolfStick");
        }

        if (launchPoint == null)
        {
            launchPoint = transform;
            Debug.LogWarning($"GolfBall en {gameObject.name}: usando transform de la pelota como punto de lanzamiento");
        }
    }

    #endregion

    #region Update Loop
    void Update()
    {
        // Solo dibujamos la trayectoria cuando NO se está presionando espacio
        if (!Input.GetKey(KeyCode.Space) && _line != null)
        {
            DrawTrajectory();
        }
    }

    #endregion

    #region Visualización de Trayectoria
    /// <summary>
    /// Dibuja la trayectoria predicha de la pelota usando física parabólica
    /// </summary>
    void DrawTrajectory()
    {
        if (_golfStick == null || _line == null) return;

        Vector3[] points = new Vector3[resolution];
        Vector3 startPos = launchPoint.position;
        Vector3 startVel = (Vector3)_golfStick.GetVectorAngle() * _golfStick.GetForce();

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            // Ecuación de movimiento parabólico: s = s₀ + v₀t + ½at²
            points[i] = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
        }

        _line.positionCount = resolution;
        _line.SetPositions(points);
    }

    /// <summary>
    /// Elimina la visualización de la trayectoria
    /// </summary>
    public void DeleteTrajectory()
    {
        if (_line != null)
        {
            _line.enabled = false;
        }
    }

    #endregion

    #region Física - FixedUpdate
    private void FixedUpdate()
    {
        ApplyGroundPhysics();
        ApplyEnvironmentEffects();
        LimitVelocity();
    }

    /// <summary>
    /// Aplica todas las físicas relacionadas con el contacto con el suelo
    /// </summary>
    private void ApplyGroundPhysics()
    {
        if (!IsMoving()) return;

        // Aplicar efectos de suelo en orden de prioridad
        if (groundState.onSlowGround)
        {
            ApplySlowGroundDeceleration();
        }
        else if (groundState.onFastGround)
        {
            ApplyFastGroundAcceleration();
        }
        else if (groundState.onNormalGround)
        {
            ApplyNormalDeceleration();
        }

        // Impulsos son independientes del tipo de suelo
        if (groundState.onImpulseGroundX)
        {
            ApplyImpulseX();
        }

        if (groundState.onImpulseGroundY)
        {
            ApplyImpulseY();
        }
    }

    /// <summary>
    /// Aplica desaceleración en suelo normal
    /// </summary>
    private void ApplyNormalDeceleration()
    {
        rb.linearVelocity -= rb.linearVelocity * _NormalGroundDeceleration;
    }

    /// <summary>
    /// Aplica desaceleración aumentada en suelo lento (fricción alta)
    /// </summary>
    private void ApplySlowGroundDeceleration()
    {
        rb.linearVelocity -= rb.linearVelocity * _SlowGroundDeceleration;
    }

    /// <summary>
    /// Aplica aceleración en suelo rápido (baja fricción o impulso)
    /// </summary>
    private void ApplyFastGroundAcceleration()
    {
        rb.linearVelocity += rb.linearVelocity * _FastGroundAcceleration;
    }

    /// <summary>
    /// Aplica impulso horizontal (AddForce con ForceMode.Impulse)
    /// </summary>
    private void ApplyImpulseX()
    {
        rb.AddForce(Vector2.right * _impulse, ForceMode.Impulse);
        // Reset del estado para evitar múltiples impulsos
        groundState.onImpulseGroundX = false;
    }

    /// <summary>
    /// Aplica impulso vertical (AddForce con ForceMode.Impulse)
    /// </summary>
    private void ApplyImpulseY()
    {
        rb.AddForce(Vector2.up * _impulse, ForceMode.Impulse);
        // Reset del estado para evitar múltiples impulsos
        groundState.onImpulseGroundY = false;
    }

    /// <summary>
    /// Aplica todos los efectos ambientales (viento, gravedad invertida)
    /// </summary>
    private void ApplyEnvironmentEffects()
    {
        if (environmentState.inWindArea)
        {
            ApplyWindForce();
        }

        if (environmentState.inInvertedGravityArea)
        {
            ApplyInvertedGravity();
        }
    }

    /// <summary>
    /// Aplica fuerza de viento constante (AddForce con ForceMode.Force)
    /// </summary>
    private void ApplyWindForce()
    {
        if (!IsMoving()) return;
        rb.AddForce(environmentState.windDirection * _windForce, ForceMode.Force);
    }

    /// <summary>
    /// Aplica gravedad invertida cancelando la gravedad normal
    /// </summary>
    private void ApplyInvertedGravity()
    {
        // Cancelar la gravedad normal y aplicar gravedad invertida
        // Gravedad de Unity es aproximadamente -9.81 en Y
        rb.AddForce(Vector3.up * Mathf.Abs(Physics.gravity.y) * environmentState.gravityMultiplier, ForceMode.Acceleration);
    }

    /// <summary>
    /// Limita la velocidad máxima de la pelota (requisito del proyecto)
    /// </summary>
    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > _maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * _maxVelocity;
        }
    }

    /// <summary>
    /// Comprueba si la pelota se está moviendo significativamente
    /// </summary>
    private bool IsMoving()
    {
        return rb.linearVelocity.magnitude > _minVelocityThreshold;
    }

    #endregion

    #region Gestión de Colisiones - Suelo
    /// <summary>
    /// Detecta contacto continuo con diferentes tipos de suelo
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        string layerName = LayerMask.LayerToName(collision.gameObject.layer);

        switch (layerName)
        {
            case "Ground":
                groundState.onNormalGround = true;
                break;
            case "SlowGround":
                groundState.onSlowGround = true;
                break;
            case "FastGround":
                groundState.onFastGround = true;
                break;
            case "ImpulseGroundX":
                groundState.onImpulseGroundX = true;
                break;
            case "ImpulseGroundY":
                groundState.onImpulseGroundY = true;
                break;
        }
    }

    /// <summary>
    /// Detecta cuando la pelota deja de tocar un tipo de suelo
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        string layerName = LayerMask.LayerToName(collision.gameObject.layer);

        switch (layerName)
        {
            case "Ground":
                groundState.onNormalGround = false;
                break;
            case "SlowGround":
                groundState.onSlowGround = false;
                break;
            case "FastGround":
                groundState.onFastGround = false;
                break;
            case "ImpulseGroundX":
                groundState.onImpulseGroundX = false;
                break;
            case "ImpulseGroundY":
                groundState.onImpulseGroundY = false;
                break;
        }
    }

    #endregion

    #region Gestión de Triggers - Áreas de Efecto
    /// <summary>
    /// Detecta entrada continua en áreas de efecto especiales
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        switch (layerName)
        {
            case "Wind":
                HandleWindArea(other);
                break;
            case "InvertedGravity":
                HandleInvertedGravityArea(other);
                break;
        }
    }

    /// <summary>
    /// Maneja la entrada en un área de viento
    /// </summary>
    private void HandleWindArea(Collider windCollider)
    {
        environmentState.inWindArea = true;

        // Obtener la dirección del viento desde el componente WindArea
        WindArea windArea = windCollider.GetComponent<WindArea>();
        if (windArea != null)
        {
            environmentState.windDirection = windArea.GetWindDirection();
        }
        else
        {
            // Dirección por defecto si no hay componente WindArea
            environmentState.windDirection = Vector3.up;
            Debug.LogWarning($"WindArea en {windCollider.gameObject.name} no tiene componente WindArea");
        }
    }

    /// <summary>
    /// Maneja la entrada en un área de gravedad invertida
    /// </summary>
    private void HandleInvertedGravityArea(Collider gravityCollider)
    {
        environmentState.inInvertedGravityArea = true;

        // Obtener el multiplicador de gravedad si existe
        InvertedGravityArea gravityArea = gravityCollider.GetComponent<InvertedGravityArea>();
        if (gravityArea != null)
        {
            environmentState.gravityMultiplier = gravityArea.GetGravityMultiplier();
        }
        else
        {
            environmentState.gravityMultiplier = 2f; // Valor por defecto
        }
    }

    /// <summary>
    /// Detecta cuando la pelota sale de áreas de efecto
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        switch (layerName)
        {
            case "Wind":
                environmentState.inWindArea = false;
                environmentState.windDirection = Vector3.zero;
                break;
            case "InvertedGravity":
                environmentState.inInvertedGravityArea = false;
                break;
        }
    }

    #endregion

    #region Utilidades y Debug
    /// <summary>
    /// Obtiene información del estado actual de la pelota para debugging
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Velocidad: {rb.linearVelocity.magnitude:F2} m/s\n" +
               $"En suelo: {groundState.IsGrounded()}\n" +
               $"En viento: {environmentState.inWindArea}\n" +
               $"Gravedad invertida: {environmentState.inInvertedGravityArea}";
    }

    #endregion

    #region Gizmos (Editor)
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || rb == null) return;

        // Dibujar vector de velocidad
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity * 0.5f);

        // Dibujar estados
        if (groundState.IsGrounded())
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }

        if (environmentState.inWindArea)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + environmentState.windDirection);
        }
    }
#endif
    #endregion
}