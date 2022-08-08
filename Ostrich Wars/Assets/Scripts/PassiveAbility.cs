public class PassiveAbility : Ability
{
    protected bool TriggerCondition;

    private void Update()
    {
        if (TriggerCondition)
        {
            OnCast(); 
        }
    }

    protected override void OnCast()
    {
    }
}
