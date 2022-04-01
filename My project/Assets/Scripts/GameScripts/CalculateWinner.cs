using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class CalculateWinner : MonoBehaviour
{


    public Dealer dealer;
    public GameController gameController;
    public UIController uIController;


    [Header("Temp Card Values")]
    [SerializeField] int[] valuesTo13;
    [SerializeField] int[] suitesTo13;
    [SerializeField] int[] suites;
    public int HighestHandValue;


    #region UnityFunctions

    private void Start() {
        valuesTo13 = new int[13];
        suitesTo13 = new int[13];
        suites = new int[5];
    }
    
    public void Reset(){
        Start();
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

            value = int.Parse(cardValue);  

        
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

    public void CalculateHands(){

        for(int i = 0; i < dealer.dealerCards.Length; i++){
            string cardName = dealer.dealerCards[i].GetComponent<SpriteRenderer>().sprite.name;
            ConvertCardToArray(cardName);
        }

        String cardName1 = uIController.playerCards[0].GetComponent<Image>().sprite.name;
        String cardName2 = uIController.playerCards[1].GetComponent<Image>().sprite.name;

        ConvertCardToArray(cardName1);
        ConvertCardToArray(cardName2);

        HighestHandValue = getHighestHand();
        

        for(int i = 0; i < gameController.PokerPlayers.Length; i++){
            if(PhotonNetwork.PlayerList[i].IsLocal){
                gameController.PokerPlayers[i].GetComponent<TimerController>().highestHandValue = HighestHandValue;
            }
        }

        object[] content = new object[]{};
 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
       
        PhotonNetwork.RaiseEvent(EventController.CALCULATE_WINNER, content, raiseEventOptions, SendOptions.SendReliable);

    }
    private void ConvertCardToArray(String cardName){
        CardToSuite(cardName);
        CardToValue(cardName);
    }
    public void CalculateWinnerEvent(){
            int min = 1000;
            int index = -1;
            for(int i = 0; i < gameController.PokerPlayers.Length; i++){
                if(gameController.PokerPlayers[i].GetComponent<TimerController>().highestHandValue < min){
                    index = i;
                    min = gameController.PokerPlayers[i].GetComponent<TimerController>().highestHandValue;
                }
            }

            gameController.PokerPlayers[index].GetComponent<TimerController>().Money += uIController.pot;
    }
    
    #endregion


    #region bestHandMethods
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
