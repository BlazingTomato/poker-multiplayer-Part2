using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class TimerController : MonoBehaviour
{
    

    [Header("PlayerStates")]
    public bool isTurn;
    public bool isPlaying;

    public int Money = 1000;

    public int highestHandValue;


    private void Start() {
    }
    public void setTurnFalse(){
        isTurn = false;
    }

    public void setTurnTrue(){
        isTurn = true;
    }

    public void setPlayingFalse(){
        isPlaying = false;
    }

}
