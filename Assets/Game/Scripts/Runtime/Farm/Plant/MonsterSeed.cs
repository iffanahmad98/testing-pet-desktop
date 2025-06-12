using System.Collections.Generic;
namespace MagicalGarden.Farm
{
    public class MonsterSeed : SeedBase
    {
        public string nameMonster;
        public MonsterType rarity;

        private void Start()
        {
            var expectedStageCount = GetGrowthRequirements().Count;
            if (itemData.stageTiles.Count < expectedStageCount)
            {
                UnityEngine.Debug.LogWarning($"MonsterSeed '{seedName}' tile count ({itemData.stageTiles.Count}) is less than required stage count ({expectedStageCount}).");
            }
        }
        public override List<GrowthStage> GetGrowthRequirements()
        {
            switch (rarity)
            {
                case MonsterType.Common:
                    return new List<GrowthStage> {
                        new GrowthStage { requiredHours = 1 },
                        new GrowthStage { requiredHours = 1 },
                        new GrowthStage { requiredHours = 1 },
                        new GrowthStage { requiredHours = 1 },
                    };
                case MonsterType.Uncommon:
                    return new List<GrowthStage> {
                        new GrowthStage { requiredHours = 3 },
                        new GrowthStage { requiredHours = 4 },
                        new GrowthStage { requiredHours = 5 },
                    };
                default:
                    return new List<GrowthStage> {
                        new GrowthStage { requiredHours = 2 },
                        new GrowthStage { requiredHours = 3 },
                        new GrowthStage { requiredHours = 4 },
                    };
            }
        }

        public override Monster GetMonster()
        {
            return new Monster(nameMonster, rarity);
        }
    }
}