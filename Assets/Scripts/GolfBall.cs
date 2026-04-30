using UnityEditor.UIElements;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    internal Rigidbody rb;

    [SerializeField] private GolfStick _golfStick;

    [SerializeField] private float _groundDesaceleration;

    internal LineRenderer _line;
    public int resolution = 30;
    public float timeStep = 0.1f;
    public Transform launchPoint;
    public Vector2 launchVelocity;

    private bool onCol;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        DrawTrajectory();
    }

    void DrawTrajectory()
    {
        Vector3[] points = new Vector3[resolution];
        Vector3 startPos = launchPoint.position;
        Vector3 startVel = (Vector3)_golfStick.GetVectorAngle();

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            points[i] = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
        }

        _line.positionCount = resolution;
        _line.SetPositions(points);
    }

    private Vector3 FloatToVector3(float desacelarationFloat)
    {
        return new Vector3(desacelarationFloat, 0, 0);
    }

    private void Desaceleration(Vector3 desaceleration)
    {
        if (onCol && rb.linearVelocity.magnitude > 0.005)
        {
            rb.linearVelocity -= rb.linearVelocity * desaceleration.x;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            onCol = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            onCol = false;
        }
    }

    private void FixedUpdate()
    {
        Desaceleration(FloatToVector3(_groundDesaceleration));
    }
}
