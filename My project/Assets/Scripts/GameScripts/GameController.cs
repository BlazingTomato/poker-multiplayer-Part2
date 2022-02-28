using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

 
 
struct Card{
    public int index;
    public Sprite card;
}
public class GameController : MonoBehaviourPun
{
 
    #region Variables

    [Header("Player Information")]
    public GameObject[] PokerPlayers;
    public GameObject[] PokerPlayerTexts;
    public GameObject[] StatusTexts;
    [SerializeField] float timeBetweenTurns = 0f;
    public int playerCount = 0;
    GameObject p = null;
    public int HighestHandValue;
 
 
    [Header("Game States")]
    [SerializeField] bool gameStarted = false;
    [SerializeField] bool GameOver = false;
    [SerializeField] bool EveryoneHasPlayed = false;
    public int currentBet = 0;

    public int totalBet = 0;
    public int pot = 0;
    public Text potTxt;
   
 
   
    [Header("Dealer Infomation")]
 
    public int round;
 
 
    [Header("Buttons")]
 
    public GameObject CheckBTN;
    public GameObject FoldBTN;
    public GameObject CallBTN;
    public GameObject BetBTN;
    public GameObject StartGameBTN;


    [Header("Deck of cards")]
    public List<Sprite> fullDeck;
    public List<Sprite> tempDeck;

    [Header("Deal information")]
    public GameObject[] dealerCards;
    public GameObject[] playerCards;
    public int count;
    Sprite currentCard;

    [Header("Bet Information")]

    public int betAmount;
    public Text betAmountTXT;
    public GameObject BetBox;

    [Header("Temp Card Values")]
    [SerializeField] int[] valuesTo13;
    [SerializeField] int[] suitesTo13;
    [SerializeField] int[] suites;

 
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
 
    #endregion
 
