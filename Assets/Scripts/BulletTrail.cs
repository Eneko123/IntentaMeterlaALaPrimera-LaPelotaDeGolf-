using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [SerializeField] private float stelaDuration = 0.5f;
    [SerializeField] private float startWidth = 0.5f;
    [SerializeField] private float endWidth = 0.01f;

    [SerializeField] private Color initialColor;
    [SerializeField] private Color finalColor;



    private TrailRenderer trailRenderer;

    void Awake()
    {
        // Añadir TrailRenderer si no existe
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        ConfigureTrail();
    }

    void ConfigureTrail()
    {
        trailRenderer.time = stelaDuration; // Duracion de la estela
        trailRenderer.startWidth = startWidth; // Tamanio de la estela al inicio
        trailRenderer.endWidth = endWidth; // Tamanio de la estela al final

        // Material y color
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = initialColor;
        trailRenderer.endColor = finalColor;

        // Suavizado
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
    }
}