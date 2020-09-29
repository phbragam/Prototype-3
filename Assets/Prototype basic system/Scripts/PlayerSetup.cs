using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerCamera;

    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [SerializeField]
    private TextMeshProUGUI playerHitsText;

    [SerializeField]
    private TextMeshProUGUI playerDeathsText;

    [SerializeField]
    private TextMeshProUGUI playerWeaponText;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            gameObject.GetComponent<MovementController>().enabled = true;
            playerCamera.SetActive(true);
        }
        else
        {
            gameObject.GetComponent<MovementController>().enabled = false;
            playerCamera.SetActive(false);
        }

        SetGameSceneUI();
        SetPlayerUI1();
        SetPlayerUI2();
    }

    private void FixedUpdate()
    {
        SetGameSceneUI();
        SetPlayerUI2();
    }

    void SetPlayerUI1()
    {
        if (playerNameText != null && playerWeaponText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
            playerWeaponText.text = (string)photonView.Owner.CustomProperties["Weapon"]; 
        }
    }

    void SetPlayerUI2()
    {
        if (playerHitsText != null && playerDeathsText != null)
        {
            playerHitsText.text = "Hits: " + (string)photonView.Owner.CustomProperties["Hits"]; 
            playerDeathsText.text = "Deaths: " + (string)photonView.Owner.CustomProperties["Deaths"];
        }
    }

    void SetGameSceneUI()
    {
        if (photonView.IsMine)
        {
            SceneTextsManager STM = GameObject.Find("GameManager").GetComponent<SceneTextsManager>();

            STM.youText.text = "Player: " + PhotonNetwork.NickName;
            STM.weaponText.text = "Weapon: " + (string)photonView.Owner.CustomProperties["Weapon"];
            STM.hitCountingText.text = "Hits: " + (string)photonView.Owner.CustomProperties["Hits"];
            STM.deathCountingText.text = "Deaths: " + (string)photonView.Owner.CustomProperties["Deaths"];
        }
    }
}
