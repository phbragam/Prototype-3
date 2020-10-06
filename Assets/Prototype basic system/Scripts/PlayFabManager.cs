using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

public class PlayFabManager : MonoBehaviour
{

    private string userEmail;
    private string userPassword;
    private string username;
    private string playerID;
    private string weaponData;
    private string receivedWeaponData;
    private string receivedMissionCompleteData;
    private string receivedDeathsData;
    public GameObject introPanel;
    public GameObject logInPanel;
    public GameObject signUpPanel;
    public GameObject dataPanel;
    public GameObject createRoomPanel;
    public GameObject insideRoomPanel;
    public GameObject chooseRoomPanel;
    public GameObject connectionStatusPanel;
    //public static GameObject gamePanel;
    public GameObject[] textNotification = new GameObject[8];
    public InputField[] loginInputFields = new InputField[2];
    public InputField[] signUpInputFields = new InputField[3];

    // Start is called before the first frame update

    #region Unity Methods

    private void Start()
    {
        ActivatePanel(introPanel);

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "6EC06";
        }
    }

    #endregion 

    #region Gets

    public string GetReceivedWeaponData()
    {
        return receivedWeaponData;
    }

    #endregion

    #region UI Methods

    public void SetUserEmail(string emailIn)
    {
        userEmail = emailIn;
    }

    public void SetUserPassword(string passwordIn)
    {
        userPassword = passwordIn;
    }

    public void SetUsername(string usernameIn)
    {
        username = usernameIn;
    }

    public void SetWeaponData(string weaponDataIn)
    {
        weaponData = weaponDataIn;
    }

    public void OnSelectLogInPage()
    {
        ActivatePanel(logInPanel);
    }

    public void OnClickLogIn()
    {
        Login();
    }

    public void OnSelectSignUpPage()
    {
        ActivatePanel(signUpPanel);
    }

    public void OnClicksignUp()
    {
        SignUp();
    }

    public void OnClickBack()
    {
        ActivatePanel(introPanel);
        foreach (GameObject t in textNotification)
        {
            t.SetActive(false);
        }
    }

    public void OnClickSubmit()
    {
        if (!string.IsNullOrEmpty(weaponData))
        {
            SetUserData(weaponData);
        }
        else
        {
            ActivateText(textNotification[5]);
        }

    }

    #endregion

    #region Login and Sign Up

    private void Login()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in!");
        ActivateText(textNotification[3]);
        gameObject.GetComponent<PunManager>().ConnectToPhotonServer();
        connectionStatusPanel.SetActive(true);

        playerID = result.PlayFabId;
        GetAccountInfo();
        GetUserData();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
        ActivateText(textNotification[0]);
    }

    public void SignUp()
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Successfully registered!");
        ActivateText(textNotification[2]);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
        ActivateText(textNotification[1]);
    }

    #endregion

    #region Player Data

    public void GetUserData()
    {
        var request = new GetUserDataRequest() { PlayFabId = playerID, Keys = null };
        PlayFabClientAPI.GetUserData(request, GetUserDataSuccess, GetUserDataFailure);
    }

    public void GetUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("Weapon"))
        {
            Debug.Log("Weapon data obtained successfully!");

            receivedWeaponData = result.Data["Weapon"].Value;

            if (result.Data.ContainsKey("MissionComplete") && result.Data.ContainsKey("Deaths"))
            {
                Debug.Log("MissionComplete and Deaths data obtained successfully");
                receivedMissionCompleteData = result.Data["MissionComplete"].Value;
                receivedDeathsData = result.Data["Deaths"].Value;
            }
            else
            {
                UpdateUserData();
                GetUserData();
            }
            
            ActivatePanel(createRoomPanel);

            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

            hash.Add("Weapon", receivedWeaponData);
            hash.Add("MissionComplete", receivedMissionCompleteData);
            hash.Add("Deaths", receivedDeathsData);

            PhotonNetwork.SetPlayerCustomProperties(hash);
        }
        else
        {
            ActivatePanel(dataPanel);
        }
    }

    public void GetUserDataFailure(PlayFabError error)
    {
        ActivatePanel(dataPanel);
    }

    public void GetAccountInfo()
    {
        var request = new GetAccountInfoRequest() { Email = userEmail };
        PlayFabClientAPI.GetAccountInfo(request, GetAccountInfoSuccess, GetAccountInfoFailure);
    }

    public void GetAccountInfoSuccess(GetAccountInfoResult result)
    {
        Debug.Log("Get account info successfully!");
        PhotonNetwork.NickName = result.AccountInfo.Username;
    }

    public void GetAccountInfoFailure(PlayFabError error)
    {
        Debug.Log("Get account info failed!");
    }

    public void SetUserData(string weaponData)
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"Weapon", weaponData },
            {"MissionComplete", "NO" },
            {"Deaths", 0.ToString() }
        };

        var request = new UpdateUserDataRequest() { Data = data };

        PlayFabClientAPI.UpdateUserData(request, SetUserDataSuccess, SetUserDataFailure);
    }

    public void SetUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data sent successfully!");
        GetUserData();
        ActivateText(textNotification[4]);
        ActivatePanel(createRoomPanel);
    }

    public void SetUserDataFailure(PlayFabError error)
    {
        ActivateText(textNotification[5]);
    }

    // I called if there is only WeaponData and no MissionComplete/DeathsData
    public void UpdateUserData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            //{"Weapon", weaponData },
            {"MissionComplete", "NO" },
            {"Deaths", 0.ToString() }
        };

        var request = new UpdateUserDataRequest() { Data = data };

        PlayFabClientAPI.UpdateUserData(request, UpdateUserDataSuccess, UpdateUserDataFailure);
    }

    public void UpdateUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Update success");
    }

    public void UpdateUserDataFailure(PlayFabError error)
    {
        Debug.Log("Update failure");
    }

    #endregion

    #region Public Methods

    public void ActivatePanel(GameObject panel)
    {
        introPanel.SetActive(panel == introPanel);
        logInPanel.SetActive(panel == logInPanel);
        signUpPanel.SetActive(panel == signUpPanel);
        dataPanel.SetActive(panel == dataPanel);
        createRoomPanel.SetActive(panel == createRoomPanel);
        insideRoomPanel.SetActive(panel == insideRoomPanel);
        chooseRoomPanel.SetActive(panel == chooseRoomPanel);
    }

    public void ActivateText(GameObject text)
    {
        foreach (GameObject t in textNotification)
        {
            t.SetActive(text == t);
        }
    }

    public void CleanLoginInputFields()
    {
        foreach (InputField i in loginInputFields)
        {
            i.text = "";
        }
    }

    public void CleansignUpInputFields()
    {
        foreach (InputField i in signUpInputFields)
        {
            i.text = "";
        }
    }

    #endregion
}
