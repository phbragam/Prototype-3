using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class PlayerMethodsScript : MonoBehaviourPunCallbacks
{
    public GameObject playerModel;
    public GameObject playerUI;
    public int deaths;

    // Start is called before the first frame update
    void Start()
    {
        deaths = int.Parse((string)photonView.Owner.CustomProperties["Deaths"]);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<Transform>().position.y <= -5)
        {
            Die();
        }
    }

    void Die()
    {
        if (photonView.IsMine)
        {
            AttDeathsData();
        }
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        SceneTextsManager STM = GameObject.Find("GameManager").GetComponent<SceneTextsManager>();

        gameObject.GetComponent<PhotonView>().RPC("ActivateDeactivatePlayerModel", RpcTarget.AllBuffered, false);
        transform.GetComponent<MovementController>().enabled = false;
        transform.GetComponent<Rigidbody>().isKinematic = true;

        int randomX = Random.Range(0, 50);
        int randomZ = Random.Range(-10, 10);
        transform.position = new Vector3(randomX, 50, randomZ);

        if (photonView.IsMine)
        {
            STM.respawnText.text = "You are dead. Respawning soon";
        }
        
        yield return new WaitForSeconds(7.0f);

        if (photonView.IsMine)
        {
            STM.respawnText.text = "";
        }

        gameObject.GetComponent<PhotonView>().RPC("ActivateDeactivatePlayerModel", RpcTarget.AllBuffered, true);
        transform.GetComponent<MovementController>().enabled = true;
        transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    [PunRPC]
    void ActivateDeactivatePlayerModel(bool option)
    {
        playerModel.SetActive(option);
        playerUI.SetActive(option);
    }

    #region Send data to PlayFab 

    public void AttDeathsData()
    {
        deaths++;
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("Deaths", deaths.ToString());
        PhotonNetwork.SetPlayerCustomProperties(hash);

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"Deaths", (string)photonView.Owner.CustomProperties["Deaths"]}
        };

        var request = new UpdateUserDataRequest() { Data = data };
        PlayFabClientAPI.UpdateUserData(request, SetUserDataSuccess, SetUserDataFailure);
    }

    private void SetUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Deaths data updated");
    }

    private void SetUserDataFailure(PlayFabError error)
    {
        Debug.Log("Deaths data update failure");
    }

    #endregion

}
