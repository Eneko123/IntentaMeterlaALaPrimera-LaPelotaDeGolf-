using UnityEngine;

// Componente que define un area con gravedad invertida.
public class InvertedGravityArea : MonoBehaviour
{
    [Header("Configuración")]
    // Multiplicador de la gravedad invertida (2 = gravedad normal hacia arriba
    [SerializeField] private float gravityMultiplier = 2f;

    private void Start()
    {
        // Verificar que tiene un trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"InvertedGravityArea en {gameObject.name} necesita un Collider!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"InvertedGravityArea en {gameObject.name}: El Collider debe ser Trigger!");
            col.isTrigger = true;
        }

        // Verificar que está en la layer correcta
        if (LayerMask.LayerToName(gameObject.layer) != "InvertedGravity")
        {
            Debug.LogWarning($"InvertedGravityArea en {gameObject.name} debe estar en la Layer 'InvertedGravity'!");
        }
    }

    public float GetGravityMultiplier()
    {
        return gravityMultiplier;
    }
}