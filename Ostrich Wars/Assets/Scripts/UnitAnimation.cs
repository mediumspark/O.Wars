using UnityEngine;
using UnityEditor;

public class UnitAnimation : MonoBehaviour
{
    Animator _ani;
    string _defaultName; 
    private void Awake()
    {
        _ani = GetComponentInChildren<Animator>();
        _defaultName = _ani.GetCurrentAnimatorClipInfo(0)[0].clip.name; 
    }

    public void AttackTrigger() => _ani.SetTrigger("Attack");
    public void CastTrigger() => _ani.SetTrigger("Cast");
    public void DamageTrigger() => _ani.SetTrigger("Take Damage");
    public void DieTrigger() => _ani.SetTrigger("Die");

    public void HitParticleEffect(GameObject go)
    {
        FindObjectOfType<BattleStateManager>().PlayHitAnimation(go.GetComponent<ParticleSystem>()); 
    }

}