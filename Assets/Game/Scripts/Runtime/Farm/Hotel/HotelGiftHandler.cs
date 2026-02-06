using UnityEngine;
using TMPro;
using MagicalGarden.Gift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
using MagicalGarden.AI;
using System;
public class HotelGiftHandler : MonoBehaviour
{
    public static HotelGiftHandler instance;
     
    [SerializeField] GameObject hotelGiftDisplayPrefab;
    [SerializeField] GameObject hotelGiftReceivedPrefab;
    [Header ("UI")]
    [SerializeField] GameObject lootTargetUI;
    [SerializeField] TMP_Text currentText;
    public DOTweenScaleBounce uiIconBounce;
    public DOTweenScaleBounce uiIconTabBounce;

    [Header ("Hotel Gift Spawner")]
    [SerializeField] GameObject hotelGift2dPrefab;

    [Tooltip ("NPC Service")]
    public List <NPCService> listNPCService = new List <NPCService> ();
    [Tooltip("Collider yang terdeteksi")]
   // public List<PolygonCollider2D> detectedGiftPolygons = new List <PolygonCollider2D> ();
    public Tilemap tilemap;   // assign tilemap di inspector
    bool onceRefresh = false;

    [Tooltip ("Data")]
    public bool dataLoaded = false;
    public List <GameObject> listHotelGift = new List <GameObject> ();
    public int maxHotelGiftLoad = 4;
    
    void Awake () {
        instance = this;
    }

    void Start () {
        if (tilemap == null) {
            tilemap = TileManager.Instance.tilemapHotelFacilities;
        }

        LoadHotelGift ();
    }


    public void SpawnHotelGiftDisplay () { // HotelGiftSpawner
        if (!onceRefresh) {
            onceRefresh = true;
            currentText.text = HotelGift.instance.GetCurrency().ToString ();
        }

        try {
        GameObject clone = GameObject.Instantiate(hotelGiftDisplayPrefab);
        clone.SetActive (true);
        clone.transform.SetParent (lootTargetUI.transform.parent);
        clone.GetComponent<HotelLootDisplay> ().OnClearTransitionFinished += PlayLootBounce;
        clone.GetComponent <HotelLootDisplay> ().StartPlay (lootTargetUI.transform); 
        ShowTabLoot ();
        } catch (Exception exception) {
            PlayLootBounce ();
        }
    }

    void ShowTabLoot () {
        
       // uiIconTabBounce.gameObject.SetActive (true);
      //  uiIconTabBounce.Play ();
    }

    void PlayLootBounce () {
        Debug.Log ("Refresh Bounce " + HotelGift.instance.GetCurrency());
        currentText.text = HotelGift.instance.GetCurrency().ToString ();
        uiIconBounce.Play ();
    }

    #region Claim Gift
    
    public void ClaimGift (GiftItem giftItem, bool isManualClick) { // GiftItem.cs
    // Debug.Log ("ClaimGift");
     GameObject cloneReceived = GameObject.Instantiate (hotelGiftReceivedPrefab);
     // cloneReceived.transform.SetParent (giftItem.gameObject.transform);
     cloneReceived.SetActive (true); 
     cloneReceived.transform.localPosition = new Vector3 (0, 2.0f, 0);

     HotelGift.instance.GetLoot (1);
     HotelGiftHandler.instance.SpawnHotelGiftDisplay ();

     RemoveHotelGift (giftItem.gameObject);
     
     if (isManualClick) {
        RefreshAllMovementRoboShroom ();
     } 
   }
    #endregion
    
    #region NPCRoboShroom
    public bool IsAnyGifts () {
        return listHotelGift.Count >0;
    }

    /*
    public Vector2Int GetRandomLootPosition () {
        return DetectTiles (detectedLootPolygons[Random.Range (0, detectedLootPolygons.Count)].gameObject);
    }
    */

    public (Vector2Int pos, GameObject obj) GetRandomGiftPosition()
    {
        int idTarget = UnityEngine.Random.Range(0, listHotelGift.Count);
        GameObject chosen = listHotelGift[idTarget].gameObject;
        Vector2Int tilePos = DetectTiles(chosen);
        listHotelGift.Remove (listHotelGift[idTarget]);
        return (tilePos, chosen);
    }

    public (Vector2Int pos, GameObject obj) GetSpecificGiftPosition (GameObject target) {
        GameObject chosen = target;
        Vector2Int tilePos = DetectTiles(chosen);
        listHotelGift.Remove (chosen);
        return (tilePos, chosen);
    }

    public List <GameObject> GetListHotelGift () {
        return listHotelGift;
    }
    #endregion
    #region Tileset

