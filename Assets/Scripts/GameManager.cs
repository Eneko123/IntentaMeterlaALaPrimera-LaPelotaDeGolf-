using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuracion de Intentos")]
    [SerializeField] private int maxAttempts = 10;
    private int attempts = 0;

    [Header("Referencias")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private GameUI gameUI;

    // Estados del juego
    public enum GameState
    {
        PreGame,    // Antes de empezar (configurando angulo y fuerza)
        Playing,    // Pelota en movimiento
        Won,        // Victoria
        GameOver    // Sin intentos
    }

    private GameState currentState = GameState.PreGame;
    private Vector3 ballInitialPosition;
    private Quaternion ballInitialRotation;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Guardar posicion inicial de la pelota
        if (ball != null)
        {
            ballInitialPosition = ball.transform.position;
            ballInitialRotation = ball.transform.rotation;
        }

        currentState = GameState.PreGame;
    }

    void Update()
    {
        // Comprobar si la pelota esta casi parada
        if (currentState == GameState.Playing && ball != null)
        {
            if (ball.rb.linearVelocity.magnitude < 0.1f && ball.rb.angularVelocity.magnitude < 0.1f)
            {
                // Esperar un poco antes de auto-reiniciar
                Invoke(nameof(CheckIfStillStopped), 1f);
            }
        }
    }

    private void CheckIfStillStopped()
    {
        if (currentState == GameState.Playing && ball != null)
        {
            if (ball.rb.linearVelocity.magnitude < 0.1f)
            {
                Debug.Log("Pelota detenida - Auto reinicio");
                RetryLevel();
            }
        }
    }

    // Se llama cuando la pelota es golpeada (desde GolfStick o GolfBall)
    public void OnBallHit()
    {
        if (currentState != GameState.PreGame) return;

        attempts++;
        currentState = GameState.Playing;

        // Notificar a la UI
        if (gameUI != null)
        {
            gameUI.OnGameStart();
            gameUI.UpdateAttemptsUI(attempts, maxAttempts);
        }

        Debug.Log($"Intento {attempts}/{maxAttempts}");
    }

    // Se llama cuando la pelota toca el hoyo (Victoria)
    public void OnBallInHole()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        Debug.Log("Victoria!");

        if (gameUI != null)
        {
            gameUI.ShowVictoryPanel();
        }
    }

    // Se llama cuando la pelota cae al vacio
    public void OnBallFellOff()
    {
        if (currentState != GameState.Playing) return;

        Debug.Log("Pelota cayo al vacio!");
        RetryLevel();
    }

    // Reintentar el nivel (mantiene el contador de intentos)
    public void RetryLevel()
    {
        if (attempts >= maxAttempts)
        {
            GameOver();
            return;
        }

        // Resetear pelota a posicion inicial
        ResetBall();

        currentState = GameState.PreGame;

        if (gameUI != null)
        {
            gameUI.OnRetry();
        }

        Debug.Log("Reiniciando nivel...");
    }

    // Reiniciar el juego completamente (resetea intentos)
    public void RestartGame()
    {
        attempts = 0;
        ResetBall();

        currentState = GameState.PreGame;

        if (gameUI != null)
        {
            gameUI.OnGameRestart();
        }

        Debug.Log("Juego reiniciado");
    }

    // Game Over por falta de intentos
    private void GameOver()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over - Sin intentos");

        if (gameUI != null)
        {
            gameUI.ShowGameOverPanel();
        }
    }

    // Resetear la pelota a su posicion inicial
    private void ResetBall()
    {
        if (ball != null)
        {
            // Detener fisica
            ball.rb.linearVelocity = Vector3.zero;
            ball.rb.angularVelocity = Vector3.zero;

            // Restaurar posicion y rotacion
            ball.transform.position = ballInitialPosition;
            ball.transform.rotation = ballInitialRotation;

            // Volver a dibujar la trayectoria
            ball.ShowTrajectory();

            Debug.Log("Pelota reseteada");
        }
    }

    #region Getters
    public int GetAttempts() => attempts;
    public int GetMaxAttempts() => maxAttempts;
    public GameState GetCurrentState() => currentState;
    #endregion
}
