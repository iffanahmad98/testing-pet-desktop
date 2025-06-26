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
        protected override IEnumerator CustomState(string stateName)
        {
            switch (stateName)
            {
                // case "RunToPoint": return MoveToTarget(destinationTile);
                // case "Wander": return WanderRoutine();
                default: return base.CustomState(stateName);
            }

            // return base.CustomState(stateName);
        }

        public void RunIdle()
        { 
            StartNewCoroutine(IdleState());
        }

        void Start()
        {
            base.Start();
            // stateLoopCoroutine = StartCoroutine(StateLoop());
        }


        private IEnumerator SetupPetHotelRoutine()
        {
            yield return new WaitForSeconds(1f); // ← Delay 1.5 detik sebelum mulai
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            Vector2Int? destinationOpt = hotelRoomRef.GetRandomWanderingTile2D();
            if (!destinationOpt.HasValue)
            {
                Debug.LogWarning("🚫 Tidak ada wandering tile tersedia!");
                StartNewCoroutine(MoveToTarget(destinationTile));
                yield break;
            }

            Vector2Int destination = destinationOpt.Value;
            StartNewCoroutine(MoveToTarget(destination));
        }
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
        public IEnumerator MoveToTarget(Vector2Int destination, bool walkOnly = false)
        {
            if (!IsWalkableTile(destination))
            {
                Debug.LogError("Destination is not walkable!");
                yield break;
            }

            List<Vector2Int> path = FindPath(currentTile, destination);
            if (path == null || path.Count < 2)
            {
                Debug.LogWarning("No valid path found!");
                yield break;
            }

            // Debug: gambarkan path di scene
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 wp1 = GridToWorld(path[i]);
                Vector3 wp2 = GridToWorld(path[i + 1]);
            }

            isOverridingState = true;
            SetAnimation(walkOnly ? "walking" : "running");

            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int next = path[i];
                Vector3 rawTargetPos = GridToWorld(next);
                Vector3 targetPos = new Vector3(rawTargetPos.x, rawTargetPos.y, transform.position.z);
                Vector2Int direction = next - currentTile;
                Debug.Log($"[Step {i}] currentTile: {currentTile}, next: {next}, direction: {direction}");
                FlipByTarget(transform.position, targetPos);

                if (!walkOnly && (next - currentTile).magnitude > 1.5f)
                {
                    SetAnimation("jumping");
                    yield return JumpToTile(next);
                    yield return new WaitForSeconds(0.5f);
                    SetAnimation("running");
                }
                else
                {

                    while (Vector3.Distance(transform.position, targetPos) > 0.1f)
                    {
                        float speed = walkOnly ? walkSpeed : runSpeed;

                        // Simpan posisi sebelumnya
                        Vector3 prevPos = transform.position;

                        // Gerakkan karakter
                        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

                        // Debug: Garis dari posisi sebelumnya ke sekarang (arah gerakan)
                        Debug.DrawLine(prevPos, transform.position, Color.red); // hanya tampil 1 frame (0.1s)

                        Debug.Log(
                            $"Moving to Step {i}: current={transform.position}, target={targetPos}, " +
                            $"dist={Vector3.Distance(transform.position, targetPos):F4}, speed={speed:F2}"
                        );

                        yield return null;
                    }
                }

                transform.position = targetPos;
                currentTile = next;
            }

            SetAnimation("idle");
            isOverridingState = false;
            StartNewCoroutine(StateLoop());
        }
        private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            AStarPathfinder pathfinder = new AStarPathfinder(IsWalkableTile);
            var path = pathfinder.FindPath(start, end);

            if (path == null)
            {
                Debug.LogWarning("No valid path found!");
                return null;
            }

            // Validate path is actually walkable
            for (int i = 0; i < path.Count; i++)
            {
                if (!IsWalkableTile(path[i]))
                {
                    Debug.LogWarning($"Path contains non-walkable tile at {path[i]}");
                    return null;
                }
            }

            return path;
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
        
    }
}
