using UnityEngine;

public class MonsterStatsHandler
{
    private MonsterController _controller;
    private float _currentHunger;
    private float _currentHappiness;
    private float _currentHP;
    private float _maxHP;
    private bool _isSick;
    private float _lowHealthTimer;
    private const float SICKNESS_THRESHOLD_HP = 0.4f;
    private const float HP_DRAIN_PER_MINUTE = 10f;
    private const float SICK_HUNGER_THRESHOLD = 30f;
    private const float SICK_THRESHOLD_TIME = 1f;

    // Properties
    public float CurrentHunger => _currentHunger;
    public float CurrentHappiness => _currentHappiness;
    public float CurrentHP => _currentHP;
    public bool IsSick => _currentHP < SICKNESS_THRESHOLD_HP * _maxHP;

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
            _currentHunger = Mathf.Clamp(_controller.MonsterData.baseHunger, 0f, 100f);
            _currentHappiness = _controller.MonsterData.baseHappiness;
            _currentHP = _controller.MonsterData.GetMaxHealth(_controller.evolutionLevel);
        }
        else
        {
            _currentHunger = 50f;
            _currentHappiness = 100f;
            _currentHP = 100f;
        }

        _isSick = false;
        _lowHealthTimer = 0f;

        OnHungerChanged?.Invoke(_currentHunger);
        OnHappinessChanged?.Invoke(_currentHappiness);
        OnHealthChanged?.Invoke(_currentHP);
        OnSickChanged?.Invoke(_isSick);
    }

    public void Initialize(float initialHealth, float initialHunger, float initialHappiness, float maxHP)
    {
        _currentHunger = Mathf.Clamp(initialHunger, 0f, 100f);
        _currentHappiness = Mathf.Clamp(initialHappiness, 0f, 100f);
        _maxHP = maxHP;
        _currentHP = Mathf.Clamp(initialHealth, 0f, _maxHP);

    }

    public void SetHunger(float value)
    {
        // Clamp hunger between 0 and 100
        float clampedValue = Mathf.Clamp(value, 0f, 100f);

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

    public void UpdateSickStatus(float deltaTime)
    {
        if (_currentHunger <= SICK_HUNGER_THRESHOLD && !_isSick)
        {
            _lowHealthTimer += deltaTime;
            if (_lowHealthTimer >= SICK_THRESHOLD_TIME)
            {
                SetSick(true);
            }
        }
        else if (_currentHunger > SICK_HUNGER_THRESHOLD)
        {
            _lowHealthTimer = 0f;
        }
    }

    public void IncreaseHappiness(float amount)
    {
        SetHappiness(Mathf.Clamp(_currentHappiness + amount, 0f, 100f));
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
