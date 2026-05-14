using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    internal Rigidbody rb;

    [SerializeField] private GolfStick _golfStick;

    [SerializeField] private float _NormalGroundDesaceleration;
    [SerializeField] private float _SlowGroundDesaceleration;
    [SerializeField] private float _FastGroundAceleration;
    [SerializeField] private float _impulse;
    [SerializeField] private float _Fly;

    internal LineRenderer _line;
    public int resolution = 30;
    public float timeStep = 0.1f;
    public Transform launchPoint;
    public Vector2 launchVelocity;

    private bool onCol;
    private bool onColWithSlowGround;
    private bool onColWithFastGround;
    private bool onColWithImpulseGroundX;
    private bool onColWithImpulseGroundY;
    private bool onColWithFlyArea;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasSpacePress())
        {
            DrawTrajectory();
        }
    }

    private bool HasSpacePress()
    {
        bool pressSpace;

        if (Input.GetKey(KeyCode.Space))
        {
            pressSpace = true;
        }
        pressSpace = false;

        return pressSpace;
    }

    void DrawTrajectory()
    {
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

    private void StopDrawingLine()
    {
        // no funciona
        _line.positionCount = 0;
    }

    public void DeleteTrajectory()
    {
        _line.enabled = false;
    }

    private Vector3 FloatToVector3X(float desacelarationFloat)
    {
        return new Vector3(desacelarationFloat, 0, 0);
    }
    private Vector3 FloatToVector3Y(float desacelarationFloat)
    {
        return new Vector3(0, desacelarationFloat, 0);
    }

    private void NormalDesaceleration(Vector3 desaceleration)
    {
        if (onCol && rb.linearVelocity.magnitude > 0.005)
        {
            rb.linearVelocity -= rb.linearVelocity * desaceleration.x;
        }
    }

    private void SlowGroundDesaceleration(Vector3 desaceleration)
    {
        if (onColWithSlowGround && rb.linearVelocity.magnitude > 0.005)
        {
            rb.linearVelocity -= rb.linearVelocity * desaceleration.x * 2;
        }
    }

    private void FastGroundAceleration(Vector3 aceleration)
    {
        if (onColWithFastGround && rb.linearVelocity.magnitude > 0.005)
        {
            rb.linearVelocity += rb.linearVelocity * aceleration.x;
        }
    }
    private void ImpulseGroundX(Vector3 aceleration)
    {
        if (onColWithImpulseGroundX && rb.linearVelocity.magnitude > 0.005)
        {
            rb.AddForce(Vector2.right * _impulse, ForceMode.Impulse);
        }
    }

    private void ImpulseGroundY(Vector3 aceleration)
    {
        if (onColWithImpulseGroundY && rb.linearVelocity.magnitude > 0.005)
        {
            rb.AddForce(Vector2.up * _impulse, ForceMode.Impulse);
        }
    }

    private void UpAceleration(Vector3 aceleration)
    {
        if (onColWithFlyArea && rb.linearVelocity.magnitude > 0.005)
        {
            rb.AddForce(Vector2.up * _Fly);
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            onCol = true;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "SlowGround")
        {
            onColWithSlowGround = true;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "FastGround")
        {
            onColWithFastGround = true;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "ImpulseGroundX")
        {
            onColWithImpulseGroundX = true;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "ImpulseGroundY")
        {
            onColWithImpulseGroundY = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Fly")
        {
            onColWithFlyArea = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            onCol = false;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "SlowGround")
        {
            onColWithSlowGround = false;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "FastGround")
        {
            onColWithFastGround = false;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "ImpulseGroundY")
        {
            onColWithImpulseGroundX = false;
        }
        if (LayerMask.LayerToName(collision.gameObject.layer) == "ImpulseGroundY")
        {
            onColWithImpulseGroundY = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Fly")
        {
            onColWithFlyArea = false;
        }
    }

    private void FixedUpdate()
    {
        NormalDesaceleration(FloatToVector3X(_NormalGroundDesaceleration));
        SlowGroundDesaceleration(FloatToVector3X(_SlowGroundDesaceleration));
        FastGroundAceleration(FloatToVector3X(_FastGroundAceleration));
        ImpulseGroundX(FloatToVector3X(_impulse));
        ImpulseGroundY(FloatToVector3Y(_impulse));
        UpAceleration(FloatToVector3Y(_Fly));
    }
}
