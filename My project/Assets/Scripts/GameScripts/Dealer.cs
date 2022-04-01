using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;




public class Dealer : MonoBehaviour
{

    public GameController gameController;
    public RaiseEventOptions raiseEventOptions;
    public UIController uIController;

    [Header("Deck of cards")]
    public List<Sprite> fullDeck;
    public List<Sprite> tempDeck;

    public GameObject[] dealerCards;

    int count;


    //Deal Card to Player
    private void Start() {
        raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        tempDeck = new List<Sprite>();
        tempDeck.AddRange(fullDeck);
        count = 0;
    }
    
    public void dealCardToPlayer(){

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

        PhotonNetwork.RaiseEvent(EventController.DEAL_CARD_TO_PLAYER, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void dealCardPlayerEvent(object[] data){
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++){
                if(PhotonNetwork.PlayerList[i].IsLocal){
                    Sprite cardSprite1 = tempDeck[(int)data[i*2]];
                    uIController.playerCards[0].GetComponent<Image>().sprite = cardSprite1;
                    Sprite cardSprite2 = tempDeck[(int)data[(i*2) + 1]];
                    uIController.playerCards[1].GetComponent<Image>().sprite = cardSprite2;    

                    uIController.playerCards[0].GetComponent<Image>().color = new Color(255f,255f,255f,255f);
                    uIController.playerCards[1].GetComponent<Image>().color = new Color(255f,255f,255f,255f);
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


    //Flop
    public void flop(){
        List<int> listNumbers = new List<int>();
        int number;
 
        for(int i = 0; i < 3; i++){
            do{
                number = UnityEngine.Random.Range(0,tempDeck.Count);
            }while(listNumbers.Contains(number));
 
            listNumbers.Add(number);
        }
 
        object[] content = new object[] {listNumbers[0], listNumbers[1], listNumbers[2]};
        PhotonNetwork.RaiseEvent(EventController.DEAL_FLOP, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void flopEvent(object[] data){
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



    //Deal Card
    public void dealCard(){
        object[] content = new object[] {count, UnityEngine.Random.Range(0,tempDeck.Count)};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
 
        PhotonNetwork.RaiseEvent(EventController.DEAL_CARD, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void dealCardEvent(object[] data){
        int random = (int)data[1];

        dealerCards[count].GetComponent<SpriteRenderer>().sprite = tempDeck[random];
        dealerCards[count].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
        tempDeck.Remove(dealerCards[count].GetComponent<SpriteRenderer>().sprite);
        count++;
    }


    public void dealerReset(){
        tempDeck = new List<Sprite>();
        tempDeck.AddRange(fullDeck);
        count = 0;

        for(int i = 0; i < dealerCards.Length; i++){
                dealerCards[i].GetComponent<SpriteRenderer>().sprite = null;
                dealerCards[i].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,0f);
        }


    }

}
