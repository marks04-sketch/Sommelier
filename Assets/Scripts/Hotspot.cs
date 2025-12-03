using UnityEngine;

namespace Sommelier.Navigation
{
    [RequireComponent(typeof(Collider))]
    public class Hotspot : MonoBehaviour
    {
        [Tooltip("Where the player should end up (pos+rot). Child transform recommended.")]
        public Transform target;
        [Tooltip("Optional: where the player should look after arriving. If null, uses target.forward.")]
        public Transform lookAt;
        [Tooltip("Optional: name shown in UI prompt/cursor.")]
        public string displayName;

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = false; // can be trigger or not; raycast works either way
            if (target == null)
            {
                var t = new GameObject("Target").transform;
                t.SetParent(transform, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                target = t;
            }
        }
    }
}
