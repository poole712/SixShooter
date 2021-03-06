﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] KeyCode Interact;
    Vector3 forward;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        forward = cam.transform.forward;
        Debug.DrawRay(transform.position, forward * 3, Color.cyan);
        if (Physics.Raycast(transform.position, forward, out hit, 3, 1 << 9))
        {
            if (Input.GetKeyDown(Interact))
            {
                if(hit.transform.tag == "door")
                {
                    hit.transform.gameObject.GetComponent<DoorSwing>().triggered = !hit.transform.gameObject.GetComponent<DoorSwing>().triggered;
                }
                
                if(hit.transform.tag == "emit")
                {
                    hit.transform.gameObject.GetComponent<lightFlash>().check = !hit.transform.gameObject.GetComponent<lightFlash>().check;
                }
            }
        }
    }
}
