using UnityEngine;
using static GameManager;

public class BallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform bounceMarker;
    [SerializeField] float pitchY = 0f;
    [SerializeField] TrailRenderer trail;

    [Header("Movement")]
    [SerializeField] float forwardSpeed = 18f;
    [SerializeField] float bounceForce = 6f;
    [SerializeField] float swingStrength = 4f;
    [SerializeField] float spinTurnAngle = 25f;

    Rigidbody rb;
    Vector3 startPosition;

    float airProgress;
    float totalAirTime;

    // Swing path
    Vector3 swingStartPos;
    Vector3 swingEndPos;
    Vector3 swingControlPos;

    // Swing direction tracking
    Vector3 lastPos;
    Vector3 currentDir;

    bool hasBounced;
    bool isBowling;

    BowlingMode mode;
    float power;
    int sideMultiplier;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void StartBowling(BowlingMode mode, float power, int sideMultiplier)
    {
        this.mode = mode;
        this.power = power;
        this.sideMultiplier = sideMultiplier;

        hasBounced = false;
        isBowling = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 toTarget = bounceMarker.position - transform.position;
        Vector3 horizontal = new Vector3(toTarget.x, 0f, toTarget.z);

        float horizontalDistance = horizontal.magnitude;
        float time = horizontalDistance / forwardSpeed;

        float verticalVelocity =
            (pitchY - transform.position.y -
            0.5f * Physics.gravity.y * time * time) / time;

        rb.velocity =
            horizontal.normalized * forwardSpeed +
            Vector3.up * verticalVelocity;

        totalAirTime = time;
        airProgress = 0f;

        // -------- SWING SETUP ONLY --------
        if (mode == BowlingMode.Swing)
        {
            swingStartPos = transform.position;
            swingEndPos = bounceMarker.position;

            Vector3 sideDir = Vector3.right * sideMultiplier;
            swingControlPos =
                (swingStartPos + swingEndPos) * 0.5f +
                sideDir * swingStrength * power;

            lastPos = swingStartPos;
            currentDir = horizontal.normalized;
        }
    }

    void FixedUpdate()
    {
        // -------- SWING MOVEMENT ONLY --------
        if (!isBowling || hasBounced || mode != BowlingMode.Swing)
            return;

        airProgress += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(airProgress / totalAirTime);

        Vector3 pos =
            (1 - t) * (1 - t) * swingStartPos +
            2 * (1 - t) * t * swingControlPos +
            t * t * swingEndPos;

        Vector3 delta = pos - lastPos;
        if (delta.sqrMagnitude > 0.0001f)
            currentDir = delta.normalized;

        lastPos = pos;
        transform.position = pos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Pitch"))
        {
            hasBounced = true;

            // -------- SWING BOUNCE --------
            if (mode == BowlingMode.Swing)
            {
                Vector3 horizontalDir = new Vector3(
                    currentDir.x,
                    0f,
                    currentDir.z
                ).normalized;

                rb.velocity = horizontalDir * forwardSpeed;
                rb.velocity += Vector3.up * bounceForce;
            }

            // -------- SPIN BOUNCE --------
            if (mode == BowlingMode.Spin)
            {
                Vector3 v = rb.velocity;
                rb.velocity = new Vector3(v.x, 0f, v.z);
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                ApplySpinAtBounce(true); // YOUR LOGIC
            }
        }

        if (collision.gameObject.CompareTag("Wicket") ||
            collision.gameObject.CompareTag("Ground"))
        {
            EndDelivery();
        }
    }

    // -------- YOUR ORIGINAL SPIN LOGIC (UNCHANGED) --------
    void ApplySpinAtBounce(bool spin)
    {
        int turnDir = spin ? sideMultiplier : -sideMultiplier;
        float turnAngle = spinTurnAngle;

        Vector3 velocity = rb.velocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        float angle = turnAngle * power * turnDir;

        Vector3 newDir =
            Quaternion.AngleAxis(angle, Vector3.up) * horizontal.normalized;

        rb.velocity = newDir * horizontal.magnitude + Vector3.up * velocity.y;
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
