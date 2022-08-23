using UnityEngine;

public class TravelCast : Ability
{
    [SerializeField]
    float _smoothSpeed;
    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {
        _effect = GetComponentsInChildren<ParticleSystem>(); 
    }

    private void Update()
    {
        Travel(_smoothSpeed);

        CastCheck(); 
    }

    public void Travel(float Speed)
    {
        transform.position = Vector3.SmoothDamp(transform.position, BattleStateManager.instance.Target.transform.position, ref _velocity, Speed); 
    }

    protected override void OnCast()
    {
        //Does real cast when hit
        Debug.Log("hit");
        //TODO: Wait until Animation is finished
        BattleStateManager.instance.EndTurnAfterSpellCast();
        Destroy(gameObject);
    }
}
