using System;
using UnityEngine;

public class WineGlass : MonoBehaviour
{
    public static Action<WineGlass> OnAnyGlassDrank;

    [Header("Setup")]
    public string displayName = "Drink";
    public AudioClip sipSfx;
    [Range(0, 1)] public float sipVolume = 0.8f;

    [HideInInspector] public bool isCorrect;   // lo setea el manager

    AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    public void Drink()
    {
        if (_audio && sipSfx) _audio.PlayOneShot(sipSfx, sipVolume);
        OnAnyGlassDrank?.Invoke(this);
    }
}
