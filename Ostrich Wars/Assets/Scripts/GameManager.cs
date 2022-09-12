using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror; 

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 

    public static List<string> InventoryCodes = new List<string>(); 

    public LocalProfile Player, Other;
    public Camera _CachedCamera;
    [SerializeField]
    private ParticleSystem _hoverParticle;
    public static ParticleSystem HoverParticle;

    [SerializeField]
    BattleStateManager BMS;

    public static Camera CachedCamera { get; set; }

    private void Awake()
    {
        instance = this; 
        CachedCamera = _CachedCamera;
        HoverParticle = _hoverParticle;
    }

    public void OnBeginBattle()
    {
        BattleStateManager.instance.SetOnlineBattleComponents(Player.CurrentTeam, Other.CurrentTeam, Player.CurrenDeck, Other.CurrenDeck);
    }

}

