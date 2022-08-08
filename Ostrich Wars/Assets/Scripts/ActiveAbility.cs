using UnityEngine; 

public class ActiveAbility : Ability
{

    private void Awake()
    {
        _effect = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        CastCheck();
    }

    protected override void OnCast()
    {
        //Does real cast when hit
        Debug.Log("hit");
        //TODO: Wait until Animation is finished
        BattleStateManager.EndTurnAfterSpellCast();
        Destroy(gameObject);
    }

    public virtual void Activate()
    {
        OnCast();
    }

}
