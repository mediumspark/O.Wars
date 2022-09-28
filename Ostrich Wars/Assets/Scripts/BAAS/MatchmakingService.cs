using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Networking; 
using PlayFab;
using PlayFab.Json; 
using PlayFab.MultiplayerModels;
using System;
using System.Linq;
using Mirror; 

public class MatchmakingService : MonoBehaviour
{
    private LocalProfile player;
    private string ticketID; 
    private string QuickQue = "QuickPlay";
    private Coroutine TicktPolling;
    public GameObject Battle;
    //public OnlineBattleStateManager BSM; 


    private void Start()
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
                    DataObject = new
                    {
                        Item1 = TeamTouple.Item1,
                        Item2 = TeamTouple.Item2,
                        Latencies = new object[]
                        {
                            new {
                                latency = 150,
                                region = "EastUs"
                            }
                        }
                    }
                }
                
            },

            GiveUpAfterSeconds = 120, 
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
            yield return new WaitForSeconds(1.25f);
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
            Debug.Log(result.Status); 
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
        NetworkManager.singleton.networkAddress = result.ServerDetails.IPV4Address;
        NetworkManager.singleton.GetComponent<TelepathyTransport>().port = (ushort)result.ServerDetails.Ports[0].Num;
        NetworkManager.singleton.StartClient();

        int indexofPlayer = result.Members.IndexOf(result.Members.Where(ctx => ctx.Entity.Id == LoginManager.EntityID).FirstOrDefault());

        int indexOfOpp = indexofPlayer == 0 ? 1 : 0; 

        JsonObject OppList = result.Members[indexOfOpp].Attributes.DataObject as JsonObject;
       // Debug.Log(OppList["Item1"].ToString()); The Player's party printed
        try
        {
            GameManager.instance.Other.CurrentTeam = TeamEncoding.InventoryDecoder<UnitSO>(OppList["Item1"].ToString());
            GameManager.instance.Other.CurrenDeck = TeamEncoding.InventoryDecoder<SpellSO>(OppList["Item2"].ToString());
            MainMenu.instance.gameObject.SetActive(false);
            Battle.SetActive(true);   

        }
        catch
        {
            Debug.LogWarning("Attempting to load match information failed");
        }
        Debug.Log($"Match Found vs {result.Members[1].Entity.Id}");
    }

    public void LeaveQueue()
    {
        var request = new CancelAllMatchmakingTicketsForPlayerRequest {
            QueueName = QuickQue,
            Entity =
            {
                Id = LoginManager.EntityID,
                Type = "title_player_account"
            }
        };

        void CancelResult(CancelAllMatchmakingTicketsForPlayerResult result)
        {
            Debug.Log("Left Queue");
        }

        PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(request, CancelResult, OnPlayfabError);
    }
}
