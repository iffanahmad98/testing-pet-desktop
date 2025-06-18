using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EvolutionRequirements", menuName = "Monster/Evolution Requirements")]
public class EvolutionRequirementsSO : ScriptableObject
{
    [Header("Evolution Chain")]
    public EvolutionRequirement[] requirements;
}


