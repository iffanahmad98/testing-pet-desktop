using UnityEngine;
using UnityEngine.UI;

public class GameAreaImageAnalyzer
{
    private Image _gameAreaImage;
    private Texture2D _backgroundTexture;
    private float _groundThreshold = 0.1f; // Alpha threshold to consider as "ground"
    private (Vector2 min, Vector2 max)? _cachedGroundBounds;
    
    public GameAreaImageAnalyzer(RectTransform gameArea)
    {
        _gameAreaImage = gameArea.GetComponent<Image>();
        if (_gameAreaImage != null)
        {
            ExtractTextureFromImage();
            AnalyzeGroundArea();
        }
    }
    
    private void ExtractTextureFromImage()
    {
        if (_gameAreaImage.sprite != null)
        {
            _backgroundTexture = _gameAreaImage.sprite.texture;
        }
    }
    
    private void AnalyzeGroundArea()
    {
        if (_backgroundTexture == null) return;
        
        // Sample the image to find ground boundaries
        int width = _backgroundTexture.width;
        int height = _backgroundTexture.height;
        
        float minGroundY = float.MaxValue;
        float maxGroundY = float.MinValue;
        
        // Sample from bottom to top to find ground area
        for (int x = 0; x < width; x += 10) // Sample every 10 pixels for performance
        {
            for (int y = 0; y < height; y += 5) // More frequent vertical sampling
            {
                Color pixel = _backgroundTexture.GetPixel(x, y);
                
                // If pixel has enough alpha (not transparent), consider it ground
                if (pixel.a > _groundThreshold)
                {
                    // Convert pixel coordinates to normalized coordinates
                    float normalizedY = (float)y / height;
                    
                    minGroundY = Mathf.Min(minGroundY, normalizedY);
                    maxGroundY = Mathf.Max(maxGroundY, normalizedY);
                }
            }
        }
        
        // Convert normalized coordinates to game area coordinates
        var gameAreaRect = _gameAreaImage.rectTransform;
        Vector2 size = gameAreaRect.sizeDelta;
        
        if (minGroundY != float.MaxValue)
        {
            // Convert from texture space (0,0 bottom-left) to UI space (0,0 center)
            float worldMinY = (minGroundY - 0.5f) * size.y;
            float worldMaxY = (maxGroundY - 0.5f) * size.y;
            
            _cachedGroundBounds = (
                new Vector2(-size.x / 2, worldMinY),
                new Vector2(size.x / 2, worldMaxY)
            );
        }
    }
    
    public (Vector2 min, Vector2 max)? GetGroundBounds()
    {
        return _cachedGroundBounds;
    }
    
    public bool IsGroundPosition(Vector2 localPosition, RectTransform gameArea)
    {
        if (_backgroundTexture == null || _cachedGroundBounds == null) return true;
        
        var bounds = _cachedGroundBounds.Value;
        return localPosition.y >= bounds.min.y && localPosition.y <= bounds.max.y;
    }
}