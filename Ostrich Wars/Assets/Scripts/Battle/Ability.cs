using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    protected ParticleSystem[] _effect;

    protected abstract void OnCast();

    protected void CastCheck()
    {
        if (_effect != null)
        {
            bool Finished = false;

            foreach (ParticleSystem PA in _effect)
            {
                Finished = PA.isStopped;
            }

            BattleStateManager.instance.ActiveAnimation = !Finished;

            if (Finished)
            {
                OnCast();
            }
        }
    }
}
