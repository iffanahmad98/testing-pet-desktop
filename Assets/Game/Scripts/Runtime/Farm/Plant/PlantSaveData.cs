using System;
using System.Collections.Generic;
using UnityEngine;
using MagicalGarden.Farm;

namespace MagicalGarden.Manager
{
    [Serializable]
    public class PlantSaveData
    {
        public Vector3Int cellPosition;
        public string itemId;
        public int currentStage;
        public float timeInStage;
        public string plantedTime;
        public string lastUpdateTime;
        public string lastWateredTime;
        public PlantStatus status;
        public bool isFertilized;
        public bool isMonsterSeed;
    }

    [Serializable]
    public class PlantSaveWrapper
    {
        public List<int> farmAreaIds = new ();
        public List<PlantSaveData> data;
    }
}
