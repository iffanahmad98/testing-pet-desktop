using UnityEngine;
using Spine.Unity;
using System.Collections;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
using MagicalGarden.Farm;
using System.Linq;

namespace MagicalGarden.AI
{
    public class NPCShoper : MonoBehaviour
    {
        public SkeletonAnimation skeleton;
        public float walkSpeed = 1f;
        public float runSpeed = 2f;
        public StateChance[] stateChances;
        [Header("UI")]
        [SerializeField] private GameObject popupUI;
        private Vector3 targetPosition;
        private string currentState = "";
        public Tilemap terrainTilemap;
        public Vector2Int currentTile;

        void Start()
        {
            Vector3 worldPos = transform.position;
            if (terrainTilemap == null)
            {
                terrainTilemap = TileManager.Instance.tilemapSoil;
            }
            Vector3Int cellPos = terrainTilemap.WorldToCell(worldPos);
            currentTile = new Vector2Int(cellPos.x, cellPos.y);
            StartCoroutine(StateLoop());
        }
        void OnMouseDown()
        {
            ShowPopup();
        }
        void ShowPopup()
        {
            popupUI.SetActive(true);
        }
        public void HidePopup()
        {
            popupUI.SetActive(false);
        }

        void NormalizeProbabilities()
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

        string GetRandomState()
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

        IEnumerator StateLoop()
        {
            while (true)
            {
                string chosen = GetRandomState();

                switch (chosen)
                {
                    case "idle": yield return IdleState(); break;
                    case "walk": yield return WalkState(); break;
                    case "run": yield return RunState(); break;
                    // case "eat": yield return EatState(); break;
                    case "jump": yield return JumpState(); break;
                }

                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }
        }
        private bool IsWalkableTile(Vector2Int tileCoord)
        {
            Vector3Int gridPos = new Vector3Int(tileCoord.x, tileCoord.y, 0);
            var tile = terrainTilemap.GetTile(gridPos);
            if (tile is CustomTile myTile)
            {
                return myTile.tileType == TileType.Walkable;
            }
            return false;
        }
        private Vector3 GridToWorld(Vector2Int tile)
        {
            return terrainTilemap.CellToWorld(new Vector3Int(tile.x, tile.y, 0)) + terrainTilemap.cellSize / 2;
        }

        IEnumerator IdleState()
        {
            skeleton.skeleton.ScaleX = lastDirection == 1 ? -1f : 1f;
            SetAnimation("idle");
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }

        IEnumerator WalkState()
        {
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
        int lastDirection = -1;
        void FlipByTarget(Vector3 currentPos, Vector3 targetPos)
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

        IEnumerator RunState()
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

        IEnumerator EatState()
        {
            SetAnimation("eating");
            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }

        IEnumerator JumpState()
        {
            SetAnimation("jumping");
            // Tambahkan efek lompat kecil (misal animasi lompat tempat)
            yield return new WaitForSeconds(1f);
        }

        void SetAnimation(string animName)
        {
            if (currentState == animName) return;
            currentState = animName;
            skeleton.AnimationState.SetAnimation(0, animName, true);
        }

        private bool IsJumpOverPossible(Vector2Int start, Vector2Int end)
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
        IEnumerator JumpToTile(Vector2Int targetTile)
        {
            SetAnimation("jumping");

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
        private bool IsPathClear(Vector2Int start, Vector2Int end)
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
    }
}