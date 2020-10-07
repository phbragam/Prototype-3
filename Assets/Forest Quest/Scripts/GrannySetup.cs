using UnityEngine;
using Photon.Pun;
using TMPro;

public class GrannySetup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerCamera;

    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [SerializeField]
    private TextMeshProUGUI playerWeaponText;

    [SerializeField]
    private TextMeshProUGUI missionCompleteText;

    [SerializeField]
    private TextMeshProUGUI playerDeathsText;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            gameObject.GetComponent<GrannyControls>().enabled = true;
            playerCamera.SetActive(true);
            GameObject.Find("Compass").GetComponent<CompassController>().player = gameObject;
        }
        else
        {
            gameObject.GetComponent<GrannyControls>().enabled = false;
            playerCamera.SetActive(false);
        }

        SetGameSceneUI();
        SetPlayerUI();
    }

    private void FixedUpdate()
    {
        SetGameSceneUI();
        SetPlayerUI();
    }

    void SetGameSceneUI()
    {
        if (photonView.IsMine)
        {
            SceneTextsManager STM = GameObject.Find("GameManager").GetComponent<SceneTextsManager>();

            STM.youText.text = "Player: " + PhotonNetwork.NickName;
            STM.weaponText.text = "Weapon: " + (string)photonView.Owner.CustomProperties["Weapon"];
            STM.missionCompleteText.text = "Mission complete: " + (string)photonView.Owner.CustomProperties["MissionComplete"];
            STM.deathCountingText.text = "Deaths: " + (string)photonView.Owner.CustomProperties["Deaths"]; 
        }
    }
    
    void SetPlayerUI()
    {
        if (playerNameText != null && playerWeaponText != null && missionCompleteText != null && playerDeathsText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
            playerWeaponText.text = (string)photonView.Owner.CustomProperties["Weapon"];
            missionCompleteText.text = "Mission complete: " + (string)photonView.Owner.CustomProperties["MissionComplete"];
            playerDeathsText.text = "Deaths: " + (string)photonView.Owner.CustomProperties["Deaths"];
        }
    }
}
