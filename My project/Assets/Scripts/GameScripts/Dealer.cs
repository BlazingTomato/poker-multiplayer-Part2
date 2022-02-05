using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Dealer : MonoBehaviour
{
    #region variables
    [Header("Deck of cards")]
    public List<Sprite> fullDeck;
    public List<Sprite> tempDeck;

    [Header("Deal information")]
    public GameObject[] dealerCards;
    public GameObject[] playerCards;


    PhotonView view;
    public int count;
    
    #endregion

    private void Start() {
        tempDeck = fullDeck;
        view = this.GetComponent<PhotonView>();
        count = 0;
    }

    public void dealFlop(int card1, int card2, int card3){
        dealerCards[0].GetComponent<SpriteRenderer>().sprite = tempDeck[card1];
        dealerCards[0].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
        count++;

        dealerCards[1].GetComponent<SpriteRenderer>().sprite = tempDeck[card2];
        dealerCards[1].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
        count++;

        dealerCards[2].GetComponent<SpriteRenderer>().sprite = tempDeck[card3];
        dealerCards[2].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
        count++;
        
        tempDeck.Remove(tempDeck[card1]);
        tempDeck.Remove(tempDeck[card2]);
        tempDeck.Remove(tempDeck[card3]);

        
    }
    public void dealCard(int index, int random){
        dealerCards[index].GetComponent<SpriteRenderer>().sprite = tempDeck[random];
        dealerCards[index].GetComponent<SpriteRenderer>().color = new Color(255f,255f,255f,255f);
        tempDeck.Remove(tempDeck[random]);
        count++;
    }

    public void dealCardToPlayer(){
        int card1 = UnityEngine.Random.Range(0,tempDeck.Count);
        view.RPC("discardCard", RpcTarget.AllBuffered, card1);
        int card2 = UnityEngine.Random.Range(0,tempDeck.Count);
        view.RPC("discardCard", RpcTarget.AllBuffered, card1);
    
        Sprite cardSprite1 = tempDeck[card1];
        Sprite cardSprite2 = tempDeck[card2];

        playerCards[0].GetComponent<Image>().sprite = cardSprite1;
        playerCards[1].GetComponent<Image>().sprite = cardSprite2;    
    }

    [PunRPC]
    void discardCard(int cardNumber){
        tempDeck.Remove(tempDeck[cardNumber]);
    }
    
}
