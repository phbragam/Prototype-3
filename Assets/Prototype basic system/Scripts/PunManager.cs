using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PunManager : MonoBehaviourPunCallbacks
{
    public Text connectionStatusText;

    [Header("Lobby Panel")]
    public GameObject noRoomAvailableText;
    public GameObject roomCreationFailedText;

    [Header("Inside room panel")]
    public Text roomNameText;
    public Text playersInRoomText;

    public GameObject insideRoomPlayerPrefab;
    public Transform playerListContainer;
    public GameObject startGameButton;
    private Dictionary<int, GameObject> playerListGameObjects;

    [Header("Choose room panel")]
    public GameObject roomListPrefab;
    public Transform roomListContainer;
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;

    

    #region Unity Methods

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
    }

    private void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }

    #endregion

    #region Photon Methods

    public void ConnectToPhotonServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void CreateAndJoinRoom()
    {
        string randomRoomName = "Room" + Random.Range(0, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 100;

        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public IEnumerator JoinLobby()
    {
        
        yield return new WaitForSeconds(1.0f);
        PhotonNetwork.JoinLobby();
    }

    #endregion

    #region UI Methods

    public void OnClickJoinRandomRoom()
    {
        JoinRandomRoom();
    }

    public void OnClickCreateRoom()
    {
        CreateAndJoinRoom();
    }

    public void OnClickLeaveRoom()
    {
        LeaveRoom();

        if (!PhotonNetwork.InLobby)
        {
            StartCoroutine(JoinLobby());
        }

        gameObject.GetComponent<PlayFabManager>().ActivatePanel(gameObject.GetComponent<PlayFabManager>().chooseRoomPanel);
    }

    public void OnClickStartGame()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("ForestQuest");
    }

    public void OnClickShowAvailableRooms()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        gameObject.GetComponent<PlayFabManager>().ActivatePanel(gameObject.GetComponent<PlayFabManager>().chooseRoomPanel);
    }

    public void OnClickBackInChooseRoomPanel()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        gameObject.GetComponent<PlayFabManager>().ActivatePanel(gameObject.GetComponent<PlayFabManager>().createRoomPanel);
        noRoomAvailableText.SetActive(false);
        roomCreationFailedText.SetActive(false);
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Server!");
    }

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet!");
    }

    public override void OnJoinedRoom()
    {
        ClearRoomListView();
        cachedRoomList.Clear();

        Debug.Log(PhotonNetwork.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name
            + "\n"
            + PhotonNetwork.CurrentRoom.PlayerCount
            + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " players in room"
            + "\n"
            + "Weapon: " + gameObject.GetComponent<PlayFabManager>().GetReceivedWeaponData());

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        gameObject.GetComponent<PlayFabManager>().ActivatePanel(gameObject.GetComponent<PlayFabManager>().insideRoomPanel);

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }
        
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        playersInRoomText.text = PhotonNetwork.CurrentRoom.PlayerCount
            + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " Players";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(insideRoomPlayerPrefab, playerListContainer);
            PlayerListingManager PLM = tempListing.GetComponent<PlayerListingManager>();
            PLM.playerNameText.text = player.NickName;

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                PLM.youIndicator.gameObject.SetActive(true);
                PLM.playerWeaponText.text = gameObject.GetComponent<PlayFabManager>().GetReceivedWeaponData();
            }
            else
            {
                PLM.youIndicator.gameObject.SetActive(false);
                PLM.playerWeaponText.text = (string)player.CustomProperties["Weapon"];
                
            }

            playerListGameObjects.Add(player.ActorNumber, tempListing);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // there is no room available
        ActivateText(noRoomAvailableText);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined to" + PhotonNetwork.CurrentRoom.Name
            + "\n"
            + PhotonNetwork.CurrentRoom.PlayerCount
            + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " players in room"
            + "\n"
            + "Weapon: " + (string)newPlayer.CustomProperties["Weapon"]);

        playersInRoomText.text = PhotonNetwork.CurrentRoom.PlayerCount
           + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " Players";

        GameObject tempListing = Instantiate(insideRoomPlayerPrefab, playerListContainer);
        PlayerListingManager PLM = tempListing.GetComponent<PlayerListingManager>();
        PLM.playerNameText.text = newPlayer.NickName;

        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PLM.youIndicator.gameObject.SetActive(true);
            PLM.playerWeaponText.text = gameObject.GetComponent<PlayFabManager>().GetReceivedWeaponData();
        }
        else
        {
            PLM.youIndicator.gameObject.SetActive(false);
            PLM.playerWeaponText.text = (string)newPlayer.CustomProperties["Weapon"];
        }

        playerListGameObjects.Add(newPlayer.ActorNumber, tempListing);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left " + PhotonNetwork.CurrentRoom.Name
            + "\n"
            + PhotonNetwork.CurrentRoom.PlayerCount
            + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " players in room"
            + "\n"
            + "Weapon: ");

        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        playersInRoomText.text = PhotonNetwork.CurrentRoom.PlayerCount
          + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " Players";

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("You left the room!");
        for (int i = playerListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playerListContainer.GetChild(i).gameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            // removing closed, invisible, full rooms from cachedRoomList
            if(!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                // updating rooms that already exixts in cachedRoomList
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                // adding new rooms to cachedRoomList
                else
                {
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }

        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomInListGameObject = Instantiate(roomListPrefab, roomListContainer);
            RoomListingManager RLM = roomInListGameObject.GetComponent<RoomListingManager>();
            RLM.roomNameText.text = room.Name;
            RLM.roomCapacityText.text = room.PlayerCount + " / " + room.MaxPlayers;
            RLM.joinButton.onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            roomListGameObjects.Add(room.Name, roomInListGameObject);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }

    #endregion

    #region Public Methods

    public void ActivateText(GameObject text)
    {
        noRoomAvailableText.SetActive(text == noRoomAvailableText);
        roomCreationFailedText.SetActive(text == roomCreationFailedText);
    }

    #endregion

    #region Private Methods

    void OnJoinRoomButtonClicked(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    void ClearRoomListView()
    {
        foreach (GameObject roomListGameObject in roomListGameObjects.Values)
        {
            Debug.Log(roomListGameObject.GetComponent<RoomListingManager>().roomNameText.text);
            Debug.Log(roomListGameObject.name + " erased from roomListGameObjects");
            Destroy(roomListGameObject);
        }

        roomListGameObjects.Clear();
    }

    #endregion
}
