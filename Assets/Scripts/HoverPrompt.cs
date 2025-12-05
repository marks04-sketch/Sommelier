using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HoverPrompt : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;                       // Main Camera
    public TMP_Text promptText;              // the TMP under the crosshair
    public Image crosshair;                  // optional: to tint on hover

    [Header("Masks & Range")]
    public LayerMask hotspotMask;            // set to Hotspot layer only
    public LayerMask inspectMask;            // set to Inspectable layer only
    public float rayDistance = 50f;

    [Header("Texts")]
    public string moveText = "Click — Move";
    public string inspectText = "E — Inspect";

    [Header("Crosshair Colors (optional)")]
    public Color normalColor = Color.white;
    public Color moveColor = new Color(1f, 0.95f, 0.7f);     // soft warm
    public Color inspectColor = new Color(0.7f, 0.9f, 1f);   // soft blue

    void Reset()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!cam) cam = Camera.main;
        if (ObjectInspectorFlag()) { SetPrompt("", normalColor); return; } // hide when inspecting

        // Ray from screen center
        Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 1) Check Hotspot first (move)
        if (Physics.Raycast(r, out var hit, rayDistance, hotspotMask, QueryTriggerInteraction.Collide))
        {
            SetPrompt(moveText, moveColor);
            return;
        }

        // 2) Check Inspectable next
        if (Physics.Raycast(r, out hit, rayDistance, inspectMask, QueryTriggerInteraction.Collide))
        {
            // make sure it actually has an InspectableObject or IInteractable
            if (hit.collider.GetComponentInParent<InspectableObject>() != null
                || hit.collider.GetComponentInParent<Component>() is IInteractable)
            {
                SetPrompt(inspectText, inspectColor);
                return;
            }
        }

        // 3) Nothing hovered
        SetPrompt("", normalColor);
    }

    void SetPrompt(string text, Color c)
    {
        if (promptText) promptText.text = text;
        if (crosshair) crosshair.color = c;
    }

    // Support both your flag (if you added it) or default false
    bool ObjectInspectorFlag()
    {
        var t = System.Type.GetType("ObjectInspector");
        if (t == null) return false;
        var prop = t.GetProperty("IsInspecting", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (prop == null) return false;
        object v = prop.GetValue(null, null);
        return v is bool b && b;
    }
}
