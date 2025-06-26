using UnityEngine;
using Spine.Unity;
using System.Collections;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
using MagicalGarden.Farm;
namespace MagicalGarden.AI
{
    public abstract class BaseEntityAI : MonoBehaviour
    {
        public SkeletonAnimation skeleton;
        public float walkSpeed = 1f;
        public float runSpeed = 2f;
        public StateChance[] stateChances;
        protected string currentState = "";
        protected Tilemap terrainTilemap;
        public Vector2Int currentTile;
        protected Coroutine stateLoopCoroutine;
        protected bool isOverridingState = false;

        protected Coroutine currentCoroutine;
        protected void StartNewCoroutine(IEnumerator routine)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            currentCoroutine = StartCoroutine(routine);
        }
        protected virtual void Start()
        {
            Vector3 worldPos = transform.position;
            terrainTilemap = TileManager.Instance.tilemapSoil;
            Vector3Int cellPos = terrainTilemap.WorldToCell(worldPos);
            currentTile = new Vector2Int(cellPos.x, cellPos.y);
        }
        protected IEnumerator StateLoop()
        {
            while (true)
            {
                if (isOverridingState)
                {
                    yield return null;
                    continue;
                }

                string chosen = GetRandomState();
                yield return HandleState(chosen);

                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }
        }
        protected virtual IEnumerator HandleState(string stateName)
        {
            switch (stateName)
            {
                case "idle": return IdleState();
                case "walk": return WalkState();
                case "run":  return RunState();
                case "jump": return JumpState();
                default:     return CustomState(stateName);
            }
        }
        protected virtual IEnumerator CustomState(string stateName)
        {
            Debug.LogWarning($"State {stateName} not handled.");
            return IdleState();
        }
        protected virtual IEnumerator IdleState()
        { 
            skeleton.skeleton.ScaleX = lastDirection == 1 ? -1f : 1f;
            SetAnimation("idle");
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
        protected virtual IEnumerator WalkState()
        {
            Vector3Int tile = terrainTilemap.WorldToCell(transform.position);
            currentTile = new Vector2Int(tile.x, tile.y);
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            Vector2Int chosenDir = directions[Random.Range(0, directions.Length)];
            Vector2Int targetTile = currentTile + chosenDir;
            if (!IsPathClear(currentTile, targetTile))
            {
                Debug.Log($"Blocked path: {currentTile} → {targetTile}");
                if (IsJumpOverPossible(currentTile, targetTile))
                {
                    yield return JumpToTile(targetTile); // ← Tambahkan coroutine ini
                    yield break;
                }

                yield return IdleState();
                yield break;
            }
            SetAnimation("walking");

            Vector3 targetPos = GridToWorld(targetTile);
            FlipByTarget(transform.position, targetPos);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;
            currentTile = targetTile;
            SetAnimation("idle");
        }
        protected virtual IEnumerator RunState()
        {
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            Vector2Int chosenDir = directions[Random.Range(0, directions.Length)];
            Vector2Int targetTile = currentTile + chosenDir * 2; // lari 2 tile

            if (!IsPathClear(currentTile, targetTile))
            {
                if (IsJumpOverPossible(currentTile, targetTile))
                {
                    yield return JumpToTile(targetTile); // ← Tambahkan coroutine ini
                    yield break;
                }

                yield return IdleState();
                yield break;
            }
            SetAnimation("running");

            Vector3 targetPos = GridToWorld(targetTile);
            FlipByTarget(transform.position, targetPos);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, runSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;
            currentTile = targetTile;
            SetAnimation("idle");
        }
        protected virtual IEnumerator JumpState()
        { 
            SetAnimation("jumping");
            yield return new WaitForSeconds(1f);
        }

        protected virtual string GetRandomState()
        {
            NormalizeProbabilities();
            float rand = Random.value; // menghasilkan angka antara 0.0 sampai 1.0
            float cumulative = 0f;

            foreach (var state in stateChances)
            {
                cumulative += state.probability;
                if (rand < cumulative)
                    return state.stateName;
            }

            // Fallback (jaga-jaga jika total < 1)
            return stateChances[stateChances.Length - 1].stateName;
        }
        protected virtual void NormalizeProbabilities()
        {
            float total = 0f;
            for (int i = 0; i < stateChances.Length; i++)
            {
                total += stateChances[i].probability;
            }

            if (total > 0f)
            {
                for (int i = 0; i < stateChances.Length; i++)
                {
                    stateChances[i].probability /= total;
                }
            }
        }
        protected virtual void SetAnimation(string animName)
        {
            if (currentState == animName) return;
            currentState = animName;
            skeleton.AnimationState.SetAnimation(0, animName, true);
        }
        protected virtual bool IsWalkableTile(Vector2Int tileCoord)
        {
            Vector3Int gridPos = new Vector3Int(tileCoord.x, tileCoord.y, 0);
            var tile = terrainTilemap.GetTile(gridPos);
            if (tile is CustomTile myTile)
            {
                return myTile.tileType == TileType.Walkable;
            }
            return false;
        }
        protected virtual bool IsPathClear(Vector2Int start, Vector2Int end)
        {
            Vector2Int direction = (end - start);
            int distance = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

            direction = new Vector2Int(
                direction.x == 0 ? 0 : direction.x / Mathf.Abs(direction.x),
                direction.y == 0 ? 0 : direction.y / Mathf.Abs(direction.y)
            );

            Vector2Int checkTile = start;

            for (int i = 1; i <= distance; i++)
            {
                checkTile += direction;
                if (!IsWalkableTile(checkTile))
                    return false;
            }

            return true;
        }
        protected virtual bool IsJumpOverPossible(Vector2Int start, Vector2Int end)
        {
            Vector2Int delta = end - start;

            // Hanya izinkan lompat 2 tile secara lurus (tidak diagonal)
            if ((Mathf.Abs(delta.x) == 2 && delta.y == 0) || (Mathf.Abs(delta.y) == 2 && delta.x == 0))
            {
                Vector2Int middle = start + new Vector2Int(delta.x / 2, delta.y / 2);

                return !IsWalkableTile(middle) && IsWalkableTile(end);
            }

            return false;
        }
        protected virtual IEnumerator JumpToTile(Vector2Int targetTile)
        {
            SetAnimation("jumping");
            yield return new WaitForSeconds(0.3f);

            Vector3 targetPos = GridToWorld(targetTile);
            FlipByTarget(transform.position, targetPos);

            float jumpDuration = 0.6f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                // Gerak linear + efek lompatan ke atas
                float height = Mathf.Sin(t * Mathf.PI) * 0.5f;
                transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * height;

                yield return null;
            }

            transform.position = targetPos;
            currentTile = targetTile;

            SetAnimation("idle");
        }
        protected virtual Vector3 GridToWorld(Vector2Int tile)
        {
            Vector3 worldPos = terrainTilemap.CellToWorld(new Vector3Int(tile.x, tile.y, 0)) + terrainTilemap.cellSize / 2;
            return new Vector3(worldPos.x, worldPos.y, 0f); // pastikan z = 0
        }
        protected virtual void FlipByTarget(Vector3 currentPos, Vector3 targetPos)
        {
            float deltaX = targetPos.x - currentPos.x;

            if (deltaX > 0.01f)
            {
                skeleton.skeleton.ScaleX = -1f; // kanan
                lastDirection = 1;
            }
            else if (deltaX < -0.01f)
            {
                skeleton.skeleton.ScaleX = 1f;  // kiri
                lastDirection = -1;
            }
            else
            {
                skeleton.skeleton.ScaleX = lastDirection == 1 ? -1f : 1f;
            }
        }

        protected int lastDirection = -1;
    }
    [System.Serializable]
    public struct StateChance
    {
        public string stateName;
        public float probability;
    }
}