using UnityEngine;

namespace MagicalGarden.Farm
{
    public partial class CameraDragMove
    {
        #region Boundary Clamping

        private Vector3 ClampCameraPosition(Vector3 targetPos, float customZoom)
        {
            if (currentBoundary == null)
                return targetPos;

            Bounds bounds = currentBoundary.bounds;
            float camHeight = customZoom;
            float camWidth = camHeight * cam.aspect;

            float minX = bounds.min.x + camWidth;
            float maxX = bounds.max.x - camWidth;
            float minY = bounds.min.y + camHeight;
            float maxY = bounds.max.y - camHeight;

            float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
            float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

            return new Vector3(clampedX, clampedY, targetPos.z);
        }

        #endregion
    }
}
