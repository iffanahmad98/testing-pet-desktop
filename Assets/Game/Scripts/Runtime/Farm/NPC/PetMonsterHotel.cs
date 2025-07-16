using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;

namespace MagicalGarden.AI
{
    public class PetMonsterHotel : BaseEntityAI
    {
        public Vector2Int destinationTile;
        public HotelRoom hotelRoomRef;
        public HotelController hotelContrRef;
        protected override IEnumerator CustomState(string stateName)
        {
            switch (stateName)
            {
                default: return base.CustomState(stateName);
            }
        }

        public void RunIdle()
        {
            StartNewCoroutine(IdleState());
        }

        void Start()
        {
            base.Start();
        }


        private IEnumerator SetupPetHotelRoutine()
        {
            yield return new WaitForSeconds(1f); // ← Delay 1.5 detik sebelum mulai
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            Vector2Int? destinationOpt = hotelContrRef.GetRandomWanderingTile2D();
            if (!destinationOpt.HasValue)
            {
                Debug.LogWarning("🚫 Tidak ada wandering tile tersedia!");
                StartNewCoroutine(MoveToTarget(destinationTile));
                yield break;
            }

            Vector2Int destination = destinationOpt.Value;
            StartNewCoroutine(MoveToTarget(destination));
        }

        //wander routine get tile from hotel
        private IEnumerator WanderRoutine()
        {
            if (hotelRoomRef == null || hotelRoomRef.wanderingTiles.Count == 0)
            {
                Debug.LogWarning("Wandering area not set!");
                yield break;
            }

            isOverridingState = true;

            while (true)
            {
                // Pilih tile acak dari daftar
                Vector3Int randomTile = hotelRoomRef.wanderingTiles[Random.Range(0, hotelRoomRef.wanderingTiles.Count)];
                Vector2Int destination = new Vector2Int(randomTile.x, randomTile.y);

                if (!IsWalkableTile(destination))
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                // Jalankan movement
                yield return MoveToTarget(destination, walkOnly: true);

                // Tunggu di tempat sebelum jalan lagi
                float idleTime = Random.Range(1.5f, 3f);
                yield return new WaitForSeconds(idleTime);
            }
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
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            StartNewCoroutine(SetupPetHotelRoutine());
        }
        [ContextMenu("test to wander")]
        public void WanderTest()
        {
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            StartNewCoroutine(WanderRoutine());
        }

        #region Goto Checkout hotel
        public void RunToTargetAndDisappear(Vector2Int targetTile)
        {
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
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
    }
}
