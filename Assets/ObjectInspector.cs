using UnityEngine;

public class ObjectInspector : MonoBehaviour
{
    [Header("General")]
    public Camera playerCamera;
    public LayerMask inspectLayer;
    public float inspectDistance = 1.5f;
    public float moveSpeed = 10f;

    [Header("Rotation")]
    public float rotateSpeed = 5f;

    [Header("Input")]
    public KeyCode exitKey = KeyCode.Escape;

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
        // Left click to start inspecting
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, inspectLayer))
            {
                var inspectable = hit.collider.GetComponent<InspectableObject>();
                if (inspectable != null)
                {
                    StartInspect(inspectable);
                }
            }
        }
    }

    void StartInspect(InspectableObject obj)
    {
        _current = obj;
        _current.StoreOriginalTransform();
        _current.OnInspectStart();

        // Parent the object to the anchor in front of camera
        _current.transform.SetParent(_inspectAnchor);
        _isInspecting = true;
    }

    void HandleInspectRotate()
    {
        // Smoothly move toward anchor
        _current.transform.position = Vector3.Lerp(
            _current.transform.position,
            _inspectAnchor.position,
            Time.deltaTime * moveSpeed);

        // Rotate with mouse drag
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            _current.transform.Rotate(playerCamera.transform.up, -mouseX * rotateSpeed, Space.World);
            _current.transform.Rotate(playerCamera.transform.right, mouseY * rotateSpeed, Space.World);
        }
    }

    void HandleInspectEnd()
    {
        // Right click or key to exit
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(exitKey))
        {
            EndInspect();
        }
    }

    void EndInspect()
    {
        _current.transform.SetParent(_current.OriginalParent);
        _current.transform.position = _current.OriginalPosition;
        _current.transform.rotation = _current.OriginalRotation;

        _current.OnInspectEnd();
        _current = null;
        _isInspecting = false;
    }
}
