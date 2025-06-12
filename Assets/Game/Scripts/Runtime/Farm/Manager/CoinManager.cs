using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using System;
namespace MagicalGarden.Farm
{
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance;
        public int coins = 5000;
        public event Action OnCoinChanged;

        private void Awake()
        {
            Instance = this;
        }

        public bool HasCoins(int amount) => coins >= amount;

        public void SpendCoins(int amount)
        {
            coins -= amount;
            OnCoinChanged?.Invoke();
            Debug.Log($"Koin berkurang {amount}, sisa: {coins}");
        }

        public void AddCoins(int amount)
        {
            coins += amount;
            OnCoinChanged?.Invoke();
        }
    }
}