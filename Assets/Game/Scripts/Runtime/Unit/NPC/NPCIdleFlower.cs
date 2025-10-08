using UnityEngine;
using UnityEngine.UI;

public class NPCIdleFlower : MonoBehaviour, ITargetable
{
    private bool _isTargetable = true;
    private RectTransform _rectTransform;
    private Vector2 _position;
    [SerializeField] private Transform IdleStation1;
    [SerializeField] private Transform IdleStation2;

    [Header("Image References")] [SerializeField]
    private Image targetImage; // Image component yang akan diubah

    [SerializeField] private Sprite imageSource1; // Default sprite (1 NPC)
    [SerializeField] private Sprite imageSource2; // Sprite ketika punya 2 NPC

    [Header("NPC Monster Check")] [SerializeField]
    private string npcMonster2Id = "npc_monster_2"; // ID untuk NPC Monster ke-2

    public bool IsTargetable => _isTargetable;
    public Vector2 Position => _position;

    private bool _hasAnyNPC = false; // Player punya minimal 1 NPC
    private bool _hasSecondNPC = false; // Player punya NPC ke-2

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _position = _rectTransform.anchoredPosition;
    }

    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize NPC ownership check dan update image
    /// Dipanggil saat Start() dan ketika player membeli NPC facility
    /// </summary>
    public void Initialize()
    {
        CheckNPCOwnership();
        UpdateImageReference();
    }

    /// <summary>
    /// Cek apakah player sudah memiliki NPC Monster
    /// </summary>
    private void CheckNPCOwnership()
    {
        if (SaveSystem.PlayerConfig == null)
        {
            Debug.LogWarning("NPCIdleFlower: PlayerConfig is null!");
            _hasAnyNPC = false;
            _hasSecondNPC = false;
            return;
        }

        // Cek di ownedNPCMonsters
        var npcDatas = SaveSystem.PlayerConfig.ownedNPCMonsters;

        // Cek apakah player punya NPC sama sekali
        _hasAnyNPC = npcDatas != null && npcDatas.Count > 0;

        if (!_hasAnyNPC)
        {
            Debug.Log("NPCIdleFlower: Player doesn't have any NPC yet");
            _hasSecondNPC = false;
            return;
        }

        // Cek apakah player punya NPC ke-2
        var npcData = npcDatas.Find(npc => npc.monsterId == npcMonster2Id);

        if (npcData != null)
        {
            _hasSecondNPC = true;
            Debug.Log($"NPCIdleFlower: Player has second NPC ({npcMonster2Id})");
        }
        else
        {
            _hasSecondNPC = false;
            Debug.Log($"NPCIdleFlower: Player has {npcDatas.Count} NPC(s) but not second NPC yet");
        }
    }

    /// <summary>
    /// Update image reference berdasarkan ownership NPC
    /// </summary>
    private void UpdateImageReference()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("NPCIdleFlower: Target image is not assigned!");
            return;
        }

        // Jika player tidak punya NPC sama sekali, set sprite null
        if (!_hasAnyNPC)
        {
            targetImage.sprite = null;
            targetImage.enabled = false; // Optional: disable image component
            Debug.Log("NPCIdleFlower: Image set to null (no NPC owned)");
            return;
        }

        // Jika player punya NPC ke-2, gunakan imageSource2
        if (_hasSecondNPC && imageSource2 != null)
        {
            targetImage.sprite = imageSource2;
            targetImage.enabled = true;
            Debug.Log("NPCIdleFlower: Image changed to source 2");
        }
        // Jika player hanya punya 1 NPC, gunakan imageSource1
        else if (imageSource1 != null)
        {
            targetImage.sprite = imageSource1;
            targetImage.enabled = true;
            Debug.Log("NPCIdleFlower: Image set to source 1");
        }
    }

    /// <summary>
    /// Get idle station berdasarkan index NPC
    /// IdleStation1 hanya dapat diakses jika player sudah punya NPC ke-2
    /// </summary>
    /// <param name="npcIndex">0 untuk NPC pertama, 1 untuk NPC kedua</param>
    /// <returns>GameObject idle station atau null jika tidak tersedia</returns>
    public GameObject GetIdleStation(int npcIndex)
    {
        if (npcIndex == 1)
        {
            // IdleStation1 hanya bisa diakses jika player punya NPC ke-2
            if (IdleStation1 != null)
            {
                return IdleStation1.gameObject;
            }
            else
            {
                Debug.LogWarning($"NPCIdleFlower: IdleStation1 not available. Player has second NPC: {_hasSecondNPC}");
                return null;
            }
        }
        else if (npcIndex == 0)
        {
            // IdleStation2 selalu tersedia untuk NPC pertama
            return IdleStation2 != null ? IdleStation2.gameObject : null;
        }

        Debug.LogWarning($"NPCIdleFlower: Invalid NPC index {npcIndex}");
        return null;
    }

    /// <summary>
    /// Public method untuk refresh ownership check (dipanggil dari luar jika ada perubahan inventory)
    /// </summary>
    public void RefreshNPCOwnership()
    {
        CheckNPCOwnership();
        UpdateImageReference();
    }
}