using UnityEngine;

namespace MagicalGarden.Farm
{
    public partial class CameraDragMove
    {
        #region Keyboard/Gamepad Pan

        private void HandleKeyboardPan()
        {
            if (!canDrag)
                return;

            if (cam == null)
                cam = Camera.main;

            float x = Input.GetAxisRaw("Horizontal"); // A/D, ←/→, gamepad left stick X
            float y = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓, gamepad left stick Y

            if (Mathf.Abs(x) > 0.001f || Mathf.Abs(y) > 0.001f)
            {
                float speed = panSpeed * Time.deltaTime * cam.orthographicSize;
                Vector3 move = new Vector3(x, y, 0f).normalized * speed;
                Vector3 target = transform.position + move;
                transform.position = ClampCameraPosition(target, cam.orthographicSize);
            }
        }

        #endregion

        #region Mouse Drag

        private void HandleDrag()
        {
            if (!canDrag)
                return;

            if (cam == null)
                cam = Camera.main;

            bool down = Input.GetMouseButtonDown(2) || (allowLeftClickDrag && Input.GetMouseButtonDown(0));
            bool up = Input.GetMouseButtonUp(2) || (allowLeftClickDrag && Input.GetMouseButtonUp(0));
            bool hold = Input.GetMouseButton(2) || (allowLeftClickDrag && Input.GetMouseButton(0));

            if (down)
            {
                isDragging = true;
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (up)
                isDragging = false;

            if (isDragging && hold)
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = dragOrigin - currentPos;
                Vector3 newPos = transform.position + difference;
                transform.position = ClampCameraPosition(newPos, cam.orthographicSize);
            }
        }

        #endregion
    }
}
