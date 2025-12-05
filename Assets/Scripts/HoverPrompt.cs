using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HoverPrompt : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public TMP_Text promptText;
    public Image crosshair;

    [Header("Masks & Range")]
    public LayerMask interactMask;     // <-- Interactable (WineGlass)
    public LayerMask inspectMask;      // <-- Inspectable (InspectableObject)
    public LayerMask hotspotMask;      // <-- Hotspot (click-to-move)
    public float rayDistance = 50f;

    [Header("Texts")]
    public string useText = "F — Drink";   // WineGlass
    public string inspectText = "E — Inspect"; // InspectableObject
    public string moveText = "Click — Move";

    [Header("Crosshair Colors (optional)")]
    public Color normalColor = Color.white;
    public Color useColor = new Color(0.7f, 0.9f, 1f);
    public Color inspectColor = new Color(0.9f, 0.9f, 0.6f);
    public Color moveColor = new Color(1f, 0.95f, 0.7f);

    void Reset() { cam = Camera.main; }

    void Update()
    {
        if (!cam) cam = Camera.main;

        // Hide during inspection (if you added that flag)
        var inspType = System.Type.GetType("ObjectInspector");
        if (inspType != null)
        {
            var p = inspType.GetProperty("IsInspecting",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);
            if (p != null && p.GetValue(null, null) is bool b && b)
            { SetPrompt("", normalColor); return; }
        }

        Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 1) Interactable (WineGlass) → F — Drink
        if (Physics.Raycast(r, out var hit, rayDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponentInParent<WineGlass>() != null)
            { SetPrompt(useText, useColor); return; }
        }

        // 2) Inspectable → E — Inspect
        if (Physics.Raycast(r, out hit, rayDistance, inspectMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponentInParent<InspectableObject>() != null)
            { SetPrompt(inspectText, inspectColor); return; }
        }

        // 3) Hotspot → Click — Move
        if (Physics.Raycast(r, out hit, rayDistance, hotspotMask, QueryTriggerInteraction.Collide))
        {
            SetPrompt(moveText, moveColor); return;
        }

        // 4) Nothing
        SetPrompt("", normalColor);
    }

    void SetPrompt(string text, Color c)
    {
        if (promptText) promptText.text = text;
        if (crosshair) crosshair.color = c;
    }
}
