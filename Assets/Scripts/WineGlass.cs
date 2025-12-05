using UnityEngine;

public class WineGlass : MonoBehaviour
{
    [Header("Setup")]
    public string displayName = "Drink";
    public AudioClip sipSfx;
    [Range(0, 1)] public float sipVolume = 0.8f;

    [HideInInspector] public bool isCorrect;   // set by manager

    AudioSource _audio;

    void Awake() { _audio = GetComponent<AudioSource>(); }

    // Called by the player interactor when you press the key
    public void Drink()
    {
        if (_audio && sipSfx) _audio.PlayOneShot(sipSfx, sipVolume);
        // Let the manager know (manager will find us this frame)
        SimpleNightManager.OnAnyGlassDrank?.Invoke(this);
    }
}
