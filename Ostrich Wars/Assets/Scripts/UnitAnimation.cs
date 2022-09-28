using UnityEngine;

public class UnitAnimation : MonoBehaviour
{
    Animator _ani;
    string _defaultName;
    UnitInstance unit; 
    private void Awake()
    {
        _ani = GetComponentInChildren<Animator>();
        _defaultName = _ani.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        unit = GetComponentInParent<UnitInstance>(); 
    }

    public void AttackTrigger() => _ani.SetTrigger("Attack");
    public void CastTrigger() => _ani.SetTrigger("Cast");
    public void DamageTrigger() => _ani.SetTrigger("Take Damage");
    public void DieTrigger() => _ani.SetTrigger("Die");

    public void HitParticleEffect(GameObject go)
    {
        FindObjectOfType<BattleStateManager>().PlayHitAnimation(go.GetComponent<ParticleSystem>()); 
    }

    private void Update()
    {
        unit.CurrentlyAnimated = !isIdle(); 
    }

    private bool isIdle()
    {
        string CurrentAnimation = _ani.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        return CurrentAnimation == _defaultName||
            CurrentAnimation.Contains("Death") ||
            CurrentAnimation.Contains("Dying") ||
            CurrentAnimation.Contains("Dead");
    }
}