using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class UIController : MonoBehaviour
{

    public GameController gameController;
    public Dealer Dealer;
    public CalculateWinner calculateWinner;
    public EventController eventController;


    [Header("Player Information")]
    public GameObject[] PokerPlayerTexts; //UI
    public GameObject[] StatusTexts; //UI


    public int pot = 0; //UI
    public Text potTxt; //UI

     [Header("Buttons")] //UI
 
    public GameObject CheckBTN;
    public GameObject FoldBTN;
    public GameObject CallBTN;
    public GameObject BetBTN;
    public GameObject StartGameBTN;


    [Header("Deal information")]
    public GameObject[] playerCards; //UI

    [Header("Bet Information")]

    public int betAmount; //UI
    public Text betAmountTXT; //UI
    public GameObject BetBox; //UI





    private void Start() {
        PokerPlayerTexts = GameObject.FindGameObjectsWithTag("PlayerText");
        StatusTexts = GameObject.FindGameObjectsWithTag("Status");

        if(!PhotonNetwork.IsMasterClient)
            StartGameBTN.SetActive(false);
        
        resetStatusTexts();
        UpdatePlayerText();

        potTxt.text  = "Pot: " + pot;

    }

    public void resetUI(){
        pot = 0;
        resetStatusTexts();
        UpdatePlayerText(); 

        potTxt.text  = "Pot: " + pot;

        playerCards[0].GetComponent<Image>().color = new Color(0f,0f,0f,0f);
        playerCards[1].GetComponent<Image>().color = new Color(0f,0f,0f,0f);

        DisableButton();
    }

    public void OnStartGameButtonClicked(){

        gameController.startNewGame();

        StartGameBTN.SetActive(false);
        resetStatusTexts();
        UpdatePlayerText();
    }



    public void resetStatusTexts(){
        for(int i = 0; i < StatusTexts.Length; i++){
            StatusTexts[i].GetComponent<Text>().text = "";
        }
    }

    public void UpdatePlayerText(){
        for(int i = 0; i < gameController.PokerPlayers.Length; i++){
            PokerPlayerTexts[i].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[i].NickName, gameController.PokerPlayers[i].GetComponent<TimerController>().Money);
        }
    }

    #region Buttons



    public void DisableButton(){
        CheckBTN.SetActive(false);
        CallBTN.SetActive(false);
        FoldBTN.SetActive(false);
        BetBTN.SetActive(false);
    }
 
    public void EnableButtonWithBet(){
        CheckBTN.SetActive(false);
        CallBTN.SetActive(true);
        FoldBTN.SetActive(true);
        BetBTN.SetActive(true);

        CheckBTN.GetComponent<Button>().interactable = false;
        CallBTN.GetComponent<Button>().interactable = true;
        FoldBTN.GetComponent<Button>().interactable = true;
        BetBTN.GetComponent<Button>().interactable = true;
 
       

    }   
 
    public void EnableButtonWithoutBet(){
        CheckBTN.SetActive(true);
        CallBTN.SetActive(false);
        FoldBTN.SetActive(true);
        BetBTN.SetActive(true);

        CheckBTN.GetComponent<Button>().interactable = true;
        CallBTN.GetComponent<Button>().interactable = false;
        FoldBTN.GetComponent<Button>().interactable = true;
        BetBTN.GetComponent<Button>().interactable = true;
 
       
        CheckBTN.SetActive(true);
        CallBTN.SetActive(false);
    }
    public void OnFoldButton(){
        object[] content = new object[] {gameController.playerCount, "Fold", gameController.playerCount};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.FOLD_BTN, content, raiseEventOptions, SendOptions.SendReliable);
        
    }
 
    public void OnCallButton(){
        int betMoney = 0;

        GameObject[] PokerPlayers = gameController.PokerPlayers;
        int playerCount = gameController.playerCount;
        int currentBet = gameController.currentBet;

        if(PokerPlayers[playerCount].GetComponent<TimerController>().Money < currentBet){
            betMoney = PokerPlayers[playerCount].GetComponent<TimerController>().Money;
        }
        else{
            betMoney = currentBet;
        }
        object[] content = new object[] {"Call", betMoney, playerCount};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.CALL_BTN, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    public void OnCheckButton(){
        object[] content = new object[] {"Check"};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.CHECK_BTN, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    public void OnBetButton(){

        int playerCount = gameController.playerCount;
        if(betAmount == 0){
            BetBox.SetActive(!BetBox.activeSelf);
            betAmountTXT.text = "" + betAmount;
        }
        else{
            object[] content = new object[] {"Bet", betAmount, playerCount};

            BetBox.SetActive(!BetBox.activeSelf);
            betAmountTXT.text = "" + betAmount;

            betAmount = 0;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = ReceiverGroup.All;
        
            PhotonNetwork.RaiseEvent(EventController.BET_BTN, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void OnPotButton(){

        GameObject[] PokerPlayers = gameController.PokerPlayers;
        int playerCount = gameController.playerCount;

        if(pot < PokerPlayers[playerCount].GetComponent<TimerController>().Money){
            betAmount += pot;
            betAmountTXT.text = "" + betAmount;
        }
        
    }

    public void OnHalfPotButton(){
        GameObject[] PokerPlayers = gameController.PokerPlayers;
        int playerCount = gameController.playerCount;

        if(pot/2 < PokerPlayers[playerCount].GetComponent<TimerController>().Money){
            betAmount += pot/2;
            betAmountTXT.text = "" + betAmount;
        }
    }

    public void plus10Button(){

        GameObject[] PokerPlayers = gameController.PokerPlayers;
        int playerCount = gameController.playerCount;


        if(betAmount + 10 < PokerPlayers[playerCount].GetComponent<TimerController>().Money){
            betAmount += 10;
            betAmountTXT.text = "" + betAmount;
        }

    }

    public void minus10Button(){
        if(betAmount > 9){
            betAmount -= 10;
            betAmountTXT.text = "" + betAmount;
        }
    }
 
    #endregion

    #region ButtonEvents

    public void Fold_BTN_Event(object[] data){
        gameController.PokerPlayers[(int)data[0]].GetComponent<TimerController>().setPlayingFalse();
        gameController.PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[1];
            
    }
    
    public void Call_BTN_Event(object[] data){
        gameController.PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[0];
        pot += (int)data[1];
        gameController.PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money -= (int)data[1];
        UpdatePlayerText();        
        potTxt.text  = "Pot: " + pot;
        
        gameController.changeTimer((int)data[2], 0f);
        gameController.timeBetweenTurns = -1f;
}

    public void Check_BTN_Event(object[] data ){
        gameController.PokerPlayers[gameController.playerCount].GetComponentInChildren<Text>().text = (string)data[0];
            gameController.changeTimer(gameController.playerCount, 0f);
            gameController.timeBetweenTurns = -1f;
    }

    public void Bet_BTN_Event(object[] data){
        gameController.PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[0] + ": " + (int)data[1];
        gameController.currentBet = (int)data[1];
        pot += gameController.currentBet;
        gameController.PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money -= (int)data[1];
        PokerPlayerTexts[(int)data[2]].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[(int)data[2]].NickName, gameController.PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money);
        
        potTxt.text  = "Pot: " + pot;
        
        gameController.changeTimer((int)data[2], 0f);
        gameController.timeBetweenTurns = -1f;
}
    #endregion
    
}
