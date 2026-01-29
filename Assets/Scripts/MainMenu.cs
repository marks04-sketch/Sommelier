using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{
    // NEW: bandera para no volver a mostrar menú tras pulsar Start
    static bool s_gameStarted = false;

    [Header("UI")]
    public GameObject menuCanvas;      // tu Canvas del menú (con background + botones)

    [Header("Enable/Disable gameplay on Start")]
    public Behaviour[] gameplayScriptsToEnable; // NightsGameManager, movimiento, etc.
    public GameObject[] gameplayObjectsToEnable; // UI del juego, PlayerRig, etc.

    [Header("Audio")]
    public AudioSource menuMusic; // tu MusicPlayer (AudioSource en loop)

    [Header("Menu Camera")]
    public GameObject menuCamera; // arrastra aquí tu MenuCamera (la que está fuera de GamePlayRoot)

    void Start()
    {
        // NEW: si ya se le dio a Start una vez, NO mostrar menú nunca más (aunque se recargue la escena)
        if (s_gameStarted)
        {
            if (menuCanvas != null) menuCanvas.SetActive(false);
            if (menuCamera != null) menuCamera.SetActive(false);
            SetGameplay(true);
            return;
        }

        // Menú visible
        if (menuCanvas != null) menuCanvas.SetActive(true);

        // Cámara del menú activa (para evitar "No cameras rendering")
        if (menuCamera != null) menuCamera.SetActive(true);

        // Asegurar que NO arranca el juego
        SetGameplay(false);

        // Música en el menú
        if (menuMusic != null && !menuMusic.isPlaying) menuMusic.Play();
    }

    public void OnStartPressed()
    {
        // NEW: marca que ya empezó el juego para futuras recargas de escena
        s_gameStarted = true;

        // Oculta menú y arranca juego
        if (menuCanvas != null) menuCanvas.SetActive(false);

        // Apaga cámara del menú y deja que renderice la Main Camera del PlayerRig
        if (menuCamera != null) menuCamera.SetActive(false);

        SetGameplay(true);
    }

    public void OnLeavePressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    void SetGameplay(bool enabled)
    {
        if (gameplayScriptsToEnable != null)
            foreach (var b in gameplayScriptsToEnable)
                if (b != null) b.enabled = enabled;

        if (gameplayObjectsToEnable != null)
            foreach (var go in gameplayObjectsToEnable)
                if (go != null) go.SetActive(enabled);
    }
}
