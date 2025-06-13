using UnityEngine;
using System.Collections;
using System;

public class MonsterCoroutineHandler
{
    private MonsterController _controller;
    private MonsterStatsHandler _statsHandler;
    
    private Coroutine _hungerCoroutine;
    private Coroutine _happinessCoroutine;
    private Coroutine _poopCoroutine;
    private Coroutine _goldCoinCoroutine;
    private Coroutine _silverCoinCoroutine;
    
    public MonsterCoroutineHandler(MonsterController controller, MonsterStatsHandler statsHandler)
    {
        _controller = controller;
        _statsHandler = statsHandler;
    }
    
    public void StartAllCoroutines()
    {
        float goldCoinInterval = (float)TimeSpan.FromMinutes((double)CoinType.Gold).TotalSeconds;
        float silverCoinInterval = (float)TimeSpan.FromMinutes((double)CoinType.Silver).TotalSeconds;
        float poopInterval = (float)TimeSpan.FromMinutes(20f).TotalSeconds;


        _hungerCoroutine = _controller.StartCoroutine(HungerRoutine(1f));
        _happinessCoroutine = _controller.StartCoroutine(HappinessRoutine(1f));
        _poopCoroutine = _controller.StartCoroutine(PoopRoutine(poopInterval));
        _goldCoinCoroutine = _controller.StartCoroutine(CoinCoroutine(goldCoinInterval, CoinType.Gold));
        _silverCoinCoroutine = _controller.StartCoroutine(CoinCoroutine(silverCoinInterval, CoinType.Silver));
    }
    
    public void StopAllCoroutines()
    {
        if (_hungerCoroutine != null) _controller.StopCoroutine(_hungerCoroutine);
        if (_happinessCoroutine != null) _controller.StopCoroutine(_happinessCoroutine);
        if (_poopCoroutine != null) _controller.StopCoroutine(_poopCoroutine);
        if (_goldCoinCoroutine != null) _controller.StopCoroutine(_goldCoinCoroutine);
        if (_silverCoinCoroutine != null) _controller.StopCoroutine(_silverCoinCoroutine);
    }
    
    private IEnumerator HungerRoutine(float interval)
    {
        while (true)
        {
            if (_controller.MonsterData != null)
            {
                float newHunger = Mathf.Clamp(_statsHandler.CurrentHunger - _controller.MonsterData.hungerDepleteRate, 0f, 100f);
                _statsHandler.SetHunger(newHunger);
                _statsHandler.UpdateSickStatus(interval);
            }
            yield return new WaitForSeconds(interval);
        }
    }
    
    private IEnumerator HappinessRoutine(float interval)
    {
        while (true)
        {
            if (!_statsHandler.IsSick && _controller.MonsterData != null)
            {
                // Update happiness based on area
                _statsHandler.UpdateHappinessBasedOnArea(_controller.MonsterData, ServiceLocator.Get<GameManager>());
                
                // Reduce happiness if hungry
                if (_statsHandler.CurrentHunger < _controller.MonsterData.hungerHappinessThreshold)
                {
                    float newHappiness = Mathf.Clamp(_statsHandler.CurrentHappiness - _controller.MonsterData.hungerHappinessDrainRate, 0f, 100f);
                    _statsHandler.SetHappiness(newHappiness);
                }
            }
            else if (_statsHandler.IsSick)
            {
                // Reduce happiness when sick
                float newHappiness = Mathf.Clamp(_statsHandler.CurrentHappiness - 2f, 0f, 100f);
                _statsHandler.SetHappiness(newHappiness);
            }
            
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator PoopRoutine(float interval)
    {
        interval = 10f;

        yield return new WaitForSeconds(interval);
        while (true)
        {
            var monsterData = _controller.MonsterData;
            if (monsterData != null)
            {
                if (monsterData.monType == MonsterType.Common ||
                    monsterData.monType == MonsterType.Uncommon)
                    _controller.Poop(PoopType.Normal);
                else if (monsterData.monType == MonsterType.Rare ||
                         monsterData.monType == MonsterType.Boss ||
                         monsterData.monType == MonsterType.Mythic)
                    _controller.Poop(PoopType.Special);
                else
                    _controller.Poop(); // Default to normal poop if type is unknown
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator CoinCoroutine(float delay, CoinType type)
    {
        delay = 20f;
        
        yield return new WaitForSeconds(delay);
        while (true)
        {
            _controller.DropCoin(type);
            yield return new WaitForSeconds(delay);
        }
    }
}
