using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

 
 
public class GameController : MonoBehaviourPun
{
 
    #region Variables

    [Header("Player Information")]
    public GameObject[] PokerPlayers;
    public float timeBetweenTurns = 0f;
    public int playerCount = 0;
    GameObject p = null;
 
 
    [Header("Game States")]
    [SerializeField] bool gameStarted = false;
    [SerializeField] bool GameOver = false;
    [SerializeField] bool EveryoneHasPlayed = false;
    public int currentBet = 0;

    public int totalBet = 0;
    public int round;
 
 

    public Dealer dealer;

    public CalculateWinner calculateWinner;
    public UIController uICOntroller;


 
 
    #endregion
 
    #region UnityFunctions

    private void Start() {
        timeBetweenTurns = 10f;
        playerCount = 0;
        gameStarted = false;
        GameOver = false;
        EveryoneHasPlayed = false;
        currentBet = 0;
        round = 0;
        totalBet = 0;

        calculateWinner.Reset();
        

        dealer.dealerReset();
        uICOntroller.resetUI();
        calculateWinner.Reset();
        

        PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");

        for(int i = 0; i < PokerPlayers.Length; i++){
                PokerPlayers[i].GetComponent<TimerController>().isPlaying = true;
                PokerPlayers[i].GetComponent<TimerController>().isTurn = false;
        }

    }

    
 
    public void startNewGame(){
        setTurnTrue(0);
        gameStarted  = true;
        dealer.dealCardToPlayer();
        p = PokerPlayers[0];
    }
   
    private void Update() {
         
        EnableButtons();

        if(!gameStarted){
            return;
        }

        if(!PhotonNetwork.IsMasterClient){
            return;
        }
 
        if(!EveryoneHasPlayed){
            if(timeBetweenTurns <= 0){
                uICOntroller.resetStatusTexts();
                timeBetweenTurns = 10f;
                p.GetComponent<Image>().fillAmount = 1;


                if(playerCount == PokerPlayers.Length - 1){
                    EveryoneHasPlayed = true;
                    round++;
                    setPlayerCount(0);

                }
                else{
                    setPlayerCount(playerCount + 1);
                }
            }
            else{
                timeBetweenTurns -= Time.deltaTime;
                changeTimer(playerCount,timeBetweenTurns);
            }
        }
 
        if(!GameOver && EveryoneHasPlayed){

            switch(round){
                case 1:
                    dealer.flop();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
                case 2:
                    dealer.dealCard();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
                case 3:
                    dealer.dealCard();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
            }
        }
 
        if(round == 4  || CheckGameOver()){
            
            round = 0;
            calculateWinner.CalculateHands();

            for(int i = 0; i < PokerPlayers.Length; i++){
                PokerPlayers[i].GetComponent<TimerController>().isPlaying = true;
                PokerPlayers[i].GetComponent<TimerController>().isTurn = false;
            }

            restartRound();
        }
       
    }
 
    bool CheckGameOver(){
        int count = 0;
        for(int i = 0; i < PokerPlayers.Length; i++){
            if(PokerPlayers[i].GetComponent<TimerController>().isPlaying){
                count++;
            }
        }

        if(count <= 1){
            return true;
        }

        return false;
    }

    #endregion
 
    #region "Events"
    public void changeTimer(int playerCount, float timeBetweenTurns){
        object[] content = new object[] {playerCount, timeBetweenTurns/10f};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.START_TIMER_EVENT, content, raiseEventOptions, SendOptions.SendUnreliable);
        //Debug.Log("Sending Event");
    }

    void setPlayerCount(int value){
        object[] content = new object[]{value};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.INCREMENT_COUNT, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
 
    void restartRound(){
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        object[] content = new object[]{};

 
        PhotonNetwork.RaiseEvent(EventController.RESTART_GAME, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    public void setTurnTrue(int playerCount){
        object[] content = new object[] {playerCount};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.SET_TURN_TRUE, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    void resetCurrentBet(){
        object[] content = new object[]{};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.RESET_CURRENT_BET, content, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public void EnableButtons(){
        int index = -1;

        for(int i = 0; i < PokerPlayers.Length; i++){
            if(PhotonNetwork.PlayerList[i].IsLocal){
                index = i;
            }
        }
        
        bool isTurn = PokerPlayers[index].GetComponent<TimerController>().isTurn;

        if(!isTurn){
            uICOntroller.DisableButton();
        }
        
        if(isTurn && currentBet > 0){
            uICOntroller.EnableButtonWithBet();
        }

        if(isTurn && currentBet == 0){
            uICOntroller.EnableButtonWithoutBet();
        }
    }
    
    #endregion

    public void setTurnTrueEvent(object[] data){
            PokerPlayers[(int)data[0]].GetComponent<TimerController>().setTurnTrue();
            for(int i = 0; i < PokerPlayers.Length; i++){
                if(i != (int)data[0]){
                    PokerPlayers[i].GetComponent<TimerController>().setTurnFalse();
                }
            }
    }

    public void RestartEvent(){
            timeBetweenTurns = 10f;
            playerCount = 0;
            gameStarted = false;
            GameOver = false;
            EveryoneHasPlayed = false;
            currentBet = 0;
            round = 0;
            totalBet = 0;

            calculateWinner.Reset();
            

            dealer.dealerReset();
            uICOntroller.resetUI();
            calculateWinner.Reset();
            

            PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");




            for(int i = 0; i < PokerPlayers.Length; i++){
                PokerPlayers[i].GetComponent<TimerController>().isPlaying = true;
                PokerPlayers[i].GetComponent<TimerController>().isTurn = false;
            }

            if(PhotonNetwork.IsMasterClient){
                gameStarted  = true;
                p = PokerPlayers[0];
                dealer.dealCardToPlayer();
                setTurnTrue(0);
            }
    }


}