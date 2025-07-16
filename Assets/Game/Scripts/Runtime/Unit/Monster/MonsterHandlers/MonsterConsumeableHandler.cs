using UnityEngine;
using System.Collections;

public class MonsterConsumableHandler
{
    private MonsterController _controller;
    private RectTransform _rectTransform;
    private float _detectionRangeSqr;
    private float _consumeDistanceSqr;
    private float _cachedConsumableDistanceSqr = float.MaxValue;
    private float _lastConsumeTime = 0f;
    private const float CONSUME_COOLDOWN = 3f;
    private Coroutine _consumeCoroutine;
    private bool _isInternallyConsuming = false;

    public IConsumable NearestConsumable { get; private set; }
    public bool IsNearConsumable { get; private set; }
    public bool IsCurrentlyConsuming => _isInternallyConsuming || (_controller.StateMachine?.CurrentState == MonsterState.Eating);

    public MonsterConsumableHandler(MonsterController controller, RectTransform rectTransform)
    {
        _controller = controller;
        _rectTransform = rectTransform;
        Initialize(controller.MonsterData);
    }

    public void Initialize(MonsterDataSO data)
    {
        _detectionRangeSqr = data.foodDetectionRange * data.foodDetectionRange;
        _consumeDistanceSqr = data.eatDistance * data.eatDistance;
    }

    public void FindNearestConsumable()
    {
        if (_isInternallyConsuming) return;
        if (Time.time - _lastConsumeTime < CONSUME_COOLDOWN) return;

        // Release previous claim
        ReleaseClaim();

        NearestConsumable = null;
        float closestSqr = float.MaxValue;
        Vector2 pos = _rectTransform.anchoredPosition;

        // Check food
        foreach (FoodController food in _controller.MonsterManager.activeFoods)
        {
            if (food == null) continue;
            Vector2 foodPos = food.GetComponent<RectTransform>().anchoredPosition;
            float sqrDist = (foodPos - pos).sqrMagnitude;

            if (sqrDist < _detectionRangeSqr && sqrDist < closestSqr)
            {
                if (food.TryClaim(_controller))
                {
                    closestSqr = sqrDist;
                    NearestConsumable = food;
                }
            }
        }

        // Check medicine
        foreach (MedicineController med in _controller.MonsterManager.activeMedicines)
        {
            if (med == null) continue;
            Vector2 medPos = med.GetComponent<RectTransform>().anchoredPosition;
            float sqrDist = (medPos - pos).sqrMagnitude;

            if (sqrDist < _detectionRangeSqr && sqrDist < closestSqr)
            {
                if (med.TryClaim(_controller))
                {
                    closestSqr = sqrDist;
                    NearestConsumable = med;
                }
            }
        }

        _cachedConsumableDistanceSqr = NearestConsumable != null ? closestSqr : float.MaxValue;
        IsNearConsumable = _cachedConsumableDistanceSqr < _consumeDistanceSqr;
    }

    public void HandleConsumableLogic(ref Vector2 targetPosition)
    {
        if (NearestConsumable == null || _isInternallyConsuming) return;
        if (_controller.StateMachine?.CurrentState == MonsterState.Eating) return;

        if (!IsValidConsumable(NearestConsumable))
        {
            ClearConsumable();
            return;
        }

        Vector2 currentPos = _rectTransform.anchoredPosition;
        Vector2 consumablePos = GetConsumablePosition(NearestConsumable);
        float currentDistanceSqr = (consumablePos - currentPos).sqrMagnitude;

        if (currentDistanceSqr < _consumeDistanceSqr)
        {
            StartConsumeSequence();
        }
        else
        {
            targetPosition = consumablePos;
        }
    }

    private void StartConsumeSequence()
    {
        if (_isInternallyConsuming || _consumeCoroutine != null) return;
        if (NearestConsumable == null) return;

        _isInternallyConsuming = true;
        _controller.StateMachine?.ChangeState(MonsterState.Eating);
        _consumeCoroutine = _controller.StartCoroutine(ConsumeAfterDelay());
    }

    private IEnumerator ConsumeAfterDelay()
    {
        float consumeDuration = _controller.StateMachine?.GetCurrentStateDuration() ?? 2f;
        yield return new WaitForSeconds(consumeDuration);

        if (NearestConsumable != null)
        {
            float nutrition = NearestConsumable.GetItemData().nutritionValue;

            if (NearestConsumable is FoodController food)
            {
                float newHunger = Mathf.Clamp(_controller.StatsHandler.CurrentHunger + nutrition, 0f, 100f);
                float newHappiness = Mathf.Clamp(_controller.StatsHandler.CurrentHappiness + (nutrition * 0.5f), 0f, 100f);

                _controller.SetHunger(newHunger);
                _controller.SetHappiness(newHappiness);
                _controller.MonsterManager.activeFoods.Remove(food);
            }
            else if (NearestConsumable is MedicineController med)
            {
                _controller.GiveMedicine(nutrition);
                _controller.MonsterManager.activeMedicines.Remove(med);
                _controller.UI.PlayHealingVFX(_controller);
            }

            _controller.MonsterManager.DespawnToPool(((MonoBehaviour)NearestConsumable).gameObject);
        }

        ClearConsumable();
        FinishConsuming();
    }

    private void FinishConsuming()
    {
        _controller.StateMachine?.ChangeState(MonsterState.Idle);
        _lastConsumeTime = Time.time;
        _consumeCoroutine = null;
        _isInternallyConsuming = false;
        _controller.EvolutionHandler.OnFoodConsumed();
    }

    private bool IsValidConsumable(IConsumable item)
    {
        return item != null && ((MonoBehaviour)item).gameObject.activeInHierarchy;
    }

    private Vector2 GetConsumablePosition(IConsumable item)
    {
        RectTransform rt = ((MonoBehaviour)item).GetComponent<RectTransform>();
        return rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    private void ClearConsumable()
    {
        ReleaseClaim();
        NearestConsumable = null;
    }

    private void ReleaseClaim()
    {
        if (NearestConsumable is FoodController food)
        {
            food.ReleaseClaim(_controller);
        }
        else if (NearestConsumable is MedicineController med)
        {
            if (med.claimedBy == _controller)
                med.claimedBy = null;
        }
    }
}
