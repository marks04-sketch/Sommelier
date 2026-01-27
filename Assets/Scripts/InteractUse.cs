using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InteractUse : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public TMP_Text promptText;

    [Header("Ray")]
    public LayerMask interactMask;   // Interactable
    public float useDistance = 3.0f;

    [Header("Keys")]
    public KeyCode inspectKey = KeyCode.E;
    public KeyCode useKey = KeyCode.F;

    [Header("Inspector")]
    public ObjectInspector objectInspector;

    void Reset()
    {
        cam = Camera.main;
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (objectInspector == null)
            objectInspector = FindObjectOfType<ObjectInspector>(true);

        // Asegura que el texto esté activo
        if (promptText) promptText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!cam) cam = Camera.main;

        if (ObjectInspector.IsInspecting)
        {
            SetPrompt("ESC - Exit");
            return;
        }

        // Ray al centro
        Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(r, out var hit, useDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            SetPrompt("");
            return;
        }

        // 1) Detecta INSPECTABLE (para la E)
        var inspectable = hit.collider.GetComponentInParent<InspectableObject>();

        // 2) Detecta DRINKABLE (para la F)
        var glass = hit.collider.GetComponentInParent<WineGlass>();

        // Si no es ni inspectable ni drinkable, no mostramos nada
        if (inspectable == null && glass == null)
        {
            SetPrompt("");
            return;
        }

        // Construye el prompt según lo que haya
        if (glass != null && inspectable != null)
            SetPrompt("E - Inspect | F - Drink");
        else if (inspectable != null)
            SetPrompt("E - Inspect");
        else
            SetPrompt("F - Drink");

        // INSPECT
        if (InspectPressed() && inspectable != null)
        {
            if (objectInspector == null)
            {
                Debug.LogWarning("InteractUse: objectInspector no asignado. Arrástralo en el Inspector.");
                return;
            }

            objectInspector.TryInspect(inspectable.gameObject);
            return;
        }

        // DRINK
        if (UsePressed() && glass != null)
        {
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

    void SetPrompt(string s)
    {
        if (!promptText) return;

        // Si el texto está invisible por alpha/material, esto lo fuerza
        promptText.gameObject.SetActive(true);
        promptText.alpha = 1f;

        promptText.text = s;
        promptText.ForceMeshUpdate();
    }
}
