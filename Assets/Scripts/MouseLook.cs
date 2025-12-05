using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Sommelier.Player
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Refs")]
        public Transform body;              // PlayerRig root
        public Transform cameraPivot;       // CameraPivot (pitch)
        public PlayerMover mover;           // optional

        [Header("Settings")]
        public float sensitivity = 0.12f;   // overwritten at Start()
        public float pitchMin = -70f;
        public float pitchMax = 70f;
        public bool lockCursorOnPlay = true;
        public bool pauseWhenMoving = true;
        public bool smooth = true;
        public float smoothLerp = 12f;

        float yaw;
        float pitch;
        bool cursorLocked;

        void Start()
        {
            if (!body) body = transform;

            // Load saved sensitivity safely
            sensitivity = PlayerPrefs.GetFloat("Sensitivity", sensitivity);

            if (lockCursorOnPlay)
                LockCursor(true);

            // initialize from current transforms
            yaw = body.eulerAngles.y;
            pitch = cameraPivot ? NormalizePitch(cameraPivot.localEulerAngles.x) : 0f;
        }

        // Called externally when settings slider changes
        public void ApplySensitivity(float newSens)
        {
            sensitivity = newSens;
        }

        void OnDisable()
        {
            LockCursor(false);
        }

        void Update()
        {
            if (ObjectInspector.IsInspecting) return;

            // ESC unlocks, left click locks again
            if (KeyboardEscDown()) LockCursor(false);
            if (!cursorLocked && MouseLeftDown()) LockCursor(true);

            if (!cursorLocked) return;
            if (pauseWhenMoving && mover && mover.IsMoving) return;

            // ---- READ MOUSE INPUT ----
            Vector2 delta = ReadMouseDelta();

            // Apply sensitivity
            delta *= sensitivity;

            // Accumulate rotation
            yaw += delta.x;
            pitch -= delta.y;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            // ---- APPLY ROTATION ----
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

        // -------- Helpers --------
        Vector2 ReadMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return Vector2.zero;
            return Mouse.current.delta.ReadValue();
#else
            return new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            ) * 100f;
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
