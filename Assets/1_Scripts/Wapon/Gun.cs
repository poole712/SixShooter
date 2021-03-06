﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    //raycast Variables
    [SerializeField] float range = 50;
    GameObject gun;
    [SerializeField] Camera myCam;
    Vector3 endPoint;
    Ray ray;

    //Fire Timer
    float timeToFire;
    bool canFire = false;
    bool reloading = false;
    public bool startReload = false;

    //Ammo
    public int ammoCount = 6;
    float timeToReload = 6;

    //Other
    [SerializeField] GameObject muzzFlash;
    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] ParticleSystem bloodParticle;
    [SerializeField] float speedModifier;
    [SerializeField] GameObject gunMesh;


    //ADS
    [SerializeField] TimeManager timeManager;
    bool aiming;
    bool canAim;
    bool canSlow;
    public bool timeSwitch = false;


    //Canvas
    [SerializeField] Text ammoCountTxt;

    //AnimatorStuff
    Animator anim;
    [SerializeField] AnimationClip shootClip;

    // Start is called before the first frame update
    void Start()
    {
        gun = this.gameObject;
        canAim = true;
        canSlow = true;
        muzzFlash.SetActive(false);
        Renderer render = gunMesh.gameObject.GetComponent<Renderer>();
        mat = render.material;
        mat.SetColor("_EmissionColor", Color.white * intenseMax);
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AccesoryFunction();


        if (aiming == true)
        {
            MoveCtrl.aimSpeedModif = speedModifier;
        }
        else
        {
            MoveCtrl.aimSpeedModif = 1;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && ammoCount >= 0 && canFire == true && !ButtonClick.isPaused)
        {
            timeToFire = 0;
            ammoCount--;
            canFire = false;
            anim.SetBool("ExitTime", false);
            Shoot();

            if (timeSwitch == true)
            {
                slowTimer = 0;
                canSlow = false;
            }
            timeSwitch = false;
        }



        //Check Ammo before firing again
        CheckAmmo();
        ADSCheck();
        ADSCool();
        


        //Reload Functions
        //if bool = true
        if (startReload == true)
        {
            StartCoroutine(Reload());
        }
        else
        {
            StopCoroutine(Reload());
        }
        //Manual
        if (ammoCount < 6 && Input.GetKeyDown(KeyCode.R))
        {
            ammoCount = 0;
            startReload = true;
        }

    }


    //when ammo above 0, shoot ray, if hit target, spawn particle effect on hit pos
    //If ammo below 0, start reload
    void Shoot()
    {
        RaycastHit hit;
        if (ammoCount > 0)
        {
            StartCoroutine(MuzzleFlash());
            if (Physics.Raycast(ray, out hit, range, 1 << 10))
            {
                Debug.DrawRay(myCam.transform.position, myCam.transform.forward * 50, Color.green);
                print("hit" + hit.transform.name);
                Instantiate(bloodParticle, hit.point, transform.rotation);
                
                if(hit.transform.tag == "critPoint")
                {
                    hit.transform.gameObject.GetComponentInParent<AIBase>().Damage(i: 10);
                }
                else if (hit.transform.tag == "regDamage")
                {
                    hit.transform.gameObject.GetComponentInParent<AIBase>().Damage(i: 4);
                }
            }
            else if (Physics.Raycast(ray, out hit, range))
            {
                Debug.DrawRay(myCam.transform.position, myCam.transform.forward * 50, Color.yellow);
                Instantiate(hitParticle, hit.point, transform.rotation);
                if (hit.transform.tag == "Destructible")
                {
                    Destroy(hit.transform.gameObject);
                    print("destoryObj");
                }
                else if (hit.transform.tag == "extendBridge")
                {
                    Animator anim = hit.transform.gameObject.GetComponentInChildren<Animator>();
                    anim.SetBool("Extend", true);
                }
                else if (hit.transform.tag == "endGame")
                {
                    //gameManager.EndGame();
                    print("GAME ENDING");
                }
                else
                {
                    print("missed");
                }
            }
            else
            {
                Debug.DrawRay(myCam.transform.position, myCam.transform.forward * 50, Color.red);
                print("missed");
            }
        }
        else
        {
            startReload = true;
        }
    }

    IEnumerator MuzzleFlash()
    {
        muzzFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzFlash.SetActive(false);
        StopCoroutine(MuzzleFlash());
    }


    public void RecoilFinish()
    {
        anim.SetBool("Firing", false);
        anim.SetBool("ExitTime", true);
        if (ammoCount > 0)
        {
            canFire = true;
            StartCoroutine(CheckFire());
        }
        print("test");
    }
    IEnumerator CheckFire()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Firing", false);
    }
    public void Unlock()
    {
        canFire = true;
    }

    public void ReloadBullet()
    {
        if (ammoCount <= 5)
        {
            ammoCount++;
            StartCoroutine(enableShoot());
        }
        if (ammoCount == 6)
        {
            startReload = false;
            anim.SetBool("ExitTime", false);
            anim.Play("Transition_from_Reload");
            canAim = true;
        }

    }
    IEnumerator enableShoot()
    {
        yield return new WaitForSeconds(0.1f);
        canFire = true;
        canAim = true;
        StopCoroutine(enableShoot());
    }

    //Ammo check
    void CheckAmmo()
    {
        if (ammoCount <= 0)
        {
            canFire = false;
            ammoCount = 0;
        }

        ammoCountTxt.text = ammoCount + "/6";
    }

    //Countdown to reload over, assign values
    IEnumerator Reload()
    {
        //animate reload
        yield return new WaitForEndOfFrame();
        timeToReload -= Time.deltaTime;
        canAim = false;
        canFire = false;


    }


    [SerializeField] Material mat;
    float intensity;
    float intenseMin = 0.8f;
    float intenseMax = 2.4f;

    void ADSCheck()
    {
        if (Input.GetKey(KeyCode.Mouse1) && canAim == true)
        {
            aiming = true;
            lerpFuncIn();
        }
        else
        {
            aiming = false;
            timeSwitch = false;
            lerpFuncOut();
        }

        if (aiming == true && Input.GetKeyDown(KeyCode.LeftShift) && lerpTime >= 1 && canSlow == true)
        {
            timeSwitch = true;
        }
        else if (canSlow == false)
        {
            timeSwitch = false;
        }

        if(timeSwitch == true)
        {
            timeManager.DoSlowmo();
            hiltBrightnessLerp();
        }
        else if (timeSwitch == false)
        {
            hiltBrightnessLerp();
            timeManager.ReduceSlowmo();
        }

        //intensity
        if (canSlow == false)
        {
            intensity = Mathf.Lerp(intenseMin, intenseMax, slowTimer);
        }
        else if (canSlow == true)
        {
            intensity = Mathf.Lerp(intenseMin, intenseMax, slowTimer);
        }
    }

    float FOVHip = 60;
    float FOVAim = 50;
    float camFOV;

    void lerpFuncIn()
    {
        lerpTimerFunc();
        //transform.position = Vector3.Lerp(GunPosBase.position, gunPosADS.position, lerpTime);
        //transform.localScale = Vector3.Lerp(GunPosBase.localScale, gunPosADS.localScale, lerpTime);
        //transform.localRotation = Quaternion.Lerp(GunPosBase.localRotation, gunPosADS.localRotation, lerpTime);
        camFOV = Mathf.Lerp(FOVHip, FOVAim, lerpTime);
    }
    void lerpFuncOut()
    {
        lerpTimerFunc();
        //transform.position = Vector3.Lerp(GunPosBase.position, gunPosADS.position, lerpTime);
        //transform.localScale = Vector3.Lerp(GunPosBase.localScale, gunPosADS.localScale, lerpTime);
        //transform.localRotation = Quaternion.Lerp(GunPosBase.localRotation, gunPosADS.localRotation, lerpTime);
        camFOV = Mathf.Lerp(FOVHip, FOVAim, lerpTime);
    }

    float hiltLerpTimer;
    void hiltBrightnessLerp()
    {
        hiltLerpTimer = Mathf.Clamp(hiltLerpTimer, 0, 1);
        if (timeSwitch == true && hiltLerpTimer < 1)
        {
            hiltLerpTimer += Time.unscaledDeltaTime / 2;
        }
        else if (timeSwitch == false && hiltLerpTimer > 0)
        {
            hiltLerpTimer -= Time.unscaledDeltaTime * 4;
        }
    }

    public float lerpTime;
    [SerializeField] float multiplier;
    void lerpTimerFunc()
    {
        lerpTime = Mathf.Clamp(lerpTime, 0, 1);

        if (aiming == true && lerpTime < 1)
        {
            lerpTime += Time.deltaTime * multiplier;
        }
        else if (aiming == false && lerpTime > 0)
        {
            lerpTime -= Time.unscaledDeltaTime * multiplier;
        }
    }

    float slowTimer;
    void ADSCool()
    {
        slowTimer = Mathf.Clamp(slowTimer, 0, 1);
        //reduce float time when zoomed and can slow time
        if (timeSwitch == true && aiming == true)
        {
            slowTimer -= (Time.unscaledDeltaTime / 3);

        }
        //increase timer when not slowTime
        else if (timeSwitch == false)
        {
            slowTimer += (Time.deltaTime / 6);
        }


        if (slowTimer <= 0)
        {
            canSlow = false;
        }
        else if (slowTimer > 0)
        {
            canSlow = true;
        }
    }


    //gameStart functions
    void AccesoryFunction()
    {
        endPoint = myCam.transform.forward * 50;
        myCam.fieldOfView = camFOV;
        ray = myCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        mat.SetColor("_EmissionColor", Color.white * intensity);
    }






    void testing()
    {
    }





}
