using UnityEngine;

namespace MagicalGarden.Farm
{
    public class GameManager : MonoBehaviour
    {
        public float dragSpeed = 5f;
        private Vector3 dragOrigin;
        private bool isDragging = false;

        void Update()
        {
            if (Input.GetMouseButtonDown(2)) // Middle mouse button
            {
                isDragging = true;
                dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = dragOrigin - currentPos;

                transform.position += difference;
            }
        }
    }
}
