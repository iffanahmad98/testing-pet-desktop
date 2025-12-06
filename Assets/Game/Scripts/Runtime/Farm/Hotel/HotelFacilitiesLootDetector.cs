using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class HotelFacilitiesLootDetector : MonoBehaviour
{
    public HotelRandomLoot hotelRandomLoot;
    [Header("Filter")]
    public string targetTag = "ClickableDecoration";
    public LootType [] lootTypes;

    [Tooltip("Collider yang terdeteksi")]
    public List<PolygonCollider2D> detectedPolygons = new List<PolygonCollider2D>();
    public List<PolygonCollider2D> detectedLootPolygons = new List <PolygonCollider2D> ();
    public Tilemap tilemap;   // assign tilemap di inspector
    void Awake () {
        hotelRandomLoot = GameObject.Find ("HotelEvents").transform.Find ("HotelRandomLoot").GetComponent <HotelRandomLoot> ();
    }
    
    void Start () {
        
        if (tilemap == null) {
            tilemap = TileManager.Instance.tilemapHotelFacilities;
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (!other.CompareTag(targetTag)) return;

        PolygonCollider2D poly = other.GetComponent<PolygonCollider2D>();
        if (poly == null) return;

        if (!detectedPolygons.Contains(poly))
        {
            detectedPolygons.Add(poly);
            foreach (LootType lootType in lootTypes) {
                if (hotelRandomLoot.IsHasLoot (lootType,poly.gameObject)) {
                    detectedLootPolygons.Add (poly);
                }
            }
          //   DetectTiles (poly.gameObject);
            // Debug.Log($"Detected: {poly.name}");
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        PolygonCollider2D poly = other.GetComponent<PolygonCollider2D>();
        if (poly == null) return;

        if (detectedPolygons.Contains(poly))
        {
            detectedPolygons.Remove(poly);
            /*
            if (hotelRandomLoot.IsHasLoot (lootType,poly.gameObject)) {
                detectedLootPolygons.Remove (poly);
            }
            */
            foreach (LootType lootType in lootTypes) {
                if (hotelRandomLoot.IsHasLoot (lootType,poly.gameObject)) {
                    detectedLootPolygons.Remove (poly);
                }
            }

            // Debug.Log($"Removed: {poly.name}");
        }
        
    }

    #region NPCLootHunter
    public bool IsAnyLoots () {
        return detectedLootPolygons.Count >0;
    }

    /*
    public Vector2Int GetRandomLootPosition () {
        return DetectTiles (detectedLootPolygons[Random.Range (0, detectedLootPolygons.Count)].gameObject);
    }
    */

    public (Vector2Int pos, GameObject obj) GetRandomLootPosition()
    {
        int idTarget = Random.Range(0, detectedLootPolygons.Count);
        GameObject chosen = detectedLootPolygons[idTarget].gameObject;
        Vector2Int tilePos = DetectTiles(chosen);
        detectedLootPolygons.Remove (detectedLootPolygons[idTarget]);
        return (tilePos, chosen);
    }
    #endregion
    #region Tileset

    // Panggil: DetectTiles(gameObject);
    Vector2Int DetectTiles(GameObject target)
    {
        PolygonCollider2D poly = target.GetComponent<PolygonCollider2D>();
        if (poly == null)
        {
            Debug.LogError("GameObject tidak punya PolygonCollider2D!");
            return Vector2Int.zero;
        }

        if (tilemap == null)
        {
            Debug.LogError("Tilemap belum di-assign!");
            return Vector2Int.zero;
        }

        HashSet<Vector3Int> tiles = new HashSet<Vector3Int>();

        // Ambil semua titik polygon → konversi ke tile
        for (int i = 0; i < poly.points.Length; i++)
        {
            Vector2 worldPoint = target.transform.TransformPoint(poly.points[i]);
            Vector3Int tilePos = tilemap.WorldToCell(worldPoint);
            tiles.Add(tilePos);
        }

        // Debug print
        foreach (Vector3Int pos in tiles)
        {
            Debug.Log("Posisi tileset: " + pos);
        }

        // Jika tidak ada tile, kembalikan 0
        if (tiles.Count == 0)
            return Vector2Int.zero;

        // Ambil tile random
        Vector3Int[] arr = new Vector3Int[tiles.Count];
        tiles.CopyTo(arr);

        Vector3Int randomTile = arr[Random.Range(0, arr.Length)];

        return new Vector2Int(randomTile.x, randomTile.y);
    }

    #endregion
}
