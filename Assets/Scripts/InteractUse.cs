using UnityEngine;
using TMPro;
using System.Reflection;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InteractUse : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public TMP_Text promptText;          // optional: small text under crosshair

    [Header("Ray")]
    public LayerMask interactMask;       // set to Interactable only
    public float useDistance = 3.0f;

    [Header("Input")]
    public KeyCode useKey = KeyCode.F;   // press F to drink

    void Reset() { cam = Camera.main; }

    void Update()
    {
        if (!cam) cam = Camera.main;

        // Don’t allow while inspecting (if you added that flag)
        if (ObjectInspector_IsInspecting()) { SetPrompt(""); return; }

        // Ray from the center of screen
        Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        WineGlass glass = null;

        if (Physics.Raycast(r, out var hit, useDistance, interactMask, QueryTriggerInteraction.Collide))
            glass = hit.collider.GetComponentInParent<WineGlass>();

        if (glass)
        {
            // Debug when hovering
            Debug.Log("Hovering glass: " + glass.name);
        }

        if (glass && UsePressed())
        {
            Debug.Log("Drank " + glass.name);
            glass.Drink();
        }


        // Prompt
        SetPrompt(glass ? "F — Drink" : "");

        // Use
        if (glass && UsePressed()) glass.Drink();
    }

    bool UsePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(useKey);
#endif
    }

    void SetPrompt(string s) { if (promptText) promptText.text = s; }

    bool ObjectInspector_IsInspecting()
    {
        var t = System.Type.GetType("ObjectInspector");
        if (t == null) return false;
        var p = t.GetProperty("IsInspecting",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (p == null) return false;
        var v = p.GetValue(null, null);
        return v is bool b && b;
    }

}
