using UnityEngine;

public class MeterBar : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 300f;
    [SerializeField] float minY = -275;
    [SerializeField] float maxY = 275;

    // local variables
    private RectTransform bar;
    private Vector2 startBarPos;

    public bool isLocked = false;

    float direction = 1f;

    private void Start()
    {
        bar = gameObject.GetComponent<RectTransform>();
        startBarPos = bar.anchoredPosition; 
    }

    void Update()
    {
        if (isLocked) return;

        Vector2 pos = bar.anchoredPosition;
        pos.y += speed * direction * Time.deltaTime;

        if (pos.y >= maxY)
        {
            pos.y = maxY;
            direction = -1f;
        }
        else if (pos.y <= minY)
        {
            pos.y = minY;
            direction = 1f;
        }

        bar.anchoredPosition = pos;
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void ResetMeter()
    {
        isLocked = false;
        bar.anchoredPosition = startBarPos;
        direction = 1f;
    }

    // 0 is 0% and 1 is 100%
    public float GetPower()
    {
        float distanceFromCenter = Mathf.Abs(bar.anchoredPosition.y);
        float maxDistance = Mathf.Max(Mathf.Abs(minY), Mathf.Abs(maxY));

        float power = 1f - Mathf.Clamp01(distanceFromCenter / maxDistance);
        return power;

    }
}
