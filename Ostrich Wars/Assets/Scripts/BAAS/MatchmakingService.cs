using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.Json; 
using PlayFab.MultiplayerModels;
using System;
using System.Linq; 

public class MatchmakingService : MonoBehaviour
{
    private LocalProfile player;
    private string ticketID; 
    private string QuickQue = "QuickPlay";
    private Coroutine TicktPolling;

    public GameObject Battle;

    private void Awake()
    {
        LoginManager.OnLogin += () => player = gameObject.GetComponent<LocalProfile>();
    }


    public void StartQuickMatch()
    {
        Tuple<string, string> TeamTouple = new Tuple<string, string>(TeamEncoding.InventoryEncoder(player.CurrentTeam), TeamEncoding.InventoryEncoder((player.CurrenDeck)));

        var request = new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer
            {
                Entity = new EntityKey
                {
                    Id = LoginManager.EntityID,
                    Type = "title_player_account"
                },

                Attributes = new MatchmakingPlayerAttributes
                {
                    DataObject = TeamTouple
                }
                
            },

            GiveUpAfterSeconds = 60, 
            QueueName = QuickQue
            
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnTicketCreated, OnPlayfabError);
    }

    private void OnPlayfabError(PlayFabError error)
    {
        Debug.LogWarning(error.GenerateErrorReport()); 
    }

    private void OnTicketCreated(CreateMatchmakingTicketResult result)
    {
        ticketID = result.TicketId;
        TicktPolling = StartCoroutine(PollTicket()); 

        Debug.Log("Ticket Created");
    }

    //Using temp poll method
    private IEnumerator PollTicket()
    {
        while (true) //I don't trust this
        {
            var request = new GetMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = QuickQue,
                
            };

            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGetMatchTicket, OnPlayfabError);
            yield return new WaitForSeconds(3.5f);
        }
    }

    private void OnGetMatchTicket(GetMatchmakingTicketResult result)
    {
        if (result.Status == "Matched")
        {
            StopCoroutine(TicktPolling);
            StartMatch(result.MatchId);
            return; 
        }
        else
        {

            return;
        }
    }

    private void StartMatch(string MatchID)
    {
        var request = new GetMatchRequest
        {
            MatchId = MatchID,
            QueueName = QuickQue,
            ReturnMemberAttributes = true
        };

        PlayFabMultiplayerAPI.GetMatch(request, OnGetMatch, PlayFabMatchError);
    }

    private void PlayFabMatchError(PlayFabError error) => Debug.LogWarning(error.ErrorMessage);

    private void OnGetMatch(GetMatchResult result)
    {
        int indexofPlayer = result.Members.IndexOf(result.Members.Where(ctx => ctx.Entity.Id == LoginManager.EntityID).FirstOrDefault());

        int indexOfOpp = indexofPlayer == 0 ? 1 : 0; 

        JsonObject OppList = result.Members[indexOfOpp].Attributes.DataObject as JsonObject;
        Debug.Log(OppList["Item1"].ToString()); 
        try
        {
            GameManager.instance.Other.CurrentTeam = TeamEncoding.InventoryDecoder<UnitSO>(OppList["Item1"].ToString());
            GameManager.instance.Other.CurrenDeck = TeamEncoding.InventoryDecoder<SpellSO>(OppList["Item2"].ToString());
            MainMenu.instance.gameObject.SetActive(false);
            Battle.SetActive(true);
            GameManager.instance.OnBeginBattle();
            //FindObjectOfType<BattleToServerMessenger>().Opp
        }
        catch
        {
            Debug.LogWarning("Attempting to load match information failed");
        }
        Debug.Log($"Match Found vs {result.Members[1].Entity.Id}");
    }

}
