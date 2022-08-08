using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Profile Player, Other;
    public Camera _CachedCamera;
    [SerializeField]
    private ParticleSystem _hoverParticle;
    public static ParticleSystem HoverParticle;

    public static Camera CachedCamera { get; set; }

    private void Awake()
    {
        CachedCamera = _CachedCamera;
        HoverParticle = _hoverParticle;
    }

    public void OnBeginBattle()
    {
        BattleStateManager.SetBattleComponents(Player.CurrentTeam, Other.CurrentTeam, Player.CurrenDeck, Other.CurrenDeck);
    }
}
