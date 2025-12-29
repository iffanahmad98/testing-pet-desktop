using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
namespace MagicalGarden.AI
{
    public class PetMonsterHotel : BaseEntityAI
    {
        public Vector2Int destinationTile;
        public HotelRoom hotelRoomRef;
        public HotelController hotelContrRef;
        private bool hasJumped = false;
        private bool isMoving = false;
        public System.Action finishMoveEvent;

        [Header ("Data (PlayerConfig.cs)")]
        public PlayerConfig playerConfig;
        PetMonsterHotelData petMonsterHotelData;
        Tilemap hotelTilemap;

        

        protected override IEnumerator CustomState(string stateName)
        {
            switch (stateName)
            {
                case "itching":
                    yield return ItchingState();
                    break;
                case "wander":
                    yield return WanderRoutine();
                    break;
                case "wander run":
                    yield return WanderRoutine(false);
                    break;
                case "gotoroom":
                    if (hotelContrRef != null)
                    {
                        Vector2Int roomPos = new Vector2Int(hotelContrRef.hotelPositionTile.x, hotelContrRef.hotelPositionTile.y);
                        yield return MoveAndHideRoutine(roomPos, walkOnly: false);
                    }
                    break;
                // case "idlefront":
                //     if (hotelRoomRef != null)
                //     {
                //         transform.position = GridToWorld(hotelRoomRef.hotelPosition);
                //         yield return IdleState();
                //     }
                //     break;
                default:
                    yield return base.CustomState(stateName);
                    break;
            }
        }

        void OnMouseDown()
        {
            if (!hasJumped && !isMoving)
            {

                // 🔀 Random 0 atau 1 untuk pilih antara jump atau itch
                if (Random.value < 0.5f)
                {
                    StartNewCoroutine(JumpState());
                }
                else
                {
                    StartNewCoroutine(ItchingState());
                }
            }
        }

        protected virtual IEnumerator JumpState()
        {
            hasJumped = true;
            SetAnimation("jumping");
            yield return new WaitForSeconds(1f);

            // reset jump (jika ingin bisa lompat lagi setelah delay)
            hasJumped = false;
            yield return new WaitForSeconds(1f);
            StartNewCoroutine(StateLoop());
        }

        protected virtual IEnumerator ItchingState()
        {
            hasJumped = true;
            SetAnimation("itching");
            yield return new WaitForSeconds(1f);

            // reset jump (jika ingin bisa lompat lagi setelah delay)
            hasJumped = false;
            yield return new WaitForSeconds(1f);
            StartNewCoroutine(StateLoop());
        }
        //idle for crack egg
        public void RunIdle()
        {
            StartNewCoroutine(IdleState());
        }
        void Awake()
        {
            if (GetComponent<Collider2D>() == null)
                gameObject.AddComponent<BoxCollider2D>();
        }

        void Start () {
            base.Start ();
            
            playerConfig = SaveSystem.PlayerConfig;
        }



        private IEnumerator SetupPetHotelRoutine()
        {
            float delay = Random.Range(1f, 4f); // ← Delay antara 1 hingga 4 detik
            yield return new WaitForSeconds(delay);
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            Vector2Int? destinationOpt = hotelContrRef.GetRandomWanderingTile2D();
            if (!destinationOpt.HasValue)
            {
                Debug.LogWarning("🚫 Tidak ada wandering tile tersedia!");
                StartNewCoroutine(MoveToTarget(destinationTile));
                yield break;
            }

            Vector2Int destination = destinationOpt.Value;
            StartNewCoroutine(MoveToTargetWithFlag(destination));
        }

        private IEnumerator MoveToTargetWithFlag(Vector2Int target, bool walkOnly = false)
        {
            isMoving = true;
            Debug.Log("▶️ Mulai gerak ke " + target);

            yield return MoveToTarget(target, walkOnly, true, success =>
            {
                isMoving = false;
                if (hotelContrRef) {
                    hotelContrRef.SetIsPetReachedTarget (true);
                }
                finishMoveEvent?.Invoke ();
                Debug.Log("🛑 Hasil moveToTarget: " + success);
            });

            Debug.Log("🛑 Selesai gerak");
            
        }

        //wander routine get tile from hotel
        private IEnumerator WanderRoutine(bool _walkOnly = true)
        {
            if (hotelContrRef == null || hotelContrRef.wanderingTiles.Count == 0)
            {
                Debug.LogWarning("Wandering area not set!");
                yield break;
            }

            isOverridingState = true;

            // Pilih tile acak dari daftar
            Vector3Int randomTile = hotelContrRef.wanderingTiles[Random.Range(0, hotelContrRef.wanderingTiles.Count)];
            Vector2Int destination = new Vector2Int(randomTile.x, randomTile.y);

            if (!IsWalkableTile(destination))
            {
                yield return new WaitForSeconds(0.5f);
                isOverridingState = false;
                StartNewCoroutine(StateLoop());
                yield break;
            }

            bool result = false;
            yield return MoveToTarget(destination, _walkOnly, false, success => result = success);

            // Tunggu di tempat sebelum jalan lagi
            float idleTime = Random.Range(1.5f, 3f);
            yield return new WaitForSeconds(idleTime);

            isOverridingState = false;

            // Kembali ke loop normal
            StartNewCoroutine(StateLoop());
        }

