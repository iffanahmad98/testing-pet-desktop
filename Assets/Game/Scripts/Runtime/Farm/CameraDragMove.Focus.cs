using UnityEngine;
using System.Collections;

namespace MagicalGarden.Farm
{
    public partial class CameraDragMove
    {
        #region Focus & Movement
        public void FocusOnTarget(Vector3 target, float zoomSize = 4f, float duration = 1f, bool isHotel = false)
        {
            currentBoundary = isHotel ? boundaryColliderHotel : boundaryColliderFarm;

            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(MoveToTarget(target, duration));
        }
        public void ResetZoom(float duration = 1f)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(MoveToTarget(transform.position, duration));
        }

        #endregion

        #region Animation Coroutines

        private IEnumerator MoveToTarget(Vector3 targetPosition, float duration)
        {
            yield return null;

            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseInOutSine(t);

                Vector3 pos = Vector3.Lerp(startPos, new Vector3(targetPosition.x, targetPosition.y, startPos.z), easedT);
                transform.position = pos; // langsung set tanpa clamp

                yield return null;
            }

            transform.position = new Vector3(targetPosition.x, targetPosition.y, startPos.z); // final snap
        }

        #endregion

        #region Easing Functions

        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        }

        #endregion
    }
}
