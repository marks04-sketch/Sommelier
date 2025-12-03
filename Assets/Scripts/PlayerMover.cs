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

            // Desired final yaw from target.forward; pitch is set on camera pivot if lookAt provided
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

                // If we have a lookAt, gently pitch camera toward it
                if (lookAt != null)
                {
                    Vector3 dir = (lookAt.position - cameraTransform.position).normalized;
                    // yaw handled by body; compute pitch only
                    Vector3 localDir = transform.InverseTransformDirection(dir);
                    float pitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
                    var pivotRot = cameraPivot.localRotation;
                    var targetPivotRot = Quaternion.Euler(pitch, 0, 0);
                    cameraPivot.localRotation = Quaternion.Slerp(pivotRot, targetPivotRot, Time.deltaTime * lookLerp);
                }

                yield return null;
            }

            // Snap final
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