    #region UnityFunctions
    private void Start() {
        PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");
        PokerPlayerTexts = GameObject.FindGameObjectsWithTag("PlayerText");
        StatusTexts = GameObject.FindGameObjectsWithTag("Status");
        tempDeck = new List<Sprite>();
        tempDeck.AddRange(fullDeck);
        count = 0;
        DisableButton();

        valuesTo13 = new int[13];
        suitesTo13 = new int[13];
        suites = new int[5];



        for(int i = 0; i < PokerPlayers.Length; i++){
                PokerPlayers[i].GetComponent<TimerController>().isPlaying = true;
        }

        if(!PhotonNetwork.IsMasterClient){
            StartGameBTN.SetActive(false);
        }

        for(int i = 0; i < StatusTexts.Length; i++){
            StatusTexts[i].GetComponent<Text>().text = "";
        }

        for(int i = 0; i < PokerPlayers.Length; i++){
            PokerPlayerTexts[i].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[i].NickName, 1000);
        }
        potTxt.text  = "Pot: " + pot;
    }
 
    public void OnStartGameButtonClicked(){

        gameStarted  = true;
        p = PokerPlayers[0];
        dealCardToPlayer();

        StartGameBTN.SetActive(false);

        for(int i = 0; i < PokerPlayers.Length; i++){
            PokerPlayerTexts[i].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[i].NickName, PokerPlayers[i].GetComponent<TimerController>().Money);
        }

        setTurnTrue(0);
    }
   
    public void OnDealButtonClicked(){
        restartRound();
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
                    flop();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
                case 2:
                    dealCard();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
                case 3:
                    dealCard();
                    EveryoneHasPlayed = false;
                    setTurnTrue(0);
                    resetCurrentBet();
                    break;
            }
        }
 
        if(round == 4  || CheckGameOver()){
            round = 0;
            CalculateWinner();

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
    private void OnEnable() {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }
 
    private void OnDisable() {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
 
    #endregion
    private void OnEvent(EventData photonEvent){
        byte eventCode = photonEvent.Code;
       
        //Debug.Log("Receiving Event");
 
        if(eventCode == START_TIMER_EVENT){
            object[] data = (object[])photonEvent.CustomData;
            int playerCount = (int)data[0];
            float fillAmount = (float)data[1];
           
           
            PokerPlayers[playerCount].GetComponent<Image>().fillAmount = fillAmount;
        }
 
        if(eventCode == DEAL_CARD_TO_PLAYER){
            object[] data = (object[])photonEvent.CustomData;
            for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++){
                if(PhotonNetwork.PlayerList[i].IsLocal){
                    Sprite cardSprite1 = tempDeck[(int)data[i*2]];
                    playerCards[0].GetComponent<Image>().sprite = cardSprite1;
                    Sprite cardSprite2 = tempDeck[(int)data[(i*2) + 1]];
                    playerCards[1].GetComponent<Image>().sprite = cardSprite2;    

                    playerCards[0].GetComponent<Image>().color = new Color(255f,255f,255f,255f);
                    playerCards[1].GetComponent<Image>().color = new Color(255f,255f,255f,255f);
                }
            }
            
            Sprite[] sprites = new Sprite[data.Length];
            for(int i = 0; i < data.Length; i++){

                sprites[i] = tempDeck[(int)data[i]];
            }
            for(int i = 0; i < data.Length; i++){
                tempDeck.Remove(sprites[i]);
            }
            
        }
 
        if(eventCode == DEAL_CARD){
            object[] data = (object[])photonEvent.CustomData;
            int random = (int)data[1];

            dealerCards[count].GetComponent<SpriteRenderer>().sprite = tempDeck[random];
            dealerCards[count].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
            tempDeck.Remove(dealerCards[count].GetComponent<SpriteRenderer>().sprite);
            count++;
        }
 
        if(eventCode == DEAL_FLOP){
            object[] data = (object[])photonEvent.CustomData;
            int card1 = (int)data[0], card2 = (int)data[1], card3 = (int)data[2];
        
            dealerCards[0].GetComponent<SpriteRenderer>().sprite = tempDeck[card1];
            dealerCards[0].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
            count++;

            dealerCards[1].GetComponent<SpriteRenderer>().sprite = tempDeck[card2];
            dealerCards[1].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
            count++;

            dealerCards[2].GetComponent<SpriteRenderer>().sprite = tempDeck[card3];
            dealerCards[2].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
            count++;

            tempDeck.Remove(dealerCards[0].GetComponent<SpriteRenderer>().sprite);
            tempDeck.Remove(dealerCards[1].GetComponent<SpriteRenderer>().sprite);
            tempDeck.Remove(dealerCards[2].GetComponent<SpriteRenderer>().sprite);
        }
 
        if(eventCode == RESTART_GAME){
            timeBetweenTurns = 10f;
            playerCount = 0;
            gameStarted = false;
            GameOver = false;
            EveryoneHasPlayed = false;
            currentBet = 0;
            pot = 0;
            round = 0;
            totalBet = 0;

            valuesTo13 = new int[13];
            suitesTo13 = new int[13];
            suites = new int[5];
            
            potTxt.text  = "Pot: " + pot;

            for(int i = 0; i < dealerCards.Length; i++){
                dealerCards[i].GetComponent<SpriteRenderer>().sprite = null;
                dealerCards[i].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,0f);
            }

            PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");
            PokerPlayerTexts = GameObject.FindGameObjectsWithTag("PlayerText");
            StatusTexts = GameObject.FindGameObjectsWithTag("Status");

            for(int i = 0; i < StatusTexts.Length; i++){
                StatusTexts[i].GetComponent<Text>().text = "";
            }

        
            tempDeck = new List<Sprite>();
            tempDeck.AddRange(fullDeck);
            
            playerCards[0].GetComponent<Image>().color = new Color(0f,0f,0f,0f);
            playerCards[1].GetComponent<Image>().color = new Color(0f,0f,0f,0f);
            count = 0;

            DisableButton();



            for(int i = 0; i < PokerPlayers.Length; i++){
                PokerPlayers[i].GetComponent<TimerController>().isPlaying = true;
                PokerPlayers[i].GetComponent<TimerController>().isTurn = false;
            }

            if(PhotonNetwork.IsMasterClient){
                gameStarted  = true;
                p = PokerPlayers[0];
                dealCardToPlayer();
                setTurnTrue(0);
            }

        }
 
        if(eventCode == SET_TURN_TRUE){
            object[] data = (object[])photonEvent.CustomData;
            PokerPlayers[(int)data[0]].GetComponent<TimerController>().setTurnTrue();
            for(int i = 0; i < PokerPlayers.Length; i++){
                if(i != (int)data[0]){
                    PokerPlayers[i].GetComponent<TimerController>().setTurnFalse();
                }
            }
        }
        
        if(eventCode == FOLD_BTN){
            object[] data = (object[])photonEvent.CustomData;
            PokerPlayers[(int)data[0]].GetComponent<TimerController>().setPlayingFalse();
            PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[1];
            Debug.Log(data[0]);
        }

        if(eventCode == CALL_BTN){
            object[] data = (object[])photonEvent.CustomData;
            PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[0];
            pot += (int)data[1];
            PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money -= (int)data[1];
            PokerPlayerTexts[(int)data[2]].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[(int)data[2]].NickName, PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money);
            
            potTxt.text  = "Pot: " + pot;
            
            changeTimer((int)data[2], 0f);
            timeBetweenTurns = -1f;
        }

        if(eventCode == CHECK_BTN){
            object[] data = (object[])photonEvent.CustomData;
            PokerPlayers[playerCount].GetComponentInChildren<Text>().text = (string)data[0];
            changeTimer(playerCount, 0f);
            timeBetweenTurns = -1f;
        }

        if(eventCode == BET_BTN){
            object[] data = (object[])photonEvent.CustomData;
            PokerPlayers[(int)data[2]].GetComponentInChildren<Text>().text = (string)data[0] + ": " + (int)data[1];
            currentBet = (int)data[1];
            pot += currentBet;
            PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money -= (int)data[1];
            PokerPlayerTexts[(int)data[2]].GetComponent<Text>().text = string.Format("{0}\n{1}", PhotonNetwork.PlayerList[(int)data[2]].NickName, PokerPlayers[(int)data[2]].GetComponent<TimerController>().Money);
            
            potTxt.text  = "Pot: " + pot;
            
            changeTimer((int)data[2], 0f);
            timeBetweenTurns = -1f;
        }

        if(eventCode == INCREMENT_COUNT){
            object[] data = (object[])photonEvent.CustomData;
            playerCount = (int)data[0];
            setTurnTrue(playerCount);
        }
    
        if(eventCode == RESET_CURRENT_BET){
            currentBet = 0;
        }

        if(eventCode == CALCULATE_WINNER){
            int min = 1000;
            int index = -1;
            for(int i = 0; i < PokerPlayers.Length; i++){
                if(PokerPlayers[i].GetComponent<TimerController>().highestHandValue < min){
                    index = i;
                    min = PokerPlayers[i].GetComponent<TimerController>().highestHandValue;
                }
            }

            PokerPlayers[index].GetComponent<TimerController>().Money += pot;
        }
    
    }
 
    #region "Events"
    void changeTimer(int playerCount, float timeBetweenTurns){
        object[] content = new object[] {playerCount, timeBetweenTurns/10f};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(START_TIMER_EVENT, content, raiseEventOptions, SendOptions.SendUnreliable);
        //Debug.Log("Sending Event");
    }

    void setPlayerCount(int value){
        object[] content = new object[]{value};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(INCREMENT_COUNT, content, raiseEventOptions, SendOptions.SendReliable);
    }
    void flop(){
        List<int> listNumbers = new List<int>();
        int number;
 
        for(int i = 0; i < 3; i++){
            do{
                number = UnityEngine.Random.Range(0,tempDeck.Count);
            }while(listNumbers.Contains(number));
 
            listNumbers.Add(number);
        }
 
        object[] content = new object[] {listNumbers[0], listNumbers[1], listNumbers[2]};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
 
        PhotonNetwork.RaiseEvent(DEAL_FLOP, content, raiseEventOptions, SendOptions.SendReliable);
    }
    void dealCard(){
        object[] content = new object[] {count, UnityEngine.Random.Range(0,tempDeck.Count)};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
 
        PhotonNetwork.RaiseEvent(DEAL_CARD, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    void dealCardToPlayer(){
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;

        List<int> listNumbers = new List<int>();
        int number;

        for(int i = 0; i < PhotonNetwork.PlayerList.Length * 2; i++){
            do{
                number = UnityEngine.Random.Range(0,tempDeck.Count);
            }while(listNumbers.Contains(number));
 
            listNumbers.Add(number);
        }
       
        object[] content = new object[listNumbers.Count];
        
        for(int i = 0; i < content.Length; i++){
            content[i] = listNumbers[i];
        }
        PhotonNetwork.RaiseEvent(DEAL_CARD_TO_PLAYER, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    void restartRound(){
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        object[] content = new object[]{};

 
        PhotonNetwork.RaiseEvent(RESTART_GAME, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    void setTurnTrue(int playerCount){
        object[] content = new object[] {playerCount};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(SET_TURN_TRUE, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    void resetCurrentBet(){
        object[] content = new object[]{};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(RESET_CURRENT_BET, content, raiseEventOptions, SendOptions.SendUnreliable);
    }
    #endregion
 
    #region "ButtonEvents"
 

    public void EnableButtons(){
        int index = -1;

        for(int i = 0; i < PokerPlayers.Length; i++){
            if(PhotonNetwork.PlayerList[i].IsLocal){
                index = i;
            }
        }
        
        bool isTurn = PokerPlayers[index].GetComponent<TimerController>().isTurn;

        if(!isTurn){
            DisableButton();
        }
        
        if(isTurn && currentBet > 0){
            EnableButtonWithBet();
        }

        if(isTurn && currentBet == 0){
            EnableButtonWithoutBet();
        }
    }
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
        object[] content = new object[] {playerCount, "Fold", playerCount};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(FOLD_BTN, content, raiseEventOptions, SendOptions.SendReliable);
        
    }
 
    public void OnCallButton(){
        int betMoney = 0;
        if(PokerPlayers[playerCount].GetComponent<TimerController>().Money < currentBet){
            betMoney = PokerPlayers[playerCount].GetComponent<TimerController>().Money;
        }
        else{
            betMoney = currentBet;
        }
        object[] content = new object[] {"Call", betMoney, playerCount};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(CALL_BTN, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    public void OnCheckButton(){
        object[] content = new object[] {"Check"};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(CHECK_BTN, content, raiseEventOptions, SendOptions.SendReliable);
    }
 
    public void OnBetButton(){
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
        
            PhotonNetwork.RaiseEvent(BET_BTN, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void OnPotButton(){
        if(pot < PokerPlayers[playerCount].GetComponent<TimerController>().Money){
            betAmount = pot;
            betAmountTXT.text = "" + betAmount;
        }
        
    }

    public void OnHalfPotButton(){
        if(pot/2 < PokerPlayers[playerCount].GetComponent<TimerController>().Money){
            betAmount = pot/2;
            betAmountTXT.text = "" + betAmount;
        }
    }

    public void plus10Button(){
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

    #region cardConversion
    //convert card string to values: starting from 2-10, those are labled as numbers so parse int will work
    //for face cards, take the format and take the beginning to the underscore
    // convert face card string to the corresponding value
    // initalize to the array
    private int CardToValue(string cardName){

        string cardValue = cardName.Substring(0,cardName.IndexOf("_"));

        int value = -1;

        try{
            value = int.Parse(cardValue);  

        }catch(Exception){
            if(cardValue.Equals("jack")){
                value = 11;
            }
            else if(cardValue.Equals("queen")){
                value = 12;
            }
            else if(cardValue.Equals("king")){
                value = 13;
            }
            else if(cardValue.Equals("ace")){
                value = 14;
            }
        }

        return value - 2;
    }
    
    //convert card suite string to value
    // get the index of the 2nd underscore to the end e.g. string = "spades"
    //convert string to corresponding values
    
    
    private int CardToSuite(string cardName){
        
        int firstIndexOfUnder = cardName.IndexOf("_");
        int secondIndexOfUnder = cardName.IndexOf("_",firstIndexOfUnder + 1);

        string cardValue = cardName.Substring(secondIndexOfUnder+1);

        //Debug.Log("Suite: " + cardValue);

        int value = -1;

        if(cardValue.Equals("clubs")){
            value = 1;
        }else if(cardValue.Equals("diamonds")){
            value = 2;
        }else if(cardValue.Equals("hearts")){
            value = 3;
        }else if(cardValue.Equals("spades")){
            value = 4;
        }

        return value;

    }

    #endregion


    #region calcMethods

    void CalculateWinner(){

        Debug.Log("Here");

        for(int i = 0; i < dealerCards.Length; i++){
            string cardName = dealerCards[i].GetComponent<SpriteRenderer>().sprite.name;
            CardToSuite(cardName);
            CardToValue(cardName);
        }

        String cardName1 = playerCards[0].GetComponent<Image>().sprite.name;
        String cardName2 = playerCards[1].GetComponent<Image>().sprite.name;

        CardToSuite(cardName1);
        CardToValue(cardName1);

        CardToSuite(cardName2);
        CardToValue(cardName2);

        HighestHandValue = getHighestHand();
        
        Debug.Log("Here2");

        for(int i = 0; i < PokerPlayers.Length; i++){
            if(PhotonNetwork.PlayerList[i].IsLocal){
                PokerPlayers[i].GetComponent<TimerController>().highestHandValue = HighestHandValue;
                Debug.Log("index: " + i + " Highest Hand Value: " + HighestHandValue);
            }
        }

        object[] content = new object[]{};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
       Debug.Log("Here3");
        PhotonNetwork.RaiseEvent(CALCULATE_WINNER, content, raiseEventOptions, SendOptions.SendReliable);

    }
    int getHighestHand(){

        if(royalStraight() && isFlushRange(12, 8)){
            return 0;
        }

        int straight = getStraight();

        if(straight != 1 && isFlushRange(straight,straight-4)){
            return 1 + 11 - straight;
        }

        int fourOfAKind = getFourOfAKind();
        
        if(fourOfAKind != -1){
            return 9 + 12 - fourOfAKind;
        }

        int triple = getTriple();
        int pair = getHighestPair();

        if(triple != -1 && pair != -1){
            return 22 + (13 - triple) * (12 - pair);
        }

        int flush = getFlush();
        int highestFlush = getHighestFlush(flush);

        if(flush != -1){
            return 200 + 12 - highestFlush;
        }

        if(straight != -1){
            return 213 + 12 - straight;
        }

        if(triple != -1){
            return 226 + 12 - triple;
        }

        int secondPair = getSecondPair(pair);
        
        if(pair != -1 && secondPair != -1){
            return 240 + (13 - pair) * (12 - secondPair);
        }

        if(pair != -1){
            return 400 + 12 - pair;
        }

        return 413 +  12  - getHighCard();

    }
    int getHighCard(){

        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (valuesTo13[i] == 1){
                return i;
            } 
        }
        return -1;

    }
    int getHighestPair(){

        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (valuesTo13[i] == 2){
                return i;
            } 
        }

        return -1;
    }

    int getSecondPair(int firstPair){

        for(int i = firstPair - 1; i >= 0; i--){
            if (valuesTo13[i] == 2){
                return i;
            } 
        }

        return -1;
    }

    int getTriple(){
        
        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (valuesTo13[i] == 3){
                return i;
            } 
        }

        return -1;
    }

    int getStraight(){
        
        int current = 12;
        int count = 1;

        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (valuesTo13[i] == 1){
                count++;
                if(count == 5){
                    return current;
                }
            } 
            else{
                current = i-1;
                count = 0;
            }
        }
        return -1;
    }

    bool royalStraight(){

        for(int i = valuesTo13.Length - 1; i >= 9; i--){
            if(valuesTo13[i] != 1){
                return false;
            }
        }
        return true;
    }

    int getFlush(){
        
        for(int i = suites.Length - 1; i >= 0; i--){
            if (suites[i] == 5){
                return i;
            } 
        }

        return -1;
    }

    bool isFlushRange(int end, int start){

        if(end == -1 || suitesTo13[end] == 0){
            return false;
        }

        int suiteValue = suitesTo13[end];

        for(int i = end; i >= start; i--){
            if(suitesTo13[i] != suiteValue){
                return false;
            }
        }

        return true;
    }

    int getFourOfAKind(){
        
        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (valuesTo13[i] == 4){
                return i;
            } 
        }

        return -1;
    }

    int getHighestFlush(int flush){
        if(flush == -1){
            return -1;
        }
        
        for(int i = valuesTo13.Length - 1; i >= 0; i--){
            if (suitesTo13[i] == flush){
                return  i;
            }
        }

        return -1;
    }

    #endregion
}
 

