using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    //[SerializeField] private GameObject mainPanel;
    //[SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject gamePanel;
    //[SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private Slider force;
    [SerializeField] private Slider angle;

    [SerializeField] private Button start;
    [SerializeField] private Button restart;

    // Referencias a los textos para mostrar los valores
    [SerializeField] private TextMeshProUGUI forceText; 
    [SerializeField] private TextMeshProUGUI angleText;

    private GolfStick golfStick;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Obtener referencia al GolfStick
        golfStick = FindObjectOfType<GolfStick>();

        // Configurar los valores iniciales de los sliders desde GolfStick
        if (golfStick != null)
        {
            force.value = golfStick.GetForce();
            angle.value = golfStick.GetAngle();
        }

        // Agregar listeners
        force.onValueChanged.AddListener(OnForceChanged);
        angle.onValueChanged.AddListener(OnAngleChanged);
    }


    private void OnForceChanged(float value)
    {
        FindObjectOfType<GolfStick>().SetForce(value);
    }

    private void OnAngleChanged(float value)
    {
        FindObjectOfType<GolfStick>().SetAngle(value);
    }

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

// Update is called once per frame
void Update()
    {
        // Actualizar textos
        UpdateForceText(force.value);
        UpdateAngleText(angle.value);
    }
}
