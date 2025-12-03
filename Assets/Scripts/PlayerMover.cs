using System.Collections;
using UnityEngine;

namespace Sommelier.Player
{
    public class PlayerMover : MonoBehaviour
    {
        [Header("Movement")]
        public float moveDuration = 0.6f;   // 0 for instant
        public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Look")]
        public Transform cameraPivot;       // assign CameraPivot
        public Transform cameraTransform;   // assign Main Camera
        public float lookLerp = 12f;        // how fast to align yaw/pitch

        bool isMoving;
        Coroutine moveCo;

        // ✅ Add this property so other scripts (MouseLook) can read the moving state
        public bool IsMoving => isMoving;

        public void MoveTo(Transform target, Transform lookAt = null)
        {
            if (isMoving && moveCo != null) StopCoroutine(moveCo);
            moveCo = StartCoroutine(MoveRoutine(target, lookAt));
        }

        IEnumerator MoveRoutine(Transform target, Transform lookAt)
        {
            isMoving = true;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 faceDir = (lookAt ? (lookAt.position - cameraTransform.position) : target.forward);
            Vector3 flatDir = new Vector3(faceDir.x, 0f, faceDir.z).normalized;
            Quaternion endRot = flatDir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(flatDir, Vector3.up) : target.rotation;

            float t = 0f;
            float dur = Mathf.Max(0f, moveDuration);

            while (t < dur)
            {
                t += Time.deltaTime;
                float k = dur <= 0f ? 1f : ease.Evaluate(Mathf.Clamp01(t / dur));
                transform.position = Vector3.Lerp(startPos, target.position, k);
                transform.rotation = Quaternion.Slerp(startRot, endRot, k);

                if (lookAt != null)
                {
                    Vector3 dir = (lookAt.position - cameraTransform.position).normalized;
                    Vector3 localDir = transform.InverseTransformDirection(dir);
                    float pitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
                    var pivotRot = cameraPivot.localRotation;
                    var targetPivotRot = Quaternion.Euler(pitch, 0, 0);
                    cameraPivot.localRotation = Quaternion.Slerp(pivotRot, targetPivotRot, Time.deltaTime * lookLerp);
                }

                yield return null;
            }

            transform.position = target.position;
            transform.rotation = endRot;

            if (lookAt != null)
            {
                Vector3 dir = (lookAt.position - cameraTransform.position).normalized;
                Vector3 localDir = transform.InverseTransformDirection(dir);
                float pitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
            }
            else
            {
                cameraPivot.localRotation = Quaternion.identity;
            }

            isMoving = false;
        }
    }
}
