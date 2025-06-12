using UnityEngine;
using Spine.Unity;


public class MonsterMovementHandler
{
    private RectTransform _transform;
    private MonsterStateMachine _stateMachine;
    private MonsterController _controller; // NEW: Add controller reference
    private GameManager _gameManager;
    private SkeletonGraphic _spineGraphic;
    
    public MonsterMovementHandler(RectTransform transform, MonsterStateMachine stateMachine, MonsterController controller, GameManager gameManager, SkeletonGraphic spineGraphic)
    {
        _transform = transform;
        _stateMachine = stateMachine;
        _controller = controller; // NEW
        _gameManager = gameManager;
        _spineGraphic = spineGraphic;
    }
    
    public void UpdateMovement(ref Vector2 targetPosition, MonsterDataSO data)
    {
        Vector2 pos = _transform.anchoredPosition;
        float currentSpeed = GetCurrentMoveSpeed(data);
        
        _transform.anchoredPosition = Vector2.MoveTowards(pos, targetPosition, currentSpeed * Time.deltaTime);
        HandleStateSpecificBehavior(pos, targetPosition);
    }
    
    private float GetCurrentMoveSpeed(MonsterDataSO data)
    {
        if (_stateMachine == null || data == null) return 0f;
        
        return _stateMachine.CurrentState switch
        {
            MonsterState.Walking => GetWalkSpeed(data),
            MonsterState.Running => GetRunSpeed(data),
            MonsterState.Flying => GetFlySpeed(data),
            MonsterState.Jumping => 0f,
            MonsterState.Eating => 0f,
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
            
        return data.moveSpd * 1.3f;
    }
    
    private float GetFlySpeed(MonsterDataSO data)
    {
        if (TryGetBehaviorConfigSpeed("fly", out float configSpeed))
            return configSpeed;
            
        return data.moveSpd * 1.1f;
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
                "fly" => config.flySpeed, // Assuming this exists
                _ => 0f
            };
            return speed > 0f;
        }
        
        return false;
    }
    
    private void HandleStateSpecificBehavior(Vector2 pos, Vector2 target)
    {
        if (_spineGraphic == null || _stateMachine == null) return;
        
        var state = _stateMachine.CurrentState; 
        UpdateAnimation(state);
        HandleDirectionalFlipping(pos, target);
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
        return _stateMachine?.GetAvailableAnimation(state) ?? "idle";
    }
    
    private void HandleDirectionalFlipping(Vector2 pos, Vector2 target)
    {
        float direction = target.x - pos.x;
        
        if (Mathf.Abs(direction) > 0.1f)
        {
            Transform parentTransform = _spineGraphic.transform.parent;
            Transform targetTransform = parentTransform ?? _spineGraphic.transform;
            
            Vector3 scale = targetTransform.localScale;
            scale.x = direction > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            targetTransform.localScale = scale;
        }
    }
}

