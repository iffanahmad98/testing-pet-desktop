using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;

namespace MagicalGarden.AI
{
    public class NPCRoboShroom : BaseEntityAI
    {
        public Vector2Int destinationTile;
        [Header ("NPC Robo Shroom")]
        public HotelRequestDetector hotelRequestDetector;
        public NPCAreaPointsDatabaseSO npcAreaPointsDatabase;
        public int [] codeNpcAreaPoints;
        public float checkAreaPositionMinSeconds;
        public float checkAreaPositionMaxSeconds;
        public float changeAreaPointsMinSeconds;
        public float changeAreaPointsMaxSeconds;

        NPCAreaPointsSO currentNPCAreaPointsSO;
        bool isCollectingGift;
        GameObject giftObject;


        protected override IEnumerator HandleState(string stateName)
        {
            switch (stateName)
            {
                case "idle": return IdleState();
                case "walk": return WalkState();
                case "run":  return RunState();
                case "collect": return CollectState ();
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
            PatrolingRobo ();
           // StartCoroutine (nTestWalk ());
        }

        /*
        IEnumerator nTestWalk () {
            yield return new WaitForSeconds (3f);
            StartCoroutine (MoveToTarget(new Vector2Int(94,-45)));
        }
        */

        public IEnumerator MoveToTarget(Vector2Int destination, bool walkOnly = false)
        {
            
            Debug.Log ("Loot Hunter Destination " + destination);

            if (!IsWalkableTile(destination))
            {
              //  Debug.LogError("Destination is not walkable!");
               // yield break;
            }

            List<Vector2Int> path = FindPathNearest(currentTile, destination);
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
              //  Debug.Log($"[Step {i}] currentTile: {currentTile}, next: {next}, direction: {direction}");
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
                        /*
                        Debug.Log(
                            $"Moving to Step {i}: current={transform.position}, target={targetPos}, " +
                            $"dist={Vector3.Distance(transform.position, targetPos):F4}, speed={speed:F2}"
                        );
                        */
                        yield return null;
                    }
                }

                transform.position = targetPos;
                currentTile = next;
            }

            SetAnimation("idle");
            isOverridingState = false;

            if (!isCollectingGift) {
                stateLoopCoroutine = StartCoroutine(StateLoop());
            } else {
                StartCoroutine (CollectState ());
            }
           // GetComponent<MeshRenderer>().enabled = false;
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
        
        #region NPCRoboShroom
        void PatrolingRobo () {
            StartCoroutine (nChangeAreaPoints ());
            StartCoroutine (nCheckAreaPosition ());
        }

        IEnumerator nCheckAreaPosition () {
            StopCoroutine (stateLoopCoroutine);
            if (hotelRequestDetector.IsHasHotelRequest ()) {
                var hotelController = hotelRequestDetector.GetRandomHotelRequest ();
                Debug.Log ("Hotel Robo Target Position : " + hotelController.gameObject.name);
            } 
            else if (HotelGiftHandler.instance.IsAnyGifts ()) {
               // Vector2Int lootPosition = hotelFacilitiesLootDetector.GetRandomLootPosition ();
                var result = HotelGiftHandler.instance.GetRandomGiftPosition();
                Vector2Int lootPosition = result.pos;
                giftObject = result.obj;

                isCollectingGift = true;
                StartNewCoroutine (MoveToTarget (lootPosition));
               // Debug.Log ("Gift Position " + lootPosition);
                
            } else {
                StartNewCoroutine (MoveToTarget (currentNPCAreaPointsSO.areaPositions[Random.Range (0, currentNPCAreaPointsSO.areaPositions.Length)]));
            }
          //  isOverridingState = true;
             yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (Random.Range (checkAreaPositionMinSeconds, checkAreaPositionMaxSeconds));
            StartCoroutine (nCheckAreaPosition ());
        }

        IEnumerator nChangeAreaPoints () {
            currentNPCAreaPointsSO = npcAreaPointsDatabase.GetRandomNPCAreaPointsSO ();
           // Debug.Log ("Current NPC Area Point : " + currentNPCAreaPointsSO);
            yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (Random.Range (changeAreaPointsMinSeconds, changeAreaPointsMaxSeconds));
            
            StartCoroutine (nChangeAreaPoints ());
        }

        private List<Vector2Int> FindPathNearest(Vector2Int start, Vector2Int end)
        {
            // Jika end tidak walkable → cari yang terdekat
            if (!IsWalkableTile(end))
            {
                Vector2Int newEnd = GetNearestWalkableTile(end);
                Debug.Log($"End tile not walkable, using nearest tile: {newEnd}");
                end = newEnd;
            }

            AStarPathfinder pathfinder = new AStarPathfinder(IsWalkableTile);
            var path = pathfinder.FindPath(start, end);

            if (path == null)
            {
                Debug.LogWarning("No valid path found!");
                return null;
            }

            // Validate
            foreach (var p in path)
            {
                if (!IsWalkableTile(p))
                {
                    Debug.LogWarning($"Path contains non-walkable tile at {p}");
                    return null;
                }
            }

            return path;
        }

        private Vector2Int GetNearestWalkableTile(Vector2Int target)
        {
            // Jika sudah walkable → langsung return
            if (IsWalkableTile(target))
                return target;

            // Arah pergerakan (4 arah + 4 diagonal)
            Vector2Int[] dirs = new Vector2Int[]
            {
                new Vector2Int( 1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 0,-1),
                new Vector2Int( 1, 1),
                new Vector2Int( 1,-1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1,-1),
            };

            Queue<Vector2Int> q = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            q.Enqueue(target);
            visited.Add(target);

            while (q.Count > 0)
            {
                var current = q.Dequeue();

                foreach (var dir in dirs)
                {
                    Vector2Int next = current + dir;

                    if (visited.Contains(next))
                        continue;

                    visited.Add(next);
                    q.Enqueue(next);

                    if (IsWalkableTile(next))
                    {
                        // Ketemu tile walkable terdekat!
                        return next;
                    }
                }
            }

            // Kalau tidak ada walkable sama sekali (jarang terjadi)
            Debug.LogError ("Tidak menemukan tile walkable terdekat!");
            return target;
        }

        protected virtual IEnumerator CollectState()
        { 
           // skeleton.skeleton.ScaleX = lastDirection == 1 ? -1f : 1f;
         //  Debug.Log ("Collecting Animation");
            SetAnimation("collect");
             
            yield return new WaitForSeconds(1.33f);
            isCollectingGift = false;
            stateLoopCoroutine = StartCoroutine(StateLoop());
          //  hotelRandomLoot.GetTicketFromNPC (this.gameObject, giftObject);
            giftObject.GetComponent <MagicalGarden.Gift.GiftItem> ().OpenGiftByNPC ();
            giftObject = null;
        }
        #endregion
    }

    
}
