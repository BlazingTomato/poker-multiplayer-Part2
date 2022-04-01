using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class EventController : MonoBehaviour
{


    public GameController gameController;
    public Dealer dealer;
    public CalculateWinner calculateWinner;
    public UIController uIController;

    [Header("Event Codes")]
    public const byte START_TIMER_EVENT = 1;
    public const byte DEAL_CARD_TO_PLAYER = 2;
    public const byte DEAL_CARD = 3;
    public const byte DEAL_FLOP = 4;
    public const byte RESTART_GAME = 5;
    public const byte SET_TURN_TRUE = 6;
    public const byte FOLD_BTN = 7;
    public const byte CALL_BTN = 8;
    public const byte CHECK_BTN = 9;
    public const byte BET_BTN = 10;
    public const byte INCREMENT_COUNT = 12;
    public const byte RESET_CURRENT_BET = 13;
    public const byte CALCULATE_WINNER = 14;


    private void OnEnable() {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }
 
    private void OnDisable() {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent){
        byte eventCode = photonEvent.Code;
       
        //Debug.Log("Receiving Event");
 
        if(eventCode == START_TIMER_EVENT){
            object[] data = (object[])photonEvent.CustomData;
            int playerCount = (int)data[0];
            float fillAmount = (float)data[1];
           
           
            gameController.PokerPlayers[playerCount].GetComponent<Image>().fillAmount = fillAmount;
        }
 
        if(eventCode == DEAL_CARD_TO_PLAYER){
            object[] data = (object[])photonEvent.CustomData;
            dealer.dealCardPlayerEvent(data);
        }
 
        if(eventCode == DEAL_CARD){
            object[] data = (object[])photonEvent.CustomData;
            dealer.dealCardEvent(data);
        }
 
        if(eventCode == DEAL_FLOP){
            object[] data = (object[])photonEvent.CustomData;
            dealer.flopEvent(data);
        }
 
        if(eventCode == RESTART_GAME){
            gameController.RestartEvent();
        }
 
        if(eventCode == SET_TURN_TRUE){
            object[] data = (object[])photonEvent.CustomData;
            gameController.setTurnTrueEvent(data);
        }
        
        if(eventCode == FOLD_BTN){
            object[] data = (object[])photonEvent.CustomData;
            uIController.Fold_BTN_Event(data);
        }

        if(eventCode == CALL_BTN){
            object[] data = (object[])photonEvent.CustomData;
            uIController.Call_BTN_Event(data);
        }

        if(eventCode == CHECK_BTN){
            object[] data = (object[])photonEvent.CustomData;
            uIController.Check_BTN_Event(data);
        }

        if(eventCode == BET_BTN){
            object[] data = (object[])photonEvent.CustomData;
            uIController.Bet_BTN_Event(data);
        }

        if(eventCode == INCREMENT_COUNT){
            object[] data = (object[])photonEvent.CustomData;
            gameController.playerCount = (int)data[0];
            gameController.setTurnTrue(gameController.playerCount);
        }
    
        if(eventCode == RESET_CURRENT_BET){
            gameController.currentBet = 0;
        }

        if(eventCode == CALCULATE_WINNER){
            calculateWinner.CalculateWinnerEvent();
        }
    
    }
 
}
