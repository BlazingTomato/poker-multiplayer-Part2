using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LobbyTopPanel : MonoBehaviour
{
    // Start is called before the first frame update

    private readonly string connectionStatusMessage = "Connection Stattus: ";

    [Header("UI References")]
    public Text ConnectionStatusText;

    #region Unity

    public void Update() {
        ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
    }

    #endregion
}
