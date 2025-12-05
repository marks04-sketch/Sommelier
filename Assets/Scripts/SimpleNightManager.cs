using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class SimpleNightManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nightLabel;
    public TMP_Text toastLabel;    // short feedback: “Right glass!” / “Poison!”

    [Header("Timing")]
    public int totalNights = 4;
    public float nightTimer = 90f;

    public static Action<WineGlass> OnAnyGlassDrank;

    WineGlass[] glasses;
    int nightIndex;
    int correctIndex;
    float t;
    Coroutine _toastCo;
    IEnumerator ToastRoutine(string msg)
    {
        toastLabel.text = msg;
        toastLabel.alpha = 1f; // TMP_Text has .alpha
        yield return new WaitForSeconds(1.5f);

        // quick fade out
        float t = 0f, dur = 0.4f;
        while (t < dur)
        {
            t += Time.deltaTime;
            toastLabel.alpha = Mathf.Lerp(1f, 0f, t / dur);
            yield return null;
        }
        toastLabel.text = "";
        toastLabel.alpha = 1f; // reset so next message starts visible
    }

    void Awake()
    {
        glasses = FindObjectsOfType<WineGlass>(true);
        OnAnyGlassDrank += HandleDrank;
        StartNight(0);
    }

    void OnDestroy() { OnAnyGlassDrank -= HandleDrank; }

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f) LoseNight("Time’s up");
    }

    void StartNight(int idx)
    {
        nightIndex = idx;
        t = nightTimer;

        // choose the correct glass this night
        correctIndex = Random.Range(0, glasses.Length);
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].isCorrect = (i == correctIndex);

        if (nightLabel) nightLabel.text = $"Night {nightIndex + 1}/{totalNights}";
        Toast($"Find the right glass…");
    }

    void HandleDrank(WineGlass g)
    {
        if (g.isCorrect) WinNight();
        else LoseNight("Poison!");
    }

    void WinNight()
    {
        Toast("Right glass!");
        if (nightIndex + 1 >= totalNights) { Toast("You Won!"); enabled = false; return; }
        StartNight(nightIndex + 1);
    }

    void LoseNight(string why)
    {
        Toast(why);
        // restart same night for now
        StartNight(nightIndex);
    }

    void Toast(string msg)
    {
        if (!toastLabel) { Debug.Log(msg); return; }
        if (_toastCo != null) StopCoroutine(_toastCo);
        _toastCo = StartCoroutine(ToastRoutine(msg));
    }
}
