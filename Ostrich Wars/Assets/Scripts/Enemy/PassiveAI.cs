using UnityEngine; 
using System.Collections.Generic;
/// <summary>
/// Temporary AI Descision Making
/// </summary>
public class PassiveAI : EnemyAI
{
    protected override void DecisionCalc(UnitInstance Actor, List<UnitInstance> PotentialTargets)
    {
        Debug.Log("Enemy Decides to pass!");
        BattleStateManager.OnPass.Invoke();
    }
}