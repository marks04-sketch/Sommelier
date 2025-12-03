using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // New Input System
#endif

namespace Sommelier.Player
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Refs")]
        [Tooltip("Rotate this around Y (player body). Usually the PlayerRig root.")]
        public Transform body;              // PlayerRig
        [Tooltip("Rotate this around X (camera pitch). Usually CameraPivot.")]
        public Transform cameraPivot;       // CameraPivot
        [Tooltip("Optional: pause look while moving.")]
        public PlayerMover mover;

        [Header("Settings")]
        public float sensitivity = 0.12f;   // tune to taste
        public float pitchMin = -70f;
        public float pitchMax = 70f;
        public bool lockCursorOnPlay = true;
        public bool pauseWhenMoving = true;
        public bool smooth = true;
        public float smoothLerp = 12f;

        float yaw;    // around Y on body
        float pitch;  // around X on camera pivot
        bool cursorLocked;

        void Start()
        {
            if (!body) body = transform;
            if (lockCursorOnPlay) LockCursor(true);

            // initialize from current transforms
            yaw = body.eulerAngles.y;
            pitch = cameraPivot ? NormalizePitch(cameraPivot.localEulerAngles.x) : 0f;
        }

        void OnDisable() { LockCursor(false); }

        void Update()
        {
            // ESC toggles cursor if you want to get your mouse back
            if (KeyboardEscDown()) LockCursor(false);
            if (!cursorLocked && MouseLeftDown()) LockCursor(true);

            if (!cursorLocked) return;
            if (pauseWhenMoving && mover && mover.IsMoving) return;

            Vector2 delta = ReadMouseDelta();          // pixels this frame
                                                       // Convert to degrees: sensitivity already tuned small (feel free to tweak)
            yaw += delta.x * sensitivity;
            pitch -= delta.y * sensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            // Apply rotations
            if (smooth)
            {
                var bodyTarget = Quaternion.Euler(0f, yaw, 0f);
                body.rotation = Quaternion.Slerp(body.rotation, bodyTarget, Time.deltaTime * smoothLerp);

                if (cameraPivot)
                {
                    var camTarget = Quaternion.Euler(pitch, 0f, 0f);
                    cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, camTarget, Time.deltaTime * smoothLerp);
                }
            }
            else
            {
                body.rotation = Quaternion.Euler(0f, yaw, 0f);
                if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        // ---------- helpers ----------
        Vector2 ReadMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return Vector2.zero;
            return Mouse.current.delta.ReadValue();
#else
      return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 100f;
#endif
        }

        bool MouseLeftDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
      return Input.GetMouseButtonDown(0);
#endif
        }

        bool KeyboardEscDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
      return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        float NormalizePitch(float eulerX)
        {
            // Convert 0..360 to -180..180 then clamp
            if (eulerX > 180f) eulerX -= 360f;
            return eulerX;
        }

        void LockCursor(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
