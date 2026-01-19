using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;
using System;

namespace MagicalGarden.AI
{
    public class NPCService : BaseEntityAI, INPCHotelService, INPCCheckAreaPoisition {
        protected INPCCheckAreaPoisition service;

         public Vector2Int destinationTile;
        [Header ("NPC Robo Shroom")]
        public HotelRequestDetector hotelRequestDetector;
        public NPCAreaPointsDatabaseSO npcAreaPointsDatabase;
        public int [] codeNpcAreaPoints;
        public float checkAreaPositionMinSeconds;
        public float checkAreaPositionMaxSeconds;
        public float changeAreaPointsMinSeconds;
        public float changeAreaPointsMaxSeconds;

        [Header ("Service Features")]
        // public HotelController hotelControlRef;
        public HotelController hotelControlRef { get; set; }
        public Action<int> finishEvent;
        int finishEventValue;

        public Action rewardEvent;
        [HideInInspector] public bool isServingRoom = false;
        
        [HideInInspector] public NPCAreaPointsSO currentNPCAreaPointsSO;
        [HideInInspector] public bool isCollectingGift;
        [HideInInspector] public GameObject giftObject;

        public enum NearestTargetType
        {
            None,
            Gift,
            Hotel,
            Reward,
        }

        [Header ("Reset")]
        HotelGiftHandler hotelGiftHandler;
        [HideInInspector] public Coroutine cnCollectGiftState;
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
        public virtual void Awake()
        {
            service = this; // 🔥 INI KUNCI UTAMANYA
        }

        void Start()
        {
            base.Start();
            stateLoopCoroutine = StartCoroutine(StateLoop());
            PatrolingRobo ();
            hotelGiftHandler = HotelGiftHandler.instance;
            hotelGiftHandler.AddNPCService (this);
            HotelManager.Instance.AddNPCService (this);
           // StartCoroutine (nTestWalk ());
        }

        void OnDestroy () {
            hotelGiftHandler.RemoveNPCService (this);
            HotelManager.Instance.RemoveNPCService (this);
        }

        /*
        IEnumerator nTestWalk () {
            yield return new WaitForSeconds (3f);
            StartCoroutine (MoveToTarget(new Vector2Int(94,-45)));
        }
        */

        public IEnumerator MoveToTarget(Vector2Int destination, bool walkOnly = false)
        {
            
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
            

            if (!isCollectingGift && !isServingRoom) {
                isOverridingState = false;
                stateLoopCoroutine = StartCoroutine(StateLoop());
                
            } else if (isCollectingGift) {
                isOverridingState = false;
                StartCoroutine (CollectState ());
                
            } else if (isServingRoom) {
                GetComponent<MeshRenderer>().enabled = false;
                stateLoopCoroutine = StartCoroutine(CleaningRoutine());
            }
            
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
            currentNPCAreaPointsSO = npcAreaPointsDatabase.GetRandomNPCAreaPointsSO ();
            StartCoroutine (service.nChangeAreaPoints ());
            StartCoroutine (service.nCheckAreaPosition ());
        }

        public virtual IEnumerator nCheckAreaPosition () {
            yield break;
        }

        public virtual IEnumerator nChangeAreaPoints () {
            yield break;
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

        #region Reset
        
        public void ResetMovement () { // HotelGiftHandler
            if (cnCollectGiftState != null) {
                StopCoroutine (cnCollectGiftState);
                cnCollectGiftState =null;
            }

            isCollectingGift = false;
            isOverridingState = false;

            SetAnimation ("idle");
           // stateLoopCoroutine = StartCoroutine(StateLoop());
        }

        public void ResetMovementHotel (HotelController hotelController) { // HotelManager.cs (Mencegah bug stuck di isServingRoom)
            
            if (isServingRoom) {
                if (hotelController == hotelControlRef) {
                if (stateLoopCoroutine != null) {StopCoroutine(stateLoopCoroutine);}
                isServingRoom = false;
                isOverridingState = false;
                stateLoopCoroutine = StartCoroutine(StateLoop());
                hotelControlRef = null;
                }
            }
        }

        #endregion
        
        #region Service Features
        public void AddFinishEventHappiness(Action<int> callback, int value)
        {
            ClearAllEvent ();      // clear semua listener sebelumnya
            finishEvent += callback; // tambah listener baru
            finishEventValue = value;
        }

        public void AddRewardEvent(Action callback)
        {
            ClearAllEvent ();// clear semua listener sebelumnya
            rewardEvent += callback; // tambah listener baru
        }

        void ClearAllEvent () {
            finishEvent = null;
            rewardEvent = null;
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
           // HotelManager.Instance.CallCleaningVFX(hotelControlRef.dustPos);
           if (finishEvent != null) { 
            hotelControlRef.InstantiateVfxDust ();
           }
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
            // HotelManager.Instance.DestroyCleaningVFX(hotelControlRef.rayPos);

            Debug.Log($"✅ [NPC CLEANING COMPLETE] Kamar '{hotelName}' sudah bersih | Tamu: {guestName}");

            yield return new WaitForSeconds(2);
            GetComponent<MeshRenderer>().enabled = true;
            finishEvent?.Invoke(finishEventValue);
            rewardEvent?.Invoke ();
            // 5. Lanjut wander 
            isServingRoom = false;
            isOverridingState = false;
            stateLoopCoroutine = StartCoroutine(StateLoop());
        }

        #endregion
        
        
    }
}
