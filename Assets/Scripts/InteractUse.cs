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
    public KeyCode inspectKey = KeyCode.E; // press E to inspect
    public KeyCode useKey = KeyCode.F;     // press F to drink

    void Reset() { cam = Camera.main; }

    void Update()
    {
        if (!cam) cam = Camera.main;

        // If currently inspecting: don't allow hover/use prompt from here
        if (ObjectInspector_IsInspecting())
        {
            SetPrompt(""); 
            return;
        }

        // Ray from the center of screen
        Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        WineGlass glass = null;

        if (Physics.Raycast(r, out var hit, useDistance, interactMask, QueryTriggerInteraction.Collide))
            glass = hit.collider.GetComponentInParent<WineGlass>();

        // Prompt: show both actions when looking at a bottle
        SetPrompt(glass ? "E - Inspect | F - Drink" : "");

        // INSPECT (E)
        if (glass && InspectPressed())
        {
            ObjectInspector_TryInspect(glass.gameObject);
            return; // avoid also drinking in same frame
        }

        // DRINK (F)
        if (glass && UsePressed())
        {
            Debug.Log("Drank " + glass.name);
            glass.Drink();
        }
    }

    bool InspectPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(inspectKey);
#endif
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

    // --- ObjectInspector integration via reflection (no dependency on direct reference) ---

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

    void ObjectInspector_TryInspect(GameObject target)
    {
        var t = System.Type.GetType("ObjectInspector");
        if (t == null)
        {
            Debug.LogWarning("ObjectInspector type not found.");
            return;
        }

        // Find an ObjectInspector component in the scene
        var inspector = Object.FindFirstObjectByType(t);
        if (inspector == null)
        {
            Debug.LogWarning("No ObjectInspector component found in scene.");
            return;
        }

        // Try common method names (public or private)
        string[] methodNames = {
            "Inspect", "InspectObject", "StartInspect", "BeginInspect", "Open", "OpenInspect"
        };

        foreach (var name in methodNames)
        {
            var m = t.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) continue;

            var pars = m.GetParameters();
            try
            {
                if (pars.Length == 1)
                {
                    // Accept GameObject / Transform / Component
                    if (pars[0].ParameterType == typeof(GameObject))
                    {
                        m.Invoke(inspector, new object[] { target });
                        return;
                    }
                    if (pars[0].ParameterType == typeof(Transform))
                    {
                        m.Invoke(inspector, new object[] { target.transform });
                        return;
                    }
                    if (typeof(Component).IsAssignableFrom(pars[0].ParameterType))
                    {
                        var comp = target.GetComponent(pars[0].ParameterType);
                        if (comp != null)
                        {
                            m.Invoke(inspector, new object[] { comp });
                            return;
                        }
                    }
                }
                else if (pars.Length == 0)
                {
                    // Some inspectors inspect based on their own raycast
                    m.Invoke(inspector, null);
                    return;
                }
            }
            catch { /* try next */ }
        }

        Debug.LogWarning("Could not find a usable inspect method in ObjectInspector.");
    }
}