        public IEnumerator MoveAndHideRoutine(Vector2Int target, bool walkOnly = false)
        {
            isMoving = true;
            // 1. Bergerak ke target dulu
            bool success = false;
            yield return MoveToTarget(target, walkOnly, continueStateLoop: false, onComplete: result => success = result);

            if (success)
            {
                // 2. Sembunyikan visual
                GetComponent<MeshRenderer>().enabled = false;
                // 3. Tunggu 10 detik
                yield return new WaitForSeconds(10f);

                // 4. Munculkan kembali visual
                GetComponent<MeshRenderer>().enabled = true;
            }

            isMoving = false;

            // 5. Kembali ke StateLoop
            Vector3Int randomTile = hotelContrRef.wanderingTiles[Random.Range(0, hotelContrRef.wanderingTiles.Count)];
            Vector2Int destination = new Vector2Int(randomTile.x, randomTile.y);
            yield return MoveToTarget(destination, walkOnly, continueStateLoop: true, onComplete: result => success = result);
        }

        protected override IEnumerator WalkState()
        {
            if (hotelRoomRef == null || hotelRoomRef.wanderingTiles.Count == 0)
            {
                Debug.LogWarning("WanderingTiles kosong di WalkState override!");
                yield return IdleState();
                yield break;
            }

            // Sync posisi tile sekarang
            Vector3Int tile = terrainTilemap.WorldToCell(transform.position);
            currentTile = new Vector2Int(tile.x, tile.y);

            // Pilih tujuan acak
            Vector3Int randomTile = hotelRoomRef.wanderingTiles[Random.Range(0, hotelRoomRef.wanderingTiles.Count)];
            Vector2Int destination = new Vector2Int(randomTile.x, randomTile.y);

            // Validasi tile
            if (!IsWalkableTile(destination))
            {
                Debug.LogWarning("Tile tujuan tidak bisa dilalui");
                yield return IdleState();
                yield break;
            }

            // Jalan ke tujuan seperti biasa tapi pelan (walkOnly = true)
            yield return MoveToTarget(destination, walkOnly: true);
        }
        
        
        [ContextMenu("test to target")]
        public void SetupPetHotel()
        {
            StartNewCoroutine(SetupPetHotelRoutine());
        }
    
        #region Goto Checkout hotel
        public void RunToTargetAndDisappear(Vector2Int targetTile)
        {
            StartNewCoroutine(RunToAndDestroyRoutine(targetTile));
        }

        private IEnumerator RunToAndDestroyRoutine(Vector2Int targetTile)
        {
            // Validasi dulu
            if (!IsWalkableTile(targetTile))
            {
                Debug.LogError("❌ Target tile tidak bisa dilalui!");
                yield break;
            }

            yield return MoveToTarget(targetTile, walkOnly: false, continueStateLoop: false);

            // Delay sebelum hilang (optional)
            yield return new WaitForSeconds(0.5f);

            // Debug.Log($"💨 Pet {name} sampai ke tujuan dan akan dihancurkan.");
            Destroy(gameObject);
        }
        #endregion

        
        public void MoveToTargetWithEvent (Vector2Int targetPosition, bool meshEnabled = true, System.Action action = null) { // HotelController.cs
            StartNewCoroutine (MoveToTargetWithFlag (targetPosition));
            GetComponent<MeshRenderer>().enabled = meshEnabled;
            finishMoveEvent = action;
        }

        public void DestroyPet () { // HotelController.cs
            Debug.Log ("Destroy Pet !");
            Destroy (this.gameObject);
        }

        #region Data
        public void SetPetMonsterHotelData (PetMonsterHotelData value) { // HotelManager.cs
        
            petMonsterHotelData = value;
            playerConfig.SavePetMonsterHotelElement (petMonsterHotelData);
            SaveSystem.SaveAll ();
            
        }

        public void LoadData (PetMonsterHotelData data) { // HotelManager.cs
            petMonsterHotelData = data;
            LoadEventSpawn ();
        }

        void LoadEventSpawn()
        {
            if (hotelTilemap == null) {
                hotelTilemap = TileManager.Instance.tilemapHotelFacilities;
            }
            
            if (hotelContrRef.wanderingTiles == null || hotelContrRef.wanderingTiles.Count == 0)
                return;

            Vector3Int randomTile = hotelContrRef.wanderingTiles[
                Random.Range(0, hotelContrRef.wanderingTiles.Count)
            ];

            // Ambil posisi dunia dari tile
            Vector3 worldPos = hotelTilemap.GetCellCenterWorld(randomTile);

            transform.position = worldPos;

            StartCoroutine (SetupPetHotelRoutine ());
            Debug.Log ("Spawn Pet Hotel : " + worldPos);
        }

        public void ClearData () { // HotelController.cs (when check out)
            playerConfig.RemovePetMonsterHotelData (petMonsterHotelData);
            SaveSystem.SaveAll ();
        }
        #endregion
    }
}
