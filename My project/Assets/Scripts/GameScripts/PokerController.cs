using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerContoller : MonoBehaviour
{
    public GameObject[] PokerPlayers;
    bool gameStarted = false;
    [SerializeField] float timeBetweenTurns = 0f;
    int playerCount = -1;
    GameObject p = null;


    public void OnStartGameButtonClicked(){
        PokerPlayers = GameObject.FindGameObjectsWithTag("Timer");
        gameStarted  = true;
        p = PokerPlayers[0];
    }


    private void Update() {
        if(!gameStarted){
            return;
        }

        if(timeBetweenTurns == 0){
            timeBetweenTurns = 10f;
            p.GetComponent<Image>().fillAmount = 1;
            playerCount = playerCount <= PokerPlayers.Length - 1 ? playerCount + 1 : 0;
            p = PokerPlayers[playerCount];
        }
        else{
            timeBetweenTurns -= Time.deltaTime;
            
            p.GetComponent<Image>().fillAmount = timeBetweenTurns / 10f; 
        }
        


    }






}
