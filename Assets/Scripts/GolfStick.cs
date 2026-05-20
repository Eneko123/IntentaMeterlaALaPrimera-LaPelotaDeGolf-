using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Animations;

public class GolfStick : MonoBehaviour
{
    [SerializeField] private float _force = 10f;
    [SerializeField] private float _angle = 45f;

    [SerializeField] private GolfBall _ball;

    private static Vector2 BASE_VECTOR = Vector2.right;

    private void Update()
    {
        // Solo permitir golpear si estamos en estado PreGame
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.GetCurrentState() == GameManager.GameState.PreGame)
            {
                GiveImpulseToBall(_ball);
                _ball.DeleteTrajectory();

                // Notificar al GameManager que la pelota fue golpeada
                GameManager.Instance.OnBallHit();
            }
        }
    }

    private Vector2 ConvertAngleToVector(float angle, Vector2 baseVector)
    {
        Vector2 vector = Quaternion.Euler(0, 0, angle) * baseVector;

        return vector;
    }

    private void GiveImpulseToBall(GolfBall ball)
    {
        if (ball != null && ball.rb != null)
        {
            ball.rb.AddForce(ConvertAngleToVector(_angle, BASE_VECTOR) * _force, ForceMode.Impulse);
        }
    }

    #region Getters/Setters
    public float GetForce() => _force;
    public float GetAngle() => _angle;
    public Vector2 GetVectorAngle() => ConvertAngleToVector(_angle, BASE_VECTOR);

    public void SetForce(float newForce) => _force = newForce;
    public void SetAngle(float newAngle) => _angle = newAngle;
    #endregion
}
