using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab;
    int y = 50;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (playerPrefab != null)
            {
                int randomX = Random.Range(0, 50);
                int randomZ = Random.Range(-10, 10);

                PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomX, y, randomZ), Quaternion.identity);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name
            + "\n" + "Players in room: " + PhotonNetwork.CurrentRoom.MaxPlayers);
    }
}
