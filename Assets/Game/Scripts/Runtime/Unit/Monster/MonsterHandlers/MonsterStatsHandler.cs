using UnityEngine;

public class MonsterStatsHandler
{
    private MonsterController _controller;
    private float _currentHunger;
    private float _currentHappiness;
    private bool _isSick;
    private float _lowHungerTime;
    private float _currentHP = 100f;

    private float _maxHP = 100f;
    private const float SICKNESS_THRESHOLD_HP = 40f;
    private const float HP_DRAIN_PER_MINUTE = 50f;

    private const float SICK_HUNGER_THRESHOLD = 30f;
    private const float SICK_THRESHOLD_TIME = 1f;

    // Properties
    public float CurrentHunger => _currentHunger;
    public float CurrentHappiness => _currentHappiness;
    public bool IsSick => _currentHP < SICKNESS_THRESHOLD_HP;
    public float CurrentHP => _currentHP;

    public float LowHungerTime => _lowHungerTime;

    // Events
    public event System.Action<float> OnHungerChanged;
    public event System.Action<float> OnHappinessChanged;
    public event System.Action<bool> OnSickChanged;
    public event System.Action<float> OnHealthChanged;

    public MonsterStatsHandler(MonsterController controller)
    {
        _controller = controller;

        if (_controller.MonsterData != null)
        {
            float maxHunger = _controller.MonsterData.GetMaxHunger(_controller.evolutionLevel);
            _currentHunger = Mathf.Clamp(_controller.MonsterData.baseHunger, 0f, maxHunger);
            _currentHappiness = _controller.MonsterData.baseHappiness;
        }
        else
        {
            // Fallback values that match the controller's fallbacks
            _currentHunger = 0f;
            _currentHappiness = 0f;
        }

        _isSick = false;
        _lowHungerTime = 0f;

        OnHungerChanged?.Invoke(_currentHunger);
        OnHappinessChanged?.Invoke(_currentHappiness);
        OnHealthChanged?.Invoke(_currentHP);
        OnSickChanged?.Invoke(_isSick);
    }

    public void Initialize(float initialHealth, float initialHunger, float initialHappiness, float maxHP)
    {
        float maxHunger = _controller.MonsterData?.GetMaxHunger(_controller.evolutionLevel) ?? 100f;
        _currentHunger = Mathf.Clamp(initialHunger, 0f, maxHunger);

        _currentHappiness = Mathf.Clamp(initialHappiness, 0f, 100f);
        _maxHP = maxHP;
        _currentHP = Mathf.Clamp(initialHealth, 0f, _maxHP);
    }

    public void SetHunger(float value)
    {
        // Clamp the value between 0 and monster's max hunger based on evolution level
        float maxHunger = _controller.MonsterData?.GetMaxHunger(_controller.evolutionLevel) ?? 100f;
        float clampedValue = Mathf.Clamp(value, 0f, maxHunger);

        if (Mathf.Approximately(_currentHunger, clampedValue)) return;
        _currentHunger = clampedValue;
        OnHungerChanged?.Invoke(_currentHunger);
    }

    public void SetHappiness(float value)
    {
        // Clamp happiness between 0 and 100
        float clampedValue = Mathf.Clamp(value, 0f, 100f);

        if (Mathf.Approximately(_currentHappiness, clampedValue)) return;
        _currentHappiness = clampedValue;
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

    public void UpdateHappinessBasedOnArea(MonsterDataSO monsterData, MonsterManager gameManager)
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
    public void UpdateHealth(float deltaTime)
    {
        float avg = (CurrentHappiness + CurrentHunger) / 2f;

        if (avg < 60f)
        {
            float hpLoss = HP_DRAIN_PER_MINUTE / 60f * deltaTime;
            SetHP(_currentHP - hpLoss);
        }
    }

    public void SetHP(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, _maxHP);
        if (Mathf.Approximately(clamped, _currentHP)) return;
        _currentHP = clamped;
        OnSickChanged?.Invoke(IsSick);
        OnHealthChanged?.Invoke(_currentHP);
    }

    public void Heal(float amount)
    {
        SetHP(_currentHP + amount);
        OnHealthChanged?.Invoke(_currentHP);
    }


}