    // Panggil: DetectTiles(gameObject);
    Vector2Int DetectTiles(GameObject target)
    {
        BoxCollider2D box = target.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogError("GameObject tidak punya BoxCollider2D!");
            return Vector2Int.zero;
        }

        if (tilemap == null)
        {
            Debug.LogError("Tilemap belum di-assign!");
            return Vector2Int.zero;
        }

        HashSet<Vector3Int> tiles = new HashSet<Vector3Int>();

        // Ambil 4 sudut BoxCollider2D (local space)
        Vector2 size = box.size * 0.5f;

        Vector2[] corners =
        {
            new Vector2(-size.x, -size.y), // kiri bawah
            new Vector2(-size.x,  size.y), // kiri atas
            new Vector2( size.x,  size.y), // kanan atas
            new Vector2( size.x, -size.y)  // kanan bawah
        };

        // Konversi dari local → world → tile
        foreach (var localCorner in corners)
        {
            Vector2 worldPoint = box.transform.TransformPoint(localCorner);
            Vector3Int tilePos = tilemap.WorldToCell(worldPoint);
            tiles.Add(tilePos);
        }

        // Debug
        foreach (Vector3Int pos in tiles)
            Debug.Log("Posisi tileset: " + pos);

        if (tiles.Count == 0)
            return Vector2Int.zero;

        // Ambil tile random
        Vector3Int[] arr = new Vector3Int[tiles.Count];
        tiles.CopyTo(arr);

        Vector3Int randomTile = arr[UnityEngine.Random.Range(0, arr.Length)];
        return new Vector2Int(randomTile.x, randomTile.y);
    }


    #endregion

    #region Data

    public void AddHotelGift (GameObject giftObject) { // HotelGiftSpawner
        listHotelGift.Add (giftObject);
        if (dataLoaded) {
            SaveAddHotelGift (giftObject);
        }
    }

    void RemoveHotelGift (GameObject giftObject) {
        listHotelGift.Remove (giftObject);
        if (dataLoaded) {
            SaveRemoveHotelGift (giftObject);
        }
    }

    void SaveAddHotelGift (GameObject giftObject) {
        SaveSystem.PlayerConfig.AddHotelGiftWorld (giftObject.transform.position);
    }

    void SaveRemoveHotelGift (GameObject giftObject) {
        SaveSystem.PlayerConfig.RemoveHotelGiftWorld (giftObject.transform.position);
        Debug.Log ("Save Remove Hotel Gift");
    }

    void LoadHotelGift () {
        if (SaveSystem.PlayerConfig.HasHotelFacilityAndIsActive ("robo_shroom")) {
            // jika memiliki Robo shroom
            Debug.Log ("Memiliki Robo Shroom, hadiah terakhir tersisa : " + SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Count);
            // langsung save hadiah terakhir & Hapus semua data Hotel Gift di World :
            HotelGift.instance.GetLoot (SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Count);
            SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Clear ();
        } else {
            // jika tidak memiliki robo shroom
            int curLoadGift = 0;

            for (int i = 0; i < maxHotelGiftLoad && i < SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Count; i++)
            {
                int lastElement = SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Count - i -1;
                OnSpawnGiftByLoadData (SaveSystem.PlayerConfig.ownedHotelGiftWorldData[lastElement].dataPosition);
            }

            for (int i = 0; i < SaveSystem.PlayerConfig.ownedHotelGiftWorldData.Count - maxHotelGiftLoad; i++)
            {
                // remove old gifts :
                    SaveSystem.PlayerConfig.RemoveHotelGiftWorld (SaveSystem.PlayerConfig.ownedHotelGiftWorldData[i].dataPosition);
            }

             Debug.Log ("Tidak Memiliki Robo Shroom");
        }

        dataLoaded = true;
    }

    #endregion

    #region HotelGiftSpawner (World)
    void OnSpawnGiftByLoadData (Vector3 position) {
      GameObject cloneHotelGift = GameObject.Instantiate (hotelGift2dPrefab);
      cloneHotelGift.SetActive (true); 
      cloneHotelGift.transform.position = position;
      HotelGiftHandler.instance.AddHotelGift (cloneHotelGift);

   }
    #endregion

    #region NPCService
    // NPCRoboShroom.cs
    public void AddNPCService(NPCService npc) {
        listNPCService.Add (npc);
    }

    // NPCRoboShroom.cs
    public void RemoveNPCService (NPCService npc) {
        listNPCService.Remove (npc);
    }

    void RefreshAllMovementRoboShroom () {
        foreach (NPCService npc in listNPCService) {
            npc.ResetMovement ();    
        }
    }
    #endregion
    
}
