using UnityEngine;

public class CloudParallax : MonoBehaviour
{
    public float speed = 0.1f; // Kecepatan awan
    public float resetPositionX = -20f; // Titik di mana awan di-reset ke kanan
    public float startPositionX = 20f;  // Posisi awal saat reset

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x <= resetPositionX)
        {
            Vector3 newPos = transform.position;
            newPos.x = startPositionX;
            transform.position = newPos;
        }
    }
}