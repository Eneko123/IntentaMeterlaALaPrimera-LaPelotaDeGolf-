using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Animations;

public class GolfStick : MonoBehaviour
{
    [SerializeField] private float _force = 10f;
    [SerializeField] private float _angle = 45f;

    [SerializeField] private GolfBall _ball;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GiveImpulseToBall(_ball);
        }
    }

    private Vector2 ConvertAngleToVector(float angle)
    {
        Vector2 vector;
        vector.x = Mathf.Cos(angle);
        vector.y = Mathf.Sin(angle);
        return vector;
    }

    private void GiveImpulseToBall(GolfBall ball)
    {
        ball.rb.AddForce(ConvertAngleToVector(_angle) * _force, ForceMode.Impulse);
    }

    #region Getters/Setters
    public float GetForce() => _force;
    public float GetAngle() => _angle;
    public Vector2 GetVectorAngle() => ConvertAngleToVector(_angle);

    public void SetForce(float newForce) => _force = newForce;
    public void SetAngle(float newAngle) => _angle = newAngle;
    #endregion
}
