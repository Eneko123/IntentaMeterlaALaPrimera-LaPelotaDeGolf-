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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GiveImpulseToBall(_ball);
            _ball.DeleteTrajectory();
        }
    }

    private Vector2 ConvertAngleToVector(float angle, Vector2 baseVector)
    {
        Vector2 vector = Quaternion.Euler(0, 0, angle) * baseVector;

        return vector;
    }

    private void GiveImpulseToBall(GolfBall ball)
    {
        ball.rb.AddForce(ConvertAngleToVector(_angle, BASE_VECTOR) * _force, ForceMode.Impulse);
    }

    #region Getters/Setters
    public float GetForce() => _force;
    public float GetAngle() => _angle;
    public Vector2 GetVectorAngle() => ConvertAngleToVector(_angle, BASE_VECTOR);

    public void SetForce(float newForce) => _force = newForce;
    public void SetAngle(float newAngle) => _angle = newAngle;
    #endregion
}
