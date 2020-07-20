﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    //Camera rotate variables
    [SerializeField] string mouseXInputName, mouseYInputName;
    [SerializeField] float mouseSensitivity;

    [SerializeField] Transform playerBody;

    float xAxisClamp;

    //Headbob variables
    [SerializeField] Transform normCam;

    Vector3 CamOrigin;
    Vector3 BobPos;

    float idleCounter, movementCounter;
    [SerializeField] float speedMult;


    // Start is called before the first frame update
    void Start()
    {
        LockCursor();
        xAxisClamp = 0.0f;
        CamOrigin = normCam.transform.localPosition;
    }


    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        CameraRotation();
    }

    void CameraRotation()
    {
        float mouseX = Input.GetAxis(mouseXInputName) * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis(mouseYInputName) * mouseSensitivity * Time.deltaTime;

        xAxisClamp += mouseY;

        if(xAxisClamp > 80)
        {
            xAxisClamp = 80;
            mouseY = 0;
            ClampXRotToVal(280);
        }
        else if (xAxisClamp < -80)
        {
            xAxisClamp = -80;
            mouseY = 0;
            ClampXRotToVal(80);
        }

        transform.Rotate(Vector3.left * mouseY);
        playerBody.Rotate(Vector3.up * mouseX);


        if(MoveCtrl.forwardMovement == Vector3.zero && MoveCtrl.rightMovement == Vector3.zero)
        {
            headbob(idleCounter, 0.075f, 0.075f);
            idleCounter += Time.deltaTime;
            
        }
        else
        {
            headbob(movementCounter, 0.12f, 0.12f);
            movementCounter += Time.deltaTime * speedMult;
            
        }
        normCam.localPosition = Vector3.Lerp(normCam.localPosition, BobPos, Time.deltaTime * 1);
    }

    void ClampXRotToVal(float value)
    {
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = value;
        transform.eulerAngles = eulerRotation;
    }



    void headbob (float x, float xIntense, float yIntense)
    {
        normCam.transform.localPosition = CamOrigin + new Vector3(Mathf.Cos(x) * xIntense, Mathf.Sin(x * 2) * yIntense, 0);
    }

}
