using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporary AI Descision Making
/// </summary>
public abstract class EnemyAI : MonoBehaviour
{
    protected abstract void DecisionCalc(UnitInstance Actor, List<UnitInstance> PotentialTargets);
    public void ExecuteDescision(UnitInstance Actor, List<UnitInstance> PotentialTargets)
    {
        DecisionCalc(Actor, PotentialTargets); 
    }
}
