using UnityEngine;
using TMPro;

public class CountdownTimerUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;

    [Header("Timer")]
    public float startSeconds = 120f;
    public bool autoStart = true;

    [Header("Game Over On Time Up")]
    public NightsGameManager gameManager;

    float remaining;
    bool running;
    bool fired;

    void Awake()
    {
        remaining = startSeconds;
        if (!timerText) timerText = GetComponent<TMP_Text>();
        UpdateUI();
        if (autoStart) running = true;
    }

    void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            running = false;

            if (!fired)
            {
                fired = true;
                if (!gameManager) gameManager = FindObjectOfType<NightsGameManager>();
                if (gameManager) gameManager.TimeUpGameOver();
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int total = Mathf.CeilToInt(remaining);
        int minutes = total / 60;
        int seconds = total % 60;
        if (timerText) timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer() => running = true;
    public void StopTimer() => running = false;
    public void ResetTimer() { remaining = startSeconds; fired = false; UpdateUI(); }
}
