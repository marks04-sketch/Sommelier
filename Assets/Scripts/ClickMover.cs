using UnityEngine;
using Sommelier.Navigation;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // new input system
#endif

namespace Sommelier.Inputs
{
    public class ClickMover : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Sommelier.Player.PlayerMover mover;

        [Header("Raycast")]
        public float rayDistance = 50f;
        public LayerMask hotspotMask; // set to Hotspot layer

        [Header("UI Prompt")]
        public bool changeCursorOnHover = true;
        public Texture2D hoverCursor;
        public Vector2 cursorHotspot;

        Hotspot hovered;

        void Reset()
        {
            if (!cam) cam = Camera.main;
            if (!mover) mover = GetComponent<Sommelier.Player.PlayerMover>();
        }

        void Update()
        {
            Vector3 mousePos;

            // --- read mouse position depending on input backend ---
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return;
            mousePos = Mouse.current.position.ReadValue();
            bool clicked = Mouse.current.leftButton.wasPressedThisFrame;
#else
      mousePos = Input.mousePosition;
      bool clicked = Input.GetMouseButtonDown(0);
#endif

            // --- raycast to hotspots only ---
            Ray r = cam.ScreenPointToRay(mousePos);
            hovered = null;

            if (Physics.Raycast(r, out var hit, rayDistance, hotspotMask, QueryTriggerInteraction.Collide))
            {
                hovered = hit.collider.GetComponentInParent<Hotspot>();
            }

            if (changeCursorOnHover)
            {
                if (hovered != null && hoverCursor != null)
                    Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
                else
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            if (hovered != null && clicked)
            {
                var target = hovered.target != null ? hovered.target : hovered.transform;
                mover.MoveTo(target, hovered.lookAt);
            }
        }
    }
}
