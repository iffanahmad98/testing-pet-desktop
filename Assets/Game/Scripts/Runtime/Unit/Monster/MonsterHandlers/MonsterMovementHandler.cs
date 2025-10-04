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
        // Validate target is in bounds before moving toward it
        if (_controller.BoundHandler != null &&
            !_controller.BoundHandler.IsWithinBoundsForState(targetPosition, _controller.StateMachine.CurrentState) && !_controller.isNPC)
        {
            targetPosition = _controller.BoundHandler.GetRandomTargetForState(_controller.StateMachine.CurrentState);
        }

        if (_controller.EvolutionHandler != null && _controller.EvolutionHandler.IsEvolving)
            return;

        Vector2 pos = _transform.anchoredPosition;
        float currentSpeed = GetCurrentMoveSpeed(data);

        // Only move horizontally - keep Y position fixed
        Vector2 horizontalTarget = new Vector2(targetPosition.x, pos.y);
        _transform.anchoredPosition = Vector2.MoveTowards(pos, horizontalTarget, currentSpeed * Time.deltaTime);
        HandleStateSpecificBehavior(pos, horizontalTarget);
    }
    
    private float GetCurrentMoveSpeed(MonsterDataSO data)
    {
        if (_controller.StateMachine == null || data == null) return 0f;

        // Check if flying monster has reached destination (for hovering)
        bool isAtFlyingDestination = false;
        if (_controller.StateMachine.CurrentState == MonsterState.Flying)
        {
            Vector2 pos = _transform.anchoredPosition;
            Vector2 targetPos = _controller.GetTargetPosition();
            isAtFlyingDestination = Vector2.Distance(pos, targetPos) < 10f;
        }

        return _controller.StateMachine.CurrentState switch
        {
            MonsterState.Walking => GetWalkSpeed(data),
            MonsterState.Running => GetRunSpeed(data),
            MonsterState.Flying => isAtFlyingDestination ? 0f : GetFlySpeed(data), // Hover when reached destination
            MonsterState.Flapping => 0f,  // Ground-based wing flapping (no movement)
            MonsterState.Jumping => 0f,
            MonsterState.Eating => 0f,
            MonsterState.Idle => 0f, // No movement during idle (ground only)
            MonsterState.Itching => 0f, // No movement during itching
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
    
    private Vector2 _lastPosition;
    private float _lastFlipTime = 0f;
    private const float FLIP_COOLDOWN = 0.25f; // 250ms minimum between flips

    private void HandleDirectionalFlipping(Vector2 currentPos, Vector2 target)
    {
        // Use actual movement direction instead of target direction
        Vector2 movementDirection = currentPos - _lastPosition;
        
        // Only flip if we've moved a meaningful amount
        if (movementDirection.magnitude > 0.5f && Time.time - _lastFlipTime > FLIP_COOLDOWN) 
        {
            if (Mathf.Abs(movementDirection.x) > 0.1f) // Only flip for meaningful horizontal movement
            {
                Flip(movementDirection.x);
                _lastFlipTime = Time.time;
            }
        }
        
        _lastPosition = currentPos;
    }

    private void Flip(float direction)
    {
        Transform targetTransform = _spineGraphic.transform;
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

