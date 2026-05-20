using UnityEngine;

// - Gestionar componentes propios (Rigidbody, LineRenderer)
// - Dibujar trayectoria predicha
// - Limitar velocidad maxima
// - Detectar contacto con superficies y delegar efectos
// - Aplicar efectos ambientales (viento, gravedad)
// - Detectar victoria (Hoyo) y caida (Vacio)
public class GolfBall : MonoBehaviour
{
    #region Referencias
    internal Rigidbody rb;
    internal LineRenderer _line;

    [Header("Referencias")]
    [SerializeField] private GolfStick _golfStick;
    [SerializeField] private Transform launchPoint;
    #endregion

    #region Parametros Globales
    [Header("Limites de Velocidad")]
    [SerializeField] private float _maxVelocity = 100f;
    [SerializeField] private float _minVelocityThreshold = 0.005f;

    [Header("Areas de Efecto - Fuerza de Viento")]
    // Multiplicador de fuerza para areas de viento
    [SerializeField] private float _windForceMultiplier = 2f;

    [Header("Deteccion de Victoria y Caída")]
    // Tiempo de gracia antes de auto-reiniciar por baja velocidad (segundos)
    [SerializeField] private float _autoRestartDelay = 2f;
    private float _stoppedTimer = 0f;
    private bool _hasNotifiedStop = false;
    #endregion

    #region Parametros de Trayectoria
    [Header("Visualizaci0n de Trayectoria")]
    [SerializeField] private int resolution = 30;
    [SerializeField] private float timeStep = 0.1f;
    #endregion

    #region Estados
    // Superficie actual en contacto
    private GroundSurface currentGroundSurface;

    // Estados de areas de efecto
    private struct EnvironmentState
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

    private EnvironmentState environmentState;
    #endregion

    #region Inicializacion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _line = GetComponent<LineRenderer>();

        environmentState = new EnvironmentState();
        environmentState.Reset();

        if (launchPoint == null)
        {
            launchPoint = transform;
        }

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (rb == null)
        {
            Debug.LogError($"GolfBall en {gameObject.name} necesita un Rigidbody!");
        }

        if (_line == null)
        {
            Debug.LogWarning($"GolfBall en {gameObject.name} no tiene LineRenderer");
        }

        if (_golfStick == null)
        {
            Debug.LogWarning($"GolfBall en {gameObject.name} no tiene referencia a GolfStick");
        }
    }
    #endregion

    #region Update - Trayectoria
    void Update()
    {
        // Dibujar trayectoria solo cuando NO se esta cargando el golpe
        if (!Input.GetKey(KeyCode.Space) && _line != null)
        {
            DrawTrajectory();
        }

        // Comprobar si la pelota esta detenida por mucho tiempo
        CheckIfStopped();
    }

    void DrawTrajectory()
    {
        if (_golfStick == null || _line == null) return;

        Vector3[] points = new Vector3[resolution];
        Vector3 startPos = launchPoint.position;
        Vector3 startVel = (Vector3)_golfStick.GetVectorAngle() * _golfStick.GetForce();

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            points[i] = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
        }

        _line.positionCount = resolution;
        _line.SetPositions(points);
    }

    public void DeleteTrajectory()
    {
        if (_line != null)
        {
            _line.enabled = false;
        }
    }

    // Mostrar la trayectoria 
    public void ShowTrajectory()
    {
        if (_line != null)
        {
            _line.enabled = true;
        }
        _stoppedTimer = 0f;
        _hasNotifiedStop = false;
    }

    // Comprobar si la pelota lleva mucho tiempo parada
    private void CheckIfStopped()
    {
        // Solo verificar si el juego esta en estado Playing
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.GetCurrentState() != GameManager.GameState.Playing) return;

        if (rb.linearVelocity.magnitude < _minVelocityThreshold * 20f) // Umbral mas alto para auto-restart
        {
            _stoppedTimer += Time.deltaTime;

            if (_stoppedTimer >= _autoRestartDelay && !_hasNotifiedStop)
            {
                _hasNotifiedStop = true;
                Debug.Log("Pelota detenida - Activando reinicio automatico");

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RetryLevel();
                }
            }
        }
        else
        {
            _stoppedTimer = 0f;
            _hasNotifiedStop = false;
        }
    }
    #endregion

    #region FixedUpdate - Fisica
    private void FixedUpdate()
    {
        ApplyGroundPhysics();
        ApplyEnvironmentEffects();
        LimitVelocity();
    }

    // Delega la fisica de suelo al componente GroundSurface actual
    private void ApplyGroundPhysics()
    {
        if (!IsMoving()) return;

        if (currentGroundSurface != null)
        {
            currentGroundSurface.ApplyEffect(rb);
        }
    }

    // Aplica efectos ambientales (viento, gravedad invertida)
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

    private void ApplyWindForce()
    {
        rb.AddForce(environmentState.windDirection * _windForceMultiplier, ForceMode.Force);
    }

    private void ApplyInvertedGravity()
    {
        rb.AddForce(Vector3.up * Mathf.Abs(Physics.gravity.y) * environmentState.gravityMultiplier,
                    ForceMode.Acceleration);
    }

    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > _maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * _maxVelocity;
        }
    }

    private bool IsMoving()
    {
        return rb.linearVelocity.magnitude > _minVelocityThreshold;
    }
    #endregion

    #region Colisiones - Delegacion a Componentes
    private void OnCollisionEnter(Collision collision)
    {
        // Buscar componente GroundSurface
        GroundSurface surface = collision.gameObject.GetComponent<GroundSurface>();
        if (surface != null)
        {
            currentGroundSurface = surface;
        }

        // Detectar si toco el Hoyo (Victoria)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hoyo"))
        {
            Debug.Log("Pelota en el hoyo!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBallInHole();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Mantener referencia a la superficie mientras estamos en contacto
        if (currentGroundSurface == null)
        {
            GroundSurface surface = collision.gameObject.GetComponent<GroundSurface>();
            if (surface != null)
            {
                currentGroundSurface = surface;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Limpiar referencia cuando dejamos la superficie
        GroundSurface surface = collision.gameObject.GetComponent<GroundSurface>();
        if (surface != null && currentGroundSurface == surface)
        {
            currentGroundSurface = null;
        }
    }
    #endregion

    #region Triggers - Areas de Efecto
    private void OnTriggerEnter(Collider other)
    {
        // Detectar caida al Vacio
        if (other.gameObject.layer == LayerMask.NameToLayer("Vacio"))
        {
            Debug.Log("Pelota cayo al vacio!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBallFellOff();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Detectar WindArea
        WindArea windArea = other.GetComponent<WindArea>();
        if (windArea != null)
        {
            environmentState.inWindArea = true;
            environmentState.windDirection = windArea.GetWindDirection();
        }

        // Detectar InvertedGravityArea
        InvertedGravityArea gravityArea = other.GetComponent<InvertedGravityArea>();
        if (gravityArea != null)
        {
            environmentState.inInvertedGravityArea = true;
            environmentState.gravityMultiplier = gravityArea.GetGravityMultiplier();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Salir de WindArea
        if (other.GetComponent<WindArea>() != null)
        {
            environmentState.inWindArea = false;
            environmentState.windDirection = Vector3.zero;
        }

        // Salir de InvertedGravityArea
        if (other.GetComponent<InvertedGravityArea>() != null)
        {
            environmentState.inInvertedGravityArea = false;
        }
    }
    #endregion
}
