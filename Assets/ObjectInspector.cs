using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ObjectInspector : MonoBehaviour
{
    public Image crosshair;  // assign in Inspector

    [Header("General")]
    public Camera playerCamera;
    public LayerMask inspectLayer;
    public float inspectDistance = 1.5f;
    public float moveSpeed = 10f;

    [Header("Rotation")]
    public float rotateSpeed = 5f;

    [Header("Input")]
    public KeyCode exitKey = KeyCode.Escape;

    // --- AUDIO (NEW) ---
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip inspectClip; // Assign: InspectingBottles

    // --- STATIC FLAG (this is what ClickMover/MouseLook expect) ---
    public static bool IsInspecting { get; private set; }

    Transform _inspectAnchor;
    InspectableObject _current;
    bool _isInspecting;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Create invisible anchor in front of camera
        _inspectAnchor = new GameObject("InspectAnchor").transform;
        _inspectAnchor.SetParent(playerCamera.transform);
        _inspectAnchor.localPosition = new Vector3(0f, 0f, inspectDistance);
        _inspectAnchor.localRotation = Quaternion.identity;

        _isInspecting = false;
        IsInspecting = false;
    }

    void Update()
    {
        if (!_isInspecting)
        {
            HandleInspectStart();
        }
        else
        {
            HandleInspectRotate();
            HandleInspectEnd();
        }
    }

    void HandleInspectStart()
    {
        bool ePressed =
#if ENABLE_INPUT_SYSTEM
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            Input.GetKeyDown(KeyCode.E);
#endif

        if (!ePressed) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, inspectLayer))
        {
            var inspectable = hit.collider.GetComponentInParent<InspectableObject>();
            if (inspectable != null)
            {
                StartInspect(inspectable);
            }
        }
    }

    // IMPORTANT: This method name/signature is what InteractUse will call if we want.
    public void TryInspect(GameObject go)
    {
        if (_isInspecting) return;
        if (go == null) return;

        var inspectable = go.GetComponentInParent<InspectableObject>();
        if (inspectable != null)
        {
            StartInspect(inspectable);
        }
    }

    void StartInspect(InspectableObject obj)
    {
        _current = obj;
        _current.StoreOriginalTransform();
        _current.OnInspectStart();

        _current.transform.SetParent(_inspectAnchor);
        _isInspecting = true;
        IsInspecting = true;

        // --- PLAY INSPECT SFX ONCE (NEW) ---
        if (sfxSource != null && inspectClip != null)
            sfxSource.PlayOneShot(inspectClip);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshair) crosshair.enabled = false;
    }

    void HandleInspectRotate()
    {
        if (_current == null) return;

        _current.transform.position = Vector3.Lerp(
            _current.transform.position,
            _inspectAnchor.position,
            Time.deltaTime * moveSpeed
        );

        bool dragging;
        float mouseX, mouseY;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        dragging = mouse != null && mouse.leftButton.isPressed;
        Vector2 delta = (mouse != null) ? mouse.delta.ReadValue() : Vector2.zero;
        mouseX = delta.x;
        mouseY = delta.y;
#else
        dragging = Input.GetMouseButton(0);
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
#endif

        if (dragging)
        {
            _current.transform.Rotate(playerCamera.transform.up, -mouseX * rotateSpeed, Space.World);
            _current.transform.Rotate(playerCamera.transform.right, mouseY * rotateSpeed, Space.World);
        }
    }

    void HandleInspectEnd()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(exitKey))
        {
            EndInspect();
        }
    }

    void EndInspect()
    {
        if (_current != null)
        {
            _current.transform.SetParent(_current.OriginalParent);
            _current.transform.position = _current.OriginalPosition;
            _current.transform.rotation = _current.OriginalRotation;

            _current.OnInspectEnd();
        }

        _current = null;
        _isInspecting = false;
        IsInspecting = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair) crosshair.enabled = true;
    }
}
