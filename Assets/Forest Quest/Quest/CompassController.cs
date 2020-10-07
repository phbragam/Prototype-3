﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CompassController : MonoBehaviourPunCallbacks
{
    public GameObject pointer;
    public GameObject target;
    public GameObject player;
    public RectTransform compassLine;
    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = pointer.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] v = new Vector3[4];
        compassLine.GetLocalCorners(v);
        float pointerScale = Vector3.Distance(v[1], v[2]); // both bottom corners

        Vector3 direction = target.transform.position - player.transform.position;
        float angleToTarget = Vector3.SignedAngle(player.transform.forward, direction, player.transform.up);

        angleToTarget = Mathf.Clamp(angleToTarget, -90, 90) / 180.0f * pointerScale;
        rect.localPosition = new Vector3(angleToTarget, rect.localPosition.y, rect.localPosition.z);
    }
}
