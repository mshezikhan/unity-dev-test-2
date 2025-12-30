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
    ){
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

        rb.velocity = velocity;

    }


    void FixedUpdate()
    {
        if (!isBowling) return;

        // swing in air
        if (mode == GameManager.BowlingMode.Swing && !hasBounced)
        {
            Vector3 sideForce = Vector3.right * sideMultiplier * swingStrength * power;
            rb.velocity += sideForce * Time.fixedDeltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Pitch"))
        {
            hasBounced = true;

            // bounce ball on pitch hit
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

            if (mode == GameManager.BowlingMode.Spin)
            {
                ApplySpinAtBounce();
            }
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
