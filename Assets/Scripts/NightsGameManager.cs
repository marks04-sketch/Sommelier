using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NightsGameManager : MonoBehaviour
{
    [Header("Config")]
    public int startNight = 1;
    public int maxNight = 4;
    public float nextNightDelay = 0.6f;

    [Header("Refs")]
    public WineGlass[] bottles;
    public TMP_Text nightText;
    public TMP_Text resultText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject crosshairUI;

    [Header("Night Intro")]
    public GameObject nightIntroPanel;
    public TMP_Text nightIntroText;
    public float nightIntroDuration = 3f;

    [Header("Screen Shake")]
    public Transform cameraToShake;
    public float shakeDuration = 0.35f;
    public float shakeMagnitude = 0.10f;

    public float failShakeDuration = 0.35f;
    public float failShakeMagnitude = 0.18f;
    public float failDelayBeforePanel = 0.12f;

    [System.Serializable]
    public class LightDelayEntry
    {
        public Light lightToTurnOff;
        public float lightOffSeconds = 5f;
        public float lightReturnIntensity = 1500f;
    }

    [Header("Table Lights Delay")]
    public List<LightDelayEntry> lightsDelay = new List<LightDelayEntry>();

    [Header("Panting")]
    public AudioSource pantingSource;
    public AudioClip pantingClip;
    public float pantingStartDelay = 0f;
    public List<float> pantingIntervals = new List<float> { 15f, 13f, 11f, 9f, 7f, 5f, 3f };

    [Header("Correct bottle")]
    [Tooltip("0=Elemento0, 1=Elemento1, 2=Elemento2 (según array bottles)")]
    public int correctBottleIndex = 0;
    public bool randomizeCorrectEachNight = true;
    public bool lockWhileResolving = true;

    [System.Serializable]
    public class NightPalette
    {
        public Material correct;
        public List<Material> incorrect;
    }

    [Header("Materiales por noche (paquetes)")]
    public List<NightPalette> palettes = new List<NightPalette>();

    [Header("Comportamiento UI")]
    public bool hideResultTextOnStart = true;
    public bool hideResultTextWhenEmpty = true;

    static int s_nextNight = 0;

    int _night;
    bool _ended;
    bool _resolving;
    Coroutine _pantingCo;

    void Awake()
    {
        Time.timeScale = 1f;

        _night = (s_nextNight > 0) ? s_nextNight : startNight;
        s_nextNight = 0;

        if (hideResultTextOnStart && resultText)
        {
            resultText.text = "";
            if (hideResultTextWhenEmpty) resultText.gameObject.SetActive(false);
        }

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);

        if (!cameraToShake && Camera.main) cameraToShake = Camera.main.transform;

        if (nightIntroPanel && nightIntroText && nightIntroDuration > 0f)
        {
            if (nightText) nightText.gameObject.SetActive(false);
            if (crosshairUI) crosshairUI.SetActive(false);

            nightIntroPanel.SetActive(true);
            nightIntroText.text = $"Night {_night}";

            Time.timeScale = 0f;
            StartCoroutine(NightIntroRoutine());
        }
        else
        {
            if (crosshairUI) crosshairUI.SetActive(true);
            StartNight();
        }
    }

    IEnumerator NightIntroRoutine()
    {
        yield return new WaitForSecondsRealtime(nightIntroDuration);

        if (nightIntroPanel) nightIntroPanel.SetActive(false);

        Time.timeScale = 1f;

        if (nightText) nightText.gameObject.SetActive(true);
        if (crosshairUI) crosshairUI.SetActive(true);

        StartNight();
    }

    void StartNight()
    {
        if (lightsDelay != null && lightsDelay.Count > 0)
        {
            for (int i = 0; i < lightsDelay.Count; i++)
            {
                var e = lightsDelay[i];
                if (e != null && e.lightToTurnOff && e.lightOffSeconds > 0f)
                    StartCoroutine(LightDelay(e));
            }
        }

        StartPanting();

        RefreshNightUI();
        SetupCorrectBottleForNight(_night);
        ApplyPaletteForNight(_night);
    }

    void StartPanting()
    {
        StopPanting();
        if (!pantingSource || !pantingClip) return;
        _pantingCo = StartCoroutine(PantingRoutine());
    }

    void StopPanting()
    {
        if (_pantingCo != null)
        {
            StopCoroutine(_pantingCo);
            _pantingCo = null;
        }
    }

    IEnumerator PantingRoutine()
    {
        if (pantingStartDelay > 0f)
            yield return new WaitForSecondsRealtime(pantingStartDelay);

        if (pantingSource && pantingClip)
            pantingSource.PlayOneShot(pantingClip);

        int idx = 0;
        float currentInterval = (pantingIntervals != null && pantingIntervals.Count > 0) ? pantingIntervals[0] : 15f;

        while (!_ended)
        {
            if (_resolving) yield break;

            yield return new WaitForSecondsRealtime(currentInterval);

            if (_ended || _resolving) yield break;

            if (pantingSource && pantingClip)
                pantingSource.PlayOneShot(pantingClip);

            if (pantingIntervals != null && pantingIntervals.Count > 0 && idx < pantingIntervals.Count - 1)
            {
                idx++;
                currentInterval = pantingIntervals[idx];
            }
        }
    }

    IEnumerator LightDelay(LightDelayEntry e)
    {
        float prevIntensity = e.lightToTurnOff.intensity;
        bool prevEnabled = e.lightToTurnOff.enabled;

        e.lightToTurnOff.enabled = true;
        e.lightToTurnOff.intensity = 0f;

        yield return new WaitForSecondsRealtime(e.lightOffSeconds);

        e.lightToTurnOff.enabled = prevEnabled;
        e.lightToTurnOff.intensity = e.lightReturnIntensity;
    }

    void OnEnable() => WineGlass.OnAnyGlassDrank += OnGlassDrank;
    void OnDisable() => WineGlass.OnAnyGlassDrank -= OnGlassDrank;

    void OnGlassDrank(WineGlass glass)
    {
        if (_ended) return;
        if (lockWhileResolving && _resolving) return;
        if (!glass) return;

        int idx = IndexOfBottle(glass);
        bool isCorrect = (idx == correctBottleIndex);
        glass.isCorrect = isCorrect;

        if (isCorrect) StartCoroutine(HandleCorrect());
        else StartCoroutine(HandleWrong());
    }

    int IndexOfBottle(WineGlass g)
    {
        if (bottles == null) return -1;
        for (int i = 0; i < bottles.Length; i++)
            if (bottles[i] == g) return i;
        return -1;
    }

    IEnumerator HandleCorrect()
    {
        _resolving = true;
        StopPanting();

        yield return ScreenShake(shakeDuration, shakeMagnitude);

        yield return new WaitForSecondsRealtime(nextNightDelay);

        int next = _night + 1;

        if (next > maxNight)
        {
            EndGame(true);
            yield break;
        }

        s_nextNight = next;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    IEnumerator HandleWrong()
    {
        _resolving = true;
        StopPanting();

        yield return ScreenShake(failShakeDuration, failShakeMagnitude);
        yield return new WaitForSecondsRealtime(failDelayBeforePanel);

        EndGame(false);
    }

    IEnumerator ScreenShake(float duration, float magnitude)
    {
        if (!cameraToShake) yield break;

        Vector3 originalPos = cameraToShake.localPosition;
        float t = 0f;

        while (t < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraToShake.localPosition = originalPos + new Vector3(x, y, 0f);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cameraToShake.localPosition = originalPos;
    }

    void RefreshNightUI()
    {
        if (nightText) nightText.text = $"Night {_night}/{maxNight}";
    }

    void SetupCorrectBottleForNight(int night)
    {
        if (bottles == null || bottles.Length == 0) return;

        int idx = randomizeCorrectEachNight ? Random.Range(0, bottles.Length) : Mathf.Clamp(correctBottleIndex, 0, bottles.Length - 1);
        correctBottleIndex = idx;

        for (int i = 0; i < bottles.Length; i++)
        {
            if (!bottles[i]) continue;
            bottles[i].isCorrect = (i == correctBottleIndex);
        }
    }

    void ApplyPaletteForNight(int night)
    {
        if (palettes == null || palettes.Count == 0) return;
        if (bottles == null || bottles.Length == 0) return;

        int paletteIndex = Mathf.Clamp(night - 1, 0, palettes.Count - 1);
        NightPalette pal = palettes[paletteIndex];
        if (pal == null) return;

        List<Material> incorrect = pal.incorrect ?? new List<Material>();
        if (incorrect.Count == 0) return;

        List<Material> bag = new List<Material>(incorrect);
        Shuffle(bag);

        int wrongPick = 0;

        for (int i = 0; i < bottles.Length; i++)
        {
            var glass = bottles[i];
            if (!glass) continue;

            Renderer r = FindLiquidRenderer(glass);
            if (!r) continue;

            if (i == correctBottleIndex)
            {
                if (pal.correct != null) r.sharedMaterial = pal.correct;
            }
            else
            {
                Material m = bag[wrongPick % bag.Count];
                wrongPick++;
                if (m != null) r.sharedMaterial = m;
            }
        }
    }

    Renderer FindLiquidRenderer(WineGlass glass)
    {
        if (!glass) return null;

        Transform root = (glass.transform.parent != null) ? glass.transform.parent : glass.transform;

        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (!r) continue;

            if (r.gameObject.name.Contains("WineGlass_Inside"))
                return r;

            if (r.sharedMaterial != null && r.sharedMaterial.name.StartsWith("WineInside"))
                return r;
        }

        return null;
    }

    void EndGame(bool win)
    {
        _ended = true;
        _resolving = true;
        StopPanting();

        ShowResult(win ? "You Win!" : "Game Over");

        if (gameOverPanel) gameOverPanel.SetActive(!win);
        if (winPanel) winPanel.SetActive(win);
        if (crosshairUI) crosshairUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    void ShowResult(string msg)
    {
        if (!resultText) return;

        if (string.IsNullOrEmpty(msg))
        {
            resultText.text = "";
            if (hideResultTextWhenEmpty) resultText.gameObject.SetActive(false);
        }
        else
        {
            resultText.gameObject.SetActive(true);
            resultText.text = msg;
        }
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void RestartFromUI()
    {
        Time.timeScale = 1f;
        s_nextNight = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void QuitFromUI()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void TimeUpGameOver()
    {
        if (_ended) return;
        if (_resolving) return;
        _resolving = true;
        StopPanting();
        EndGame(false);
    }
}
