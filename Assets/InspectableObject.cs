using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [HideInInspector] public Vector3 OriginalPosition;
    [HideInInspector] public Quaternion OriginalRotation;
    [HideInInspector] public Transform OriginalParent;

    Collider _collider;
    Rigidbody _rb;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
    }

    public void StoreOriginalTransform()
    {
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
        OriginalParent = transform.parent;
    }

    public void OnInspectStart()
    {
        if (_collider) _collider.enabled = false;
        if (_rb)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    public void OnInspectEnd()
    {
        if (_collider) _collider.enabled = true;
        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }
}

