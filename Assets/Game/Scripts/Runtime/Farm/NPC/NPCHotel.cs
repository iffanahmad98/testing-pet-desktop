using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;

namespace MagicalGarden.AI
{
    public class NPCHotel : BaseEntityAI
    {
        public Vector2Int destinationTile;
        public HotelController hotelControlRef;
        protected override IEnumerator HandleState(string stateName)
        {
            switch (stateName)
            {
                case "idle": return IdleState();
                case "walk": return WalkState();
                case "run":  return RunState();
                default:     return IdleState();
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            base.Start();
            stateLoopCoroutine = StartCoroutine(StateLoop());
        }


        public IEnumerator NPCHotelCleaning()
        {
            string guestName = hotelControlRef?.nameGuest ?? "No Guest";
            Debug.Log($"🚶 [NPC] Berjalan menuju kamar tamu '{guestName}' di posisi tile ({hotelControlRef.hotelPositionTile.x}, {hotelControlRef.hotelPositionTile.y})");

            yield return new WaitForSeconds(1f);
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            stateLoopCoroutine = StartCoroutine(MoveToTarget(new Vector2Int(hotelControlRef.hotelPositionTile.x, hotelControlRef.hotelPositionTile.y)));
        }

        public IEnumerator CleaningRoutine(float cleanDuration = 5f)
        {
            string guestName = hotelControlRef?.nameGuest ?? "No Guest";
            string hotelName = hotelControlRef?.gameObject.name ?? "Unknown Hotel";
            Debug.Log($"🧹 [NPC CLEANING START] Membersihkan kamar '{hotelName}' | Tamu: {guestName} | Durasi: {cleanDuration}s");

            // 2. Timer countdown (bisa sambil munculkan efek/animasi jika perlu)
            HotelManager.Instance.CallCleaningVFX(hotelControlRef.dustPos);
            float timer = 0f;
            while (timer < cleanDuration)
            {
                timer += Time.deltaTime;
                // (opsional: tambahkan efek partikel atau animasi di sini)
                yield return null;
            }

            // 3. NPC muncul kembali

            // 4. Ubah tile kamar menjadi bersih

            if (hotelControlRef != null)
            {
                hotelControlRef.SetClean(); // ubah tile ke bersih
            }
            HotelManager.Instance.DestroyCleaningVFX(hotelControlRef.rayPos);

            Debug.Log($"✅ [NPC CLEANING COMPLETE] Kamar '{hotelName}' sudah bersih | Tamu: {guestName}");

            yield return new WaitForSeconds(2);
            GetComponent<MeshRenderer>().enabled = true;

            // 5. Lanjut wander
            stateLoopCoroutine = StartCoroutine(StateLoop());
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
            GetComponent<MeshRenderer>().enabled = false;
            stateLoopCoroutine = StartCoroutine(CleaningRoutine());
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
        
    }
}
