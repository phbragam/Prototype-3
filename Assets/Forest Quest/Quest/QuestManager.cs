using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class QuestManager : MonoBehaviourPunCallbacks
{

    public Quest quest = new Quest();
    public GameObject questPrintBox;
    public GameObject buttonPrefab;
    public GameObject victoryPopup;

    public GameObject compass;
    public GameObject compassNeedle;

    QuestEvent final;

    public GameObject A;
    public GameObject B;
    public GameObject C;
    public GameObject D;
    public GameObject E;

    void Start()
    {
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["MissionComplete"] == "NO")
        {
            //create each event
            QuestEvent a = quest.AddQuestEvent("test1", "description 1", A);
            QuestEvent b = quest.AddQuestEvent("test2", "description 2", B);
            QuestEvent c = quest.AddQuestEvent("test3", "description 3", C);
            QuestEvent d = quest.AddQuestEvent("test4", "description 4", D);
            QuestEvent e = quest.AddQuestEvent("test5", "description 5", E);

            // define the paths between the events - e.g. the order they must be completed
            quest.AddPath(a.GetId(), b.GetId());
            quest.AddPath(b.GetId(), c.GetId());
            quest.AddPath(b.GetId(), d.GetId());
            quest.AddPath(c.GetId(), e.GetId());
            quest.AddPath(d.GetId(), e.GetId());

            quest.BFS(a.GetId());

            QustButton button = CreateButton(a).GetComponent<QustButton>();
            A.GetComponent<QuestLocation>().Setup(this, a, button);
            button = CreateButton(b).GetComponent<QustButton>();
            B.GetComponent<QuestLocation>().Setup(this, b, button);
            button = CreateButton(c).GetComponent<QustButton>();
            C.GetComponent<QuestLocation>().Setup(this, c, button);
            button = CreateButton(d).GetComponent<QustButton>();
            D.GetComponent<QuestLocation>().Setup(this, d, button);
            button = CreateButton(e).GetComponent<QustButton>();
            E.GetComponent<QuestLocation>().Setup(this, e, button);

            final = e;

            quest.PrintPath();
        }

        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["MissionComplete"] == "YES")
        {
            compass.GetComponent<CompassController>().enabled = false;
            compassNeedle.SetActive(false);
            victoryPopup.SetActive(true);
        }
    }

    GameObject CreateButton(QuestEvent e)
    {
        GameObject b = Instantiate(buttonPrefab);
        b.GetComponent<QustButton>().Setup(e, questPrintBox);
        if (e.order  == 1)
        {
            b.GetComponent<QustButton>().UpdateButton(QuestEvent.EventStatus.CURRENT);
            e.status = QuestEvent.EventStatus.CURRENT;
        }
        return b;
    }

    public void UpdateQuestsOnCompletion(QuestEvent e)
    {
        if (e == final)
        {

            victoryPopup.SetActive(true);
            AttMissionCompleteData();
            compass.GetComponent<CompassController>().enabled = false;
            compassNeedle.SetActive(false);

            return;
        }

        foreach (QuestEvent n in quest.questEvents)
        {
            // if this event is the next in order
            if (n.order == (e.order +1))
            {
                // make the next in line available for completion
                n.UpdateQuestEvent(QuestEvent.EventStatus.CURRENT);
            }
        }
    }

    #region Send data to playfab

    public void AttMissionCompleteData()
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("MissionComplete", "YES");
        PhotonNetwork.SetPlayerCustomProperties(hash);

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"MissionComplete", (string)PhotonNetwork.LocalPlayer.CustomProperties["MissionComplete"] }
        };

        var request = new UpdateUserDataRequest() { Data = data };
        PlayFabClientAPI.UpdateUserData(request, SetUserDataSuccess, SetUserDataFailure);
    }

    private void SetUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("MissionComplete data updated");
    }

    private void SetUserDataFailure(PlayFabError error)
    {
        Debug.Log("MissionComplete data update failure");
    }

    #endregion

}
