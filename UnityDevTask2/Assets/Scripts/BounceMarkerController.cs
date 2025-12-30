using UnityEngine;

public class BounceMarkerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Vector2 straightLimits = new Vector2(-2f, 2f);
    [SerializeField] Vector2 sideLimits = new Vector2(2f, 10f);

    bool isLocked = false;
    Vector3 startPosition;

    void Awake()
    {
        startPosition = transform.position;
        Time.timeScale = 0;
    }

    void Update()
    {
        if (isLocked) return;

        float h = 0f;
        float v = 0f;

        // controls
        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.position += move; // update position

        ClampPosition();
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, straightLimits.x, straightLimits.y);
        pos.z = Mathf.Clamp(pos.z, sideLimits.x, sideLimits.y);
        transform.position = pos;
    }

    public void Lock()
    {
        isLocked = true;
        gameObject.SetActive(false);
    }

    private void Unlock()
    {
        isLocked = false;
        gameObject.SetActive(true);
    }

    public void ResetMarker()
    {
        Unlock();
        transform.position = startPosition;
    }
}
