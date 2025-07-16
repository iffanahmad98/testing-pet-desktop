using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using UnityEngine.Rendering;

namespace MagicalGarden.AI
{
    public class NPCFertilizer : BaseEntityAI
    {
        public Vector2Int destinationFertiMachine;
        public Vector2Int destinationGetOut;
        Coroutine currentCoroutine;
        void Start()
        {
            base.Start();
            stateLoopCoroutine = StartCoroutine(StateLoop());
        }

        public void StartMoveToTarget()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(NPCFertiMake());
        }

        public void StopMoveToTarget()
        {
            GetComponent<MeshRenderer>().enabled = true;
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            stateLoopCoroutine = StartCoroutine(MoveToTarget(new Vector2Int(destinationGetOut.x, destinationGetOut.y), false, true));
        }

        public IEnumerator NPCFertiMake()
        {
            yield return new WaitForSeconds(1f);
            if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
            stateLoopCoroutine = StartCoroutine(MoveToTarget(new Vector2Int(destinationFertiMachine.x, destinationFertiMachine.y)));
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
                // Debug.Log($"[Step {i}] currentTile: {currentTile}, next: {next}, direction: {direction}");
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
                        // Debug.DrawLine(prevPos, transform.position, Color.red); // hanya tampil 1 frame (0.1s)

                        // Debug.Log(
                        //     $"Moving to Step {i}: current={transform.position}, target={targetPos}, " +
                        //     $"dist={Vector3.Distance(transform.position, targetPos):F4}, speed={speed:F2}"
                        // );

                        yield return null;
                    }
                }

                transform.position = targetPos;
                currentTile = next;
            }

            SetAnimation("idle");
            isOverridingState = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}