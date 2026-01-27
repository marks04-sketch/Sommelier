using System.Collections;
using UnityEngine;

public class PantingPlayer : MonoBehaviour
{
    public AudioSource source;
    public AudioClip panting;
    public float intervalSeconds = 5f;
    public bool playDuringTimeScaleZero = false;

    Coroutine _co;

    void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
        if (_co == null) _co = StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            if (panting && source && (playDuringTimeScaleZero || Time.timeScale > 0f))
                source.PlayOneShot(panting);

            yield return new WaitForSecondsRealtime(intervalSeconds);
        }
    }
}
