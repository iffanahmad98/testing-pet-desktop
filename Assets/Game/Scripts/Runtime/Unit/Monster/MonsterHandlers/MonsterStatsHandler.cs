using UnityEngine;

public class MonsterStatsHandler
{
    private MonsterController _controller;
    private float _currentHunger = 100f;
    private float _currentHappiness = 100f;
    private bool _isSick = false;
    private float _lowHungerTime = 0f;
    
    private const float SICK_HUNGER_THRESHOLD = 30f;
    private const float SICK_THRESHOLD_TIME = 1f;
    
    // Properties
    public float CurrentHunger => _currentHunger;
    public float CurrentHappiness => _currentHappiness;
    public bool IsSick => _isSick;
    public float LowHungerTime => _lowHungerTime;
    
    // Events
    public event System.Action<float> OnHungerChanged;
    public event System.Action<float> OnHappinessChanged;
    public event System.Action<bool> OnSickChanged;
    
    public MonsterStatsHandler(MonsterController controller)
    {
        _controller = controller;
    }
    
    public void Initialize(float initialHunger, float initialHappiness, bool initialSick, float initialLowHungerTime)
    {
        _currentHunger = initialHunger;
        _currentHappiness = initialHappiness;
        _isSick = initialSick;
        _lowHungerTime = initialLowHungerTime;
    }
    
    public void SetHunger(float value)
    {
        if (Mathf.Approximately(_currentHunger, value)) return;
        _currentHunger = value;
        OnHungerChanged?.Invoke(_currentHunger);
    }
    
    public void SetHappiness(float value)
    {
        if (Mathf.Approximately(_currentHappiness, value)) return;
        _currentHappiness = value;
        OnHappinessChanged?.Invoke(_currentHappiness);
    }
    
    public void SetSick(bool value)
    {
        if (_isSick == value) return;
        _isSick = value;
        OnSickChanged?.Invoke(_isSick);
    }
    
    public void SetLowHungerTime(float value) => _lowHungerTime = value;
    
    public void UpdateSickStatus(float deltaTime)
    {
        if (_currentHunger <= SICK_HUNGER_THRESHOLD && !_isSick)
        {
            _lowHungerTime += deltaTime;
            if (_lowHungerTime >= SICK_THRESHOLD_TIME)
            {
                SetSick(true);
            }
        }
        else if (_currentHunger > SICK_HUNGER_THRESHOLD)
        {
            _lowHungerTime = 0f;
        }
    }
    
    public bool Feed(float amount)
    {
        if (!_isSick)
        {
            SetHunger(Mathf.Clamp(_currentHunger + amount, 0f, 100f));
            SetHappiness(Mathf.Clamp(_currentHappiness + amount, 0f, 100f));
            return true; // Successfully fed
        }
        return false; // Too sick to eat
    }
    
    public void IncreaseHappiness(float amount)
    {
        SetHappiness(Mathf.Clamp(_currentHappiness + amount, 0f, 100f));
    }
    
    public void TreatSickness()
    {
        if (!_isSick) return;
        
        SetSick(false);
        SetHunger(50f);
        SetHappiness(10f);
        _lowHungerTime = 0f;
    }
    
    public void UpdateHappinessBasedOnArea(MonsterDataSO monsterData, GameManager gameManager)
    {
        if (monsterData == null || gameManager?.gameArea == null) return;

        float gameAreaHeight = gameManager.gameArea.sizeDelta.y;
        float screenHeight = Screen.currentResolution.height;
        float heightRatio = gameAreaHeight / screenHeight;

        if (heightRatio >= 0.5f)
            SetHappiness(Mathf.Clamp(_currentHappiness + monsterData.areaHappinessRate, 0f, 100f));
        else
            SetHappiness(Mathf.Clamp(_currentHappiness - monsterData.areaHappinessRate, 0f, 100f));
    }
}
