using UnityEngine;
using Spine.Unity;
using Unity.VisualScripting;


public class MonsterMovementHandler
{
    private RectTransform _transform;
    private MonsterController _controller;
    private SkeletonGraphic _spineGraphic;

    public MonsterMovementHandler(MonsterController controller, RectTransform transform, SkeletonGraphic spineGraphic)
    {
        _controller = controller;
        _transform = transform;
        _spineGraphic = spineGraphic;
    }
    
    public void UpdateMovement(ref Vector2 targetPosition, MonsterDataSO data)
    {
        if (_controller.EvolutionHandler != null && _controller.IsEvolving)
            return;

        Vector2 pos = _transform.anchoredPosition;
        float currentSpeed = GetCurrentMoveSpeed(data);
        _transform.anchoredPosition = Vector2.MoveTowards(pos, targetPosition, currentSpeed * Time.deltaTime);
        HandleStateSpecificBehavior(pos, targetPosition);
    }
    
    private float GetCurrentMoveSpeed(MonsterDataSO data)
    {
        if (_controller.StateMachine == null || data == null) return 0f;

        return _controller.StateMachine.CurrentState switch
        {
            MonsterState.Walking => GetWalkSpeed(data),
            MonsterState.Running => GetRunSpeed(data),
            MonsterState.Flying => GetFlySpeed(data),
            MonsterState.Flapping => 0f,
            MonsterState.Jumping => 0f,
            MonsterState.Eating => 0f,
            MonsterState.Idle => IsInAir() ? GetFlySpeed(data) * 0.3f : 0f, // Gentle floating for air idle
            MonsterState.Itching => IsInAir() ? GetFlySpeed(data) * 0.2f : 0f, // Very slow air movement
            _ => 0f
        };
    }
    
    private float GetWalkSpeed(MonsterDataSO data)
    {
        // Try to get from behavior config first
        if (TryGetBehaviorConfigSpeed("walk", out float configSpeed))
            return configSpeed;
            
        // Fallback to base speed * modifier
        return data.moveSpd * 0.7f;
    }
    
    private float GetRunSpeed(MonsterDataSO data)
    {
        if (TryGetBehaviorConfigSpeed("run", out float configSpeed))
            return configSpeed;
            
        return data.moveSpd * 1.2f;
    }
    
    private float GetFlySpeed(MonsterDataSO data)
    {
        if (TryGetBehaviorConfigSpeed("fly", out float configSpeed))
            return configSpeed;
            
        return data.moveSpd * 1.5f;
    }
    
    private bool TryGetBehaviorConfigSpeed(string speedType, out float speed)
    {
        speed = 0f;
        
        var data = _controller?.MonsterData;
        if (data?.evolutionBehaviors == null) return false;
        
        var evolutionBehavior = System.Array.Find(data.evolutionBehaviors,
            config => config.evolutionLevel == _controller.evolutionLevel);
            
        if (evolutionBehavior?.behaviorConfig != null)
        {
            var config = evolutionBehavior.behaviorConfig;
            speed = speedType switch
            {
                "walk" => config.walkSpeed,
                "run" => config.runSpeed,
                "fly" => config.flySpeed,
                _ => 0f
            };
            return speed > 0f;
        }
        
        return false;
    }
    
    private void HandleStateSpecificBehavior(Vector2 pos, Vector2 target)
    {
        if (_spineGraphic == null || _controller.StateMachine == null) return;

        MonsterState state = _controller.StateMachine.CurrentState;
        HandleDirectionalFlipping(pos, target);
        UpdateAnimation(state); 
    }
    
    private void HandleDirectionalFlipping(Vector2 pos, Vector2 target)
    {
        float direction = target.x - pos.x;

        // Normal areas: Standard flipping
        float flipThreshold = 0.1f;
        if (Mathf.Abs(direction) > flipThreshold)
        {
            Flip(direction);
        }
    }

    private void Flip(float direction)
    {
        Transform parentTransform = _spineGraphic.transform.parent;
        Transform targetTransform = parentTransform ?? _spineGraphic.transform;
        
        Vector3 scale = targetTransform.localScale;
        scale.x = direction > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        targetTransform.localScale = scale;
    }
    
    private void UpdateAnimation(MonsterState state)
    {
        string animationName = GetAnimationForState(state);
        var current = _spineGraphic.AnimationState.GetCurrent(0);

        if (current == null || current.Animation.Name != animationName)
        {
            _spineGraphic.AnimationState.SetAnimation(0, animationName, true);
        }
    }
    
    private string GetAnimationForState(MonsterState state)
    {
        // Let MonsterStateMachine handle the logic
        return _controller.StateMachine?.GetAvailableAnimation(state) ?? "idle";
    }

    // helper method to check if monster is in air
    private bool IsInAir()
    {
        Vector2 currentPos = _transform.anchoredPosition;
        return currentPos.y > -200f; // Adjust threshold based on your game area
    }
}

