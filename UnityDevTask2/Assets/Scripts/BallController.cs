using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform bounceMarker;
    [SerializeField] float pitchY = 0f;

    [SerializeField] TrailRenderer trail; // Indication for ball path

    [Header("Movement")]
    [SerializeField] float forwardSpeed = 18f;
    [SerializeField] float bounceForce = 6f;
    [SerializeField] float swingStrength = 4f;
    [SerializeField] float spinTurnAngle = 25f;

    // Components & Local Variables
    Rigidbody rb;
    Vector3 startPosition;

    float airProgress = 0f;
    float totalAirTime;

    // Swing path control (REQUIRED FIX)
    Vector3 swingStartPos;
    Vector3 swingEndPos;
    Vector3 swingControlPos;

    // States
    bool hasBounced = false;
    bool isBowling = false;

    // Data from GameManager
    GameManager.BowlingMode mode;
    float power;
    int sideMultiplier;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        // stop ball from falling
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    // Called by GameManager
    public void StartBowling(
        GameManager.BowlingMode mode,
        float power,
        int sideMultiplier
    )
    {
        this.mode = mode;
        this.power = power;
        this.sideMultiplier = sideMultiplier;

        hasBounced = false;
        isBowling = true;

        rb.isKinematic = false;   // enable physics
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // aim towards bounce marker
        Vector3 toTarget = bounceMarker.position - transform.position;

        Vector3 horizontal = new Vector3(toTarget.x, 0f, toTarget.z);
        float horizontalDistance = horizontal.magnitude;

        float time = horizontalDistance / forwardSpeed;
        float verticalVelocity =
            (pitchY - transform.position.y -
            0.5f * Physics.gravity.y * time * time) / time;

        Vector3 velocity =
            horizontal.normalized * forwardSpeed +
            Vector3.up * verticalVelocity;

        totalAirTime = time;
        airProgress = 0f;

        rb.velocity = velocity;

        // ball swing (fixed! it make sure it land on marker)
        if (mode == GameManager.BowlingMode.Swing)
        {
            swingStartPos = transform.position;
            swingEndPos = bounceMarker.position;

            Vector3 sideDir = Vector3.right * sideMultiplier;
            swingControlPos =
                (swingStartPos + swingEndPos) * 0.5f +
                sideDir * swingStrength * power;
        }
    }

    void FixedUpdate()
    {
        if (!isBowling || hasBounced || mode != GameManager.BowlingMode.Swing)
            return;

        airProgress += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(airProgress / totalAirTime);

        // Bezier Curve
        Vector3 pos =
            (1 - t) * (1 - t) * swingStartPos +
            2 * (1 - t) * t * swingControlPos +
            t * t * swingEndPos;

        transform.position = pos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Pitch"))
        {
            hasBounced = true;

            // IMPULSE after bouncing
            Vector3 v = rb.velocity;
            rb.velocity = new Vector3(v.x, 0f, v.z);   // remove downward speed
            //rb.AddForce(Vector3.up * bounceForce * power, ForceMode.Impulse);
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse); // not depends on swing/spin power

            if (mode == GameManager.BowlingMode.Spin)
                ApplySpinAtBounce();
        }

        // Reset everything after ball delivered or hit on ground
        if (collision.gameObject.CompareTag("Wicket") || collision.gameObject.CompareTag("Ground"))
        {
            EndDelivery();
        }
    }

    void ApplySpinAtBounce()
    {
        Vector3 velocity = rb.velocity;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float angle = spinTurnAngle * power * sideMultiplier;

        Vector3 newDir = Quaternion.AngleAxis(angle, Vector3.up) * horizontal.normalized;
        rb.velocity = newDir * horizontal.magnitude;
    }

    void EndDelivery()
    {
        isBowling = false;
        rb.velocity = Vector3.zero;

        gameManager.ResetGame();
    }

    public void ResetBall()
    {
        isBowling = false;
        hasBounced = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        transform.position = startPosition;
        trail.Clear();
    }
}
