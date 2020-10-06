using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class GrannyMethodsScript : MonoBehaviourPunCallbacks
{

    public GameObject playerModel;
    public GameObject playerUI;
    public int deaths;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            playerUI.SetActive(false);
        }

        deaths = int.Parse((string)photonView.Owner.CustomProperties["Deaths"]);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<Transform>().position.y <= 7)
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
        ActivateDeactivateComponents(false);

        //gameObject.GetComponent<AudioSource>().enabled = false;
        //gameObject.GetComponent<Animator>().enabled = false;
        //transform.GetComponent<GrannyControls>().enabled = false;
        //transform.GetComponent<Rigidbody>().isKinematic = true;

        int randomX = Random.Range(370, 400);
        int randomZ = Random.Range(90, 110);

        transform.position = new Vector3(randomX, 50, randomZ);

        if (photonView.IsMine)
        {
            STM.respawnText.text = "You are dead. Respawning soon";
        }

        yield return new WaitForSeconds(7.0f);

        gameObject.GetComponent<PhotonView>().RPC("ActivateDeactivatePlayerModel", RpcTarget.AllBuffered, true);

        if (photonView.IsMine)
        {
            STM.respawnText.text = "";
            playerUI.SetActive(false);
        }
        
        ActivateDeactivateComponents(true);

        //gameObject.GetComponent<AudioSource>().enabled = true;
        //gameObject.GetComponent<Animator>().enabled = true;
        //transform.GetComponent<GrannyControls>().enabled = true;
        //transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    [PunRPC]
    void ActivateDeactivatePlayerModel(bool option)
    {
        playerModel.SetActive(option);
        playerUI.SetActive(option);
    }

    void ActivateDeactivateComponents(bool option)
    {
        gameObject.GetComponent<AudioSource>().enabled = option;
        gameObject.GetComponent<Animator>().enabled = option;
        transform.GetComponent<GrannyControls>().enabled = option;
        transform.GetComponent<Rigidbody>().isKinematic = !option;
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
            {"Deaths", (string)photonView.Owner.CustomProperties["Deaths"] }
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
