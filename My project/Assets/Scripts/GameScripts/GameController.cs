using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameController : MonoBehaviourPun
{

    [Header("Player Information")]
    public GameObject[] PokerPlayers;
    [SerializeField] float timeBetweenTurns = 0f;
    public int playerCount = 0;
    GameObject p = null;


    [Header("Game States")]
    [SerializeField] bool gameStarted = false;
    [SerializeField] bool GameOver = false;
    [SerializeField] bool EveryoneHasPlayed = false;

    
    [Header("Dealer Infomation")]

    public Dealer dealer;
    int round;



    [Header("Event Codes")]
    public const byte START_TIMER_EVENT = 1;
    public const byte DEAL_CARD_TO_PLAYER = 2;
    public const byte DEAL_CARD = 3;
    public const byte DEAL_FLOP = 4;

    

    private void Start() {
        PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");
    }

    public void OnStartGameButtonClicked(){
        gameStarted  = true;
        p = PokerPlayers[0];
        dealCardToPlayer();
    }

    
    public void OnDealButtonClicked(){
        dealCard();
        dealCardToPlayer();
    }


    private void Update() {
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
                    playerCount = 0;
                }
                else{
                    playerCount++;
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
                    break;
                case 2:
                    dealCard();
                    EveryoneHasPlayed = false;
                    break;
                case 3:
                    dealCard();
                    EveryoneHasPlayed = false;
                    break;
            }
        }
        
        
    }

    private void OnEnable() {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable() {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent){
        byte eventCode = photonEvent.Code;
        object[] data = (object[])photonEvent.CustomData;
        //Debug.Log("Receiving Event");

        if(eventCode == START_TIMER_EVENT){
            
            int playerCount = (int)data[0];
            float fillAmount = (float)data[1];
            
            
            PokerPlayers[playerCount].GetComponent<Image>().fillAmount = fillAmount;
        }

        if(eventCode == DEAL_CARD_TO_PLAYER){
            dealer.dealCardToPlayer();
        }

        if(eventCode == DEAL_CARD){
            dealer.dealCard((int)data[0], (int)data[1]);
        }

        if(eventCode == DEAL_FLOP){
            dealer.dealFlop((int)data[0], (int)data[1], (int)data[2]);
        }

        
    }

    void changeTimer(int playerCount, float timeBetweenTurns){
        object[] content = new object[] {playerCount, timeBetweenTurns/10f};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        
        PhotonNetwork.RaiseEvent(START_TIMER_EVENT, content, raiseEventOptions, SendOptions.SendUnreliable);
        //Debug.Log("Sending Event");
    }


    void flop(){
        List<int> listNumbers = new List<int>();
        int number;

        for(int i = 0; i < 3; i++){
            do{
                number = UnityEngine.Random.Range(0,dealer.tempDeck.Count);
            }while(listNumbers.Contains(number));

            listNumbers.Add(number);
        }

        object[] content = new object[] {listNumbers[0], listNumbers[1], listNumbers[2]};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;

        PhotonNetwork.RaiseEvent(DEAL_FLOP, content, raiseEventOptions, SendOptions.SendReliable);
    }
    void dealCard(){
        int count = dealer.count;
        object[] content = new object[] {dealer.count, UnityEngine.Random.Range(0,dealer.tempDeck.Count)};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;

        PhotonNetwork.RaiseEvent(DEAL_CARD, content, raiseEventOptions, SendOptions.SendReliable);
    }

    void dealCardToPlayer(){
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        object[] content = new object[]{};
        PhotonNetwork.RaiseEvent(DEAL_CARD_TO_PLAYER, content, raiseEventOptions, SendOptions.SendReliable);
    }


}
