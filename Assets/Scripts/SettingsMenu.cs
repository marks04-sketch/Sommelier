using UnityEngine;
using UnityEngine.UI;
using Sommelier.Player; // so we can reference MouseLook

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Targets")]
    public MouseLook mouseLook;   // drag your PlayerRig’s MouseLook here (or auto-find)

    void Awake()
    {
        // Auto-find if not assigned
        if (!mouseLook) mouseLook = FindObjectOfType<MouseLook>(true);

        // Wire listeners once
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    void OnEnable()
    {
        // Load saved values into UI when panel opens
        float sens = PlayerPrefs.GetFloat("Sensitivity", 0.12f);
        float vol = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        bool full = Screen.fullScreen;

        // Set UI without re-triggering listeners’ side-effects
        sensitivitySlider.SetValueWithoutNotify(sens);
        volumeSlider.SetValueWithoutNotify(vol);
        fullscreenToggle.SetIsOnWithoutNotify(full);

        // Apply to systems now (in case scene just loaded)
        if (mouseLook) mouseLook.ApplySensitivity(sens);
        AudioListener.volume = vol;
        Screen.fullScreen = full;
    }

    void OnDestroy()
    {
        // good hygiene (optional)
        sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }

    // ---- handlers ----
    void OnSensitivityChanged(float value)
    {
        if (mouseLook) mouseLook.ApplySensitivity(value);
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    void OnFullscreenChanged(bool isFull)
    {
        Screen.fullScreen = isFull;
        // optional: save if you want
        PlayerPrefs.SetInt("Fullscreen", isFull ? 1 : 0);
        PlayerPrefs.Save();
    }
}
