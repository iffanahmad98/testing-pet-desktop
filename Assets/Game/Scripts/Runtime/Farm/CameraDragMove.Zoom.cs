using UnityEngine;

namespace MagicalGarden.Farm
{
    public partial class CameraDragMove
    {
        #region Zoom Control

        private void HandleZoom()
        {
            if (!canZoom)
                return;

            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
            {
                float targetZoom = cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
                cam.orthographicSize = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                transform.position = ClampCameraPosition(transform.position, cam.orthographicSize);
            }
        }

        #endregion
    }
}
