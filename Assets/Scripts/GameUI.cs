using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject preGamePanel;
    [SerializeField] private GameObject playingPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Controles Pre-Juego")]
    [SerializeField] private Slider forceSlider;
    [SerializeField] private Slider angleSlider;
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI angleText;
    [SerializeField] private TextMeshProUGUI preGameAttemptsText;

    [Header("Panel de Juego")]
    [SerializeField] private Button retryButton;
    [SerializeField] private TextMeshProUGUI playingAttemptsText;

    [Header("Panel de Victoria")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TextMeshProUGUI victoryAttemptsText;

    [Header("Panel de Game Over")]
    [SerializeField] private Button restartButton;

    private GolfStick golfStick;

    void Start()
    {
        // Obtener referencia al GolfStick
        golfStick = FindObjectOfType<GolfStick>();

        // Configurar valores iniciales de los sliders
        if (golfStick != null)
        {
            forceSlider.value = golfStick.GetForce();
            angleSlider.value = golfStick.GetAngle();
        }

        // Agregar listeners a los sliders
        forceSlider.onValueChanged.AddListener(OnForceChanged);
        angleSlider.onValueChanged.AddListener(OnAngleChanged);

        // Configurar botones
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClicked);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        // Mostrar panel inicial
        ShowPreGamePanel();
    }

    void Update()
    {
        // Actualizar textos de fuerza y angulo en tiempo real
        UpdateForceText(forceSlider.value);
        UpdateAngleText(angleSlider.value);

        // Actualizar intentos en el panel pre-juego
        if (preGamePanel != null && preGamePanel.activeSelf && GameManager.Instance != null)
        {
            UpdatePreGameAttemptsText();
        }
    }

    #region Actualizacion de UI
    private void UpdateForceText(float value)
    {
        if (forceText != null)
        {
            forceText.text = $"Fuerza: {value:F1}";
        }
    }

    private void UpdateAngleText(float value)
    {
        if (angleText != null)
        {
            angleText.text = $"Ángulo: {value:F1}°";
        }
    }

    private void UpdatePreGameAttemptsText()
    {
        if (preGameAttemptsText != null && GameManager.Instance != null)
        {
            int attempts = GameManager.Instance.GetAttempts();
            int maxAttempts = GameManager.Instance.GetMaxAttempts();
            preGameAttemptsText.text = $"Intentos: {attempts}/{maxAttempts}";
        }
    }

    public void UpdateAttemptsUI(int currentAttempts, int maxAttempts)
    {
        // Actualizar texto en panel de juego
        if (playingAttemptsText != null)
        {
            playingAttemptsText.text = $"Intentos: {currentAttempts}/{maxAttempts}";
        }
    }

    private void UpdateVictoryAttemptsText()
    {
        if (victoryAttemptsText != null && GameManager.Instance != null)
        {
            int attempts = GameManager.Instance.GetAttempts();
            victoryAttemptsText.text = $"Has ganado en {attempts} intento(s)!";
        }
    }
    #endregion

    #region Callbacks de Sliders
    private void OnForceChanged(float value)
    {
        if (golfStick != null)
        {
            golfStick.SetForce(value);
        }
    }

    private void OnAngleChanged(float value)
    {
        if (golfStick != null)
        {
            golfStick.SetAngle(value);
        }
    }
    #endregion

    #region Callbacks de Botones
    private void OnRetryButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryLevel();
        }
    }

    private void OnPlayAgainButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    #endregion

    #region Gestion de Paneles
    private void HideAllPanels()
    {
        if (preGamePanel != null) preGamePanel.SetActive(false);
        if (playingPanel != null) playingPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowPreGamePanel()
    {
        HideAllPanels();
        if (preGamePanel != null)
        {
            preGamePanel.SetActive(true);
            // Solo actualizar si GameManager ya está inicializado
            if (GameManager.Instance != null)
            {
                UpdatePreGameAttemptsText();
            }
        }
    }

    private void ShowPlayingPanel()
    {
        HideAllPanels();
        if (playingPanel != null)
        {
            playingPanel.SetActive(true);
        }
    }

    public void ShowVictoryPanel()
    {
        HideAllPanels();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            UpdateVictoryAttemptsText();
        }
    }

    public void ShowGameOverPanel()
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    #endregion

    #region Eventos Publicos (llamados por GameManager)
    // Se llama cuando el juego comienza (pelota golpeada)
    public void OnGameStart()
    {
        ShowPlayingPanel();
    }

    // Se llama cuando el jugador reintenta
    public void OnRetry()
    {
        ShowPreGamePanel();
    }

    // Se llama cuando el juego se reinicia completamente
    public void OnGameRestart()
    {
        ShowPreGamePanel();
    }
    #endregion
}