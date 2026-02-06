using UnityEngine;

public interface ITutorialPointer
{
    /// <summary>
    /// Menunjuk ke target UI (RectTransform) dengan offset lokal di canvas.
    /// </summary>
    /// <param name="target">UI target yang ingin ditunjuk.</param>
    /// <param name="offset">Offset tambahan dari posisi target (dalam satuan anchoredPosition canvas).</param>
    void PointTo(RectTransform target, Vector2 offset);

    /// <summary>
    /// Menyembunyikan pointer dan menghentikan semua animasi.
    /// </summary>
    void Hide();
}
