using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class MovementController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private GameObject playerCamera;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraUpAndDownRotation = 0f;
    private float currentCameraUpAndDownRotation = 0f;
    private Rigidbody rb;

    public int hits;


    // Start is called before the first frame update
    void Start()
    {
        hits = int.Parse((string)photonView.Owner.CustomProperties["Hits"]);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // movement
        Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // rotation
        Rotate(Input.GetAxis("Mouse X"));

        // camera up and down rotation
        RotateCameraUpDown(Input.GetAxis("Mouse Y"));
    }

    private void FixedUpdate()
    {
        // move
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        //rotate
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

        // rotate camera
        if (playerCamera != null)
        {
            currentCameraUpAndDownRotation -= cameraUpAndDownRotation;
            currentCameraUpAndDownRotation = Mathf.Clamp(currentCameraUpAndDownRotation, -85, 85);

            playerCamera.transform.localEulerAngles = new Vector3(currentCameraUpAndDownRotation, 0, 0);
        }
    }

    void Move(float xMovement, float zMovement)
    {
        Vector3 movementHorizontal = transform.right * xMovement;
        Vector3 movementVertical = transform.forward * zMovement;

        velocity = (movementHorizontal + movementVertical).normalized * speed;
    }

    void Rotate(float yRotation)
    {
        rotation = new Vector3(0, yRotation, 0) * lookSensitivity;
    }

    void RotateCameraUpDown(float cameraUpDownRotation)
    {
        cameraUpAndDownRotation = cameraUpDownRotation * lookSensitivity;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (photonView.IsMine && c.gameObject.CompareTag("Player"))
        {
            AttHitsData();
        }
    }

    #region Send data to PlayFab 

    public void AttHitsData()
    {
        hits++;
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("Hits", hits.ToString());
        PhotonNetwork.SetPlayerCustomProperties(hash);

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"Hits", (string)photonView.Owner.CustomProperties["Hits"] }
        };

        var request = new UpdateUserDataRequest() { Data = data };
        PlayFabClientAPI.UpdateUserData(request, SetUserDataSuccess, SetUserDataFailure);
    }

    private void SetUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Hits data updated"); 
    }

    private void SetUserDataFailure(PlayFabError error)
    {
        Debug.Log("Hits data update failure");
    }

    #endregion
}
