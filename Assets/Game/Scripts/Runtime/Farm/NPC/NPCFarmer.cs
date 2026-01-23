using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Farm;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
namespace MagicalGarden.AI
{
    public class NPCFarmer : BaseEntityAI
    {
        protected INPCCheckAreaPoisition service;

         public Vector2Int destinationTile;
        [Header ("NPC Settings")]
        public NPCAreaPointsDatabaseSO npcAreaPointsDatabase;
        public int [] codeNpcAreaPoints;
        public float checkAreaPositionMinSeconds;
        public float checkAreaPositionMaxSeconds;
        public float changeAreaPointsMinSeconds;
        public float changeAreaPointsMaxSeconds;
        [HideInInspector] public NPCAreaPointsSO currentNPCAreaPointsSO;

        [Header ("Farmer Variables")]
        public PlantManager plantManager;
        [SerializeField] bool isWatering = false;
        bool isHarvesting = false;
        Coroutine cnService;
        public enum NearestTargetType
        {
            None,
            Water,
            Harvest,
        }

        [Header ("Navigation")]
        public TileAIType tileTargetType = TileAIType.FarmPlant; // TileType is in TileManager.cs
        public Tilemap tileTarget;
        Vector3Int nearestCell;
        PlantController nearestPlant;
        protected override IEnumerator HandleState(string stateName)
        {
            switch (stateName)
            {
                case "idle": return IdleState();
                case "walk": return WalkState();
                case "run":  return RunState();
                case "watering" : return WateringState ();
               // case "fertilizing" : return FertilizingState ();
                case "collect": return HarvestingState ();
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
            tileTarget = TileManager.Instance.GetTilemap (tileTargetType);
            stateLoopCoroutine = StartCoroutine(StateLoop());
            Patroling ();
            
           // StartCoroutine (nTestWalk ());
        }

        void OnDestroy () {
           // hotelGiftHandler.RemoveNPCService (this);
          //  HotelManager.Instance.RemoveNPCService (this);
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
                Debug.LogError("Destination is not walkable!");
               // yield break;
            }

            List<Vector2Int> path = FindPathNearest(currentTile, destination);
            if (path == null || path.Count < 2)
            {
                isWatering = false;
                RemovePlantControllerNPCTargeting();
                Debug.LogError("No valid path found! Posisi terlalu nempel" + destination);
                isOverridingState = false;
                stateLoopCoroutine = StartCoroutine(StateLoop());
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
            if (isWatering) {
                cnService = StartCoroutine (WateringState ());
            } else if (isHarvesting) {
                cnService = StartCoroutine (HarvestingState ());
            } else {
                isOverridingState = false;
                stateLoopCoroutine = StartCoroutine(StateLoop());
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
        
        #region NPC Movement

        void Patroling () {
            currentNPCAreaPointsSO = npcAreaPointsDatabase.GetRandomNPCAreaPointsSO ();
            StartCoroutine (nChangeAreaPoints ());
            StartCoroutine (nCheckAreaPosition ());
        }

        public virtual IEnumerator nCheckAreaPosition () {
           // isOverridingState = true;
            if (!plantManager) {
                yield return new WaitUntil (() => PlantManager.Instance);
                plantManager = PlantManager.Instance;
            }

            if (stateLoopCoroutine != null) { // Farm Features
                StopCoroutine (stateLoopCoroutine);
                stateLoopCoroutine = null;
                isOverridingState = false;
            }

            if (IsCanWatering() || IsCanHarvesting ())
            {
                Dictionary<Vector3Int, PlantController> dictionaryWaterAvailables = GetPlantsWatering();
                Dictionary<Vector3Int, PlantController> dictionaryHarvestAvailables = GetPlantsHarvesting ();
                Transform origin = transform;

                float nearestDistance = float.MaxValue;
                nearestPlant = null;
                nearestCell = default;
                NearestTargetType nearestTarget = NearestTargetType.None;

                // Focus on the Harvest first :
                foreach (KeyValuePair<Vector3Int, PlantController> kvp in dictionaryHarvestAvailables)
                {
                    PlantController plant = kvp.Value;
                    if (plant == null) continue;

                    // 🔴 SKIP jika sudah ada di listTargettingPlantControllers
                    if (plantManager.GetListTargettingPlantControllers ().Contains(plant))
                        continue;

                    float dist = Vector3.Distance(origin.position, plant.transform.position);

                    if (dist < nearestDistance)
                    {
                        nearestTarget = NearestTargetType.Harvest;
                        nearestDistance = dist;
                        nearestPlant = plant;
                        nearestCell = kvp.Key;
                    }
                }

                foreach (KeyValuePair<Vector3Int, PlantController> kvp in dictionaryWaterAvailables)
                {
                    PlantController plant = kvp.Value;
                    if (plant == null) continue;

                    // 🔴 SKIP jika sudah ada di listTargettingPlantControllers
                    if (plantManager.GetListTargettingPlantControllers ().Contains(plant))
                        continue;

                    float dist = Vector3.Distance(origin.position, plant.transform.position);

                    if (dist < nearestDistance)
                    {
                        nearestTarget = NearestTargetType.Water;
                        nearestDistance = dist;
                        nearestPlant = plant;
                        nearestCell = kvp.Key;
                    }
                }

                

                if (nearestPlant != null)
                {

                    Debug.Log($"Nearest plant at {nearestCell}, distance {nearestDistance}");
                    if (nearestTarget == NearestTargetType.Water) {
                        isWatering = true;

                        // simpan agar tidak dipilih NPC lain / loop berikutnya
                        AddPlantControllerNPCTargeting ();

                        StartNewCoroutine(
                            MoveToTarget(DetectTiles(nearestCell))
                        );
                    } else if (nearestTarget == NearestTargetType.Harvest) {
                        isHarvesting = true;

                        // simpan agar tidak dipilih NPC lain / loop berikutnya
                        AddPlantControllerNPCTargeting ();

                        StartNewCoroutine(
                            MoveToTarget(DetectTiles(nearestCell))
                        );
                    }
                }
                else
                {
                    // 🟡 Tidak ada plant valid selain yang sudah ditarget
                  //  Debug.Log("Stay it");
                  //  isWatering = false;
                    StartNewCoroutine(
                    MoveToTarget(
                        currentNPCAreaPointsSO.areaPositions[
                            UnityEngine.Random.Range(0, currentNPCAreaPointsSO.areaPositions.Length)
                        ]
                    )
                    );
                }
            }
            else
            {
                StartNewCoroutine(
                    MoveToTarget(
                        currentNPCAreaPointsSO.areaPositions[
                            UnityEngine.Random.Range(0, currentNPCAreaPointsSO.areaPositions.Length)
                        ]
                    )
                );
            }

            yield return new WaitUntil (()=> !isWatering);
            yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (UnityEngine.Random.Range (checkAreaPositionMinSeconds, checkAreaPositionMaxSeconds));
            StartCoroutine (nCheckAreaPosition ());
        }

        public virtual IEnumerator nChangeAreaPoints () {
            yield return new WaitUntil (()=> !isWatering);
             yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (UnityEngine.Random.Range (changeAreaPointsMinSeconds, changeAreaPointsMaxSeconds));
            
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

        /*
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
        */
        #endregion
        
        #region Reset
        /*
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
        */
        #endregion
        
        #region Farm Features

        // ------------------ Watering
        bool IsCanWatering () {
            Debug.Log ("Watering " + plantManager.GetPlantsAvailableWater ().Count);
            return plantManager.GetPlantsAvailableWater ().Count > 0;
        }

        Dictionary<Vector3Int, PlantController> GetPlantsWatering () {
            return plantManager.GetPlantsAvailableWater ();
        }

       IEnumerator WateringState()
        {
            lockFlipTarget = true;
            FlipFarmer ();

            SetAnimation("watering");

            yield return new WaitForSeconds(1.5f);
            lockFlipTarget = false;
            SetAnimation("idle");
            isWatering = false;
            isOverridingState = false;
            TileManager.Instance.NPCWateringTiles(nearestCell);
            stateLoopCoroutine = StartCoroutine(StateLoop()); // ini coba dinyalakan dulu
            RemovePlantControllerNPCTargeting();
        }
        
        

        // ----------------- Harvesting
        bool IsCanHarvesting () {
            return plantManager.GetPlantsAvailableHarvestExceptSeedMonster ().Count > 0;
        }

        Dictionary<Vector3Int, PlantController> GetPlantsHarvesting () {
            return plantManager.GetPlantsAvailableHarvestExceptSeedMonster ();
        }

        IEnumerator HarvestingState()
        {
            lockFlipTarget = true;
            FlipFarmer ();

            SetAnimation("collect");

            yield return new WaitForSeconds(1.5f);
            lockFlipTarget = false;
            SetAnimation("idle");
            isHarvesting = false;
            isOverridingState = false;
            if (PlantManager.Instance.IsCanHarvest (nearestCell)) {
                PlantManager.Instance.HarvestAt(nearestCell);
            }
            stateLoopCoroutine = StartCoroutine(StateLoop()); // ini coba dinyalakan dulu
            RemovePlantControllerNPCTargeting();
        }
        //-------------------------- Plant Controller
        void AddPlantControllerNPCTargeting () {
            plantManager.AddPlantControllerNPCTargeting (nearestPlant);
        }

        void RemovePlantControllerNPCTargeting () {
            plantManager.RemovePlantControllerNPCTargeting (nearestPlant);
        }
        //---------------------------- Flip Rotation
        void FlipFarmer () {
            Vector2Int npcTile = (Vector2Int)terrainTilemap.WorldToCell(transform.position);
            Vector3Int targetCell = nearestCell;
            Vector2Int targetTile = new Vector2Int(targetCell.x, targetCell.y);


            float faceScaleX;
            int dir;
            Debug.Log ($"{npcTile} target {targetTile}");
            // KASUS 1: X sama → cek Y
            if (npcTile.x == targetTile.x)
            {
                if (npcTile.y > targetTile.y)
                {
                    faceScaleX = -1f;   // kanan
                    dir = 1;
                    Debug.Log ("Kanan");
                }
                else
                {
                    faceScaleX = 1f;  // kiri
                    dir = -1;
                    Debug.Log ("Kiri");
                }
            }
            // KASUS 2: Y sama → cek X
            else if (npcTile.y == targetTile.y)
            {
                if (npcTile.x < targetTile.x)
                {
                    faceScaleX = -1f;   // kanan
                    dir = 1;
                    Debug.Log ("Kanan");
                }
                else
                {
                    faceScaleX = 1f;  // kiri
                    dir = -1;
                    Debug.Log ("Kiri");
                }
            }
            // KASUS 3: diagonal → pakai rule OR
            else if (npcTile.x <= targetTile.x || npcTile.y >= targetTile.y)
            {
                faceScaleX = -1f;   // kanan
                dir = 1;
                Debug.Log ("Kanan");
            }
            else
            {
                faceScaleX = 1f;  // kiri
                dir = -1;
                Debug.Log ("Kiri");
            }

            skeleton.skeleton.ScaleX = faceScaleX;
            lastDirection = dir;
        }
        #endregion
        #region Converter Vector2
        Vector2Int DetectTiles(Vector3Int tilePos)
        {
            if (tileTarget == null)
            {
                Debug.LogError("Tilemap belum di-assign!");
                return Vector2Int.zero;
            }

            // Optional: cek apakah tile benar-benar ada
            if (!tileTarget.HasTile(tilePos))
            {
                Debug.LogWarning("Tidak ada tile di posisi: " + tilePos);
                return Vector2Int.zero;
            }

            return new Vector2Int(tilePos.x, tilePos.y);
        }

        #endregion
    }
}
