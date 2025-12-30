using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum BowlingMode // swing or spin
    {
        Swing,
        Spin
    }

    [Header("References")]
    [SerializeField] MeterBar meterBar; // swing/spin power
    [SerializeField] BallController ball;
    [SerializeField] BounceMarkerController bounceMarker; // ball landing spot

    [SerializeField] TrailRenderer trail;
    [SerializeField] Button trailButton;
    [SerializeField] TMP_Text trailButtonText;


    [Header("UI Buttons")]
    [SerializeField] Button swingButton;
    [SerializeField] Button spinButton;
    [SerializeField] Button changeSideButton;
    [SerializeField] Button ballButton;
    [SerializeField] GameObject mainMenuPanel;

    // States
    BowlingMode currentMode = BowlingMode.Swing; // default=swing
    int sideMultiplier = 1; // Off-Spin , Leg-Spin
    bool isTrailEnabled = true;

    public void OnSwingClicked()
    {
        currentMode = BowlingMode.Swing;

        swingButton.interactable = false;
        spinButton.interactable = true;
    }

    public void OnSpinClicked()
    {
        currentMode = BowlingMode.Spin;

        spinButton.interactable = false;
        swingButton.interactable = true;
    }

    public void OnChangeSideClicked()
    {
        sideMultiplier *= -1; // switch to opposite sides

        // Change pos on Canvas
        RectTransform rt = changeSideButton.GetComponent<RectTransform>();
        float xPos = Mathf.Abs(rt.anchoredPosition.x);
        rt.anchoredPosition = new Vector2(xPos * sideMultiplier, rt.anchoredPosition.y); 
    }

    public void OnBowlClicked()
    {
        meterBar.Lock();
        bounceMarker.Lock();
        DisableAllButtons();

        float power = meterBar.GetPower();

        // through ball
        ball.StartBowling(
            currentMode, // spin/swing
            power, // intensity of throughing force
            sideMultiplier // bowling side (left/right)
        );

    }



    void DisableAllButtons()
    {
        swingButton.interactable = false;
        spinButton.interactable = false;
        changeSideButton.interactable = false;
        ballButton.interactable = false;
    }

    public void ResetGame()
    {
        meterBar.ResetMeter();
        bounceMarker.ResetMarker();
        ball.ResetBall();

        // enable swing/spin (last selected)
        if(currentMode == BowlingMode.Swing)
        {
            swingButton.interactable = false;
            spinButton.interactable = true;
        }
        else
        {
            swingButton.interactable = true;
            spinButton.interactable = false;
        }

        changeSideButton.interactable = true;
        ballButton.interactable = true;
    }

    public void OnPlayClicked()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1;
    }

    // Exit game
    public void OnQuitClicked()
    {
        Application.Quit();
    }


    public void OnTrailBtnClicked()
    {
        isTrailEnabled = !isTrailEnabled;

        trail.emitting = isTrailEnabled;

        trailButtonText.text = isTrailEnabled
            ? "Trail: Enabled"
            : "Trail: Disabled";
    }
}
