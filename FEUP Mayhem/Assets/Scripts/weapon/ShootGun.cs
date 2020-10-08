using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ShootingEvent : UnityEvent<int, int, int, bool>
{
}

[System.Serializable]
public class EndReloadEvent : UnityEvent<int, int, int, bool>
{
}

[System.Serializable]
public class SwapWeaponEvent : UnityEvent<int, int, int, bool>
{
}

[System.Serializable]
public class OnStartEvent : UnityEvent<int, int, int, bool>
{
}

[RequireComponent(typeof(PlayerSpecs))]
public class ShootGun : MonoBehaviour
{

    public ShootingEvent onShootingEvent;

    public EndReloadEvent onEndReload;

    public SwapWeaponEvent onSwapWeapon;

    public OnStartEvent onStart;

    //Gun related
    [SerializeField]
    weaponController defaultGunScript;
    public weaponController currentGunScript;


    //Last frame's gun controller (failsafe for picking up guns)
    weaponController gunScriptLastFrame;
    //---

    //Input related
    private string fireButtonName = "Fire1";
    //---


    float currentGunCd = 0;

    //MechPerk related
    private bool rotateWeapon = false;


    // Start is called before the first frame update
    void Start()
    {
        gunScriptLastFrame = getCorrectGunScript();
        UpdateUsedGun(currentGunScript != null);

        fireButtonName = GetComponent<PlayerSpecs>().FireButtonName();

        onStart.Invoke(gunScriptLastFrame.GetAmmo(), gunScriptLastFrame.GetClipSize(), gunScriptLastFrame.GetRemainingClips(), gunScriptLastFrame.GetAutoFire());
    }

    // Update is called once per frame
    void Update()
    {
        weaponController gunScript = getCorrectGunScript();

        currentGunCd -= Time.deltaTime;
        bool autoFire = gunScript.GetAutoFire();


        if (((!autoFire && Input.GetButtonDown(fireButtonName)) || (autoFire && Input.GetButton(fireButtonName))) && currentGunCd <= 0f)
        {
            bool result;
            gunScript.ShootBullet();
            if (gunScript.GetAmmo() == 1)
            {
                currentGunCd = gunScript.GetMaxReloadCd();
                GetComponent<ReloadBar>().StartReloading();
                GameObject.Find("/Audio Objects/SFX/Reload").GetComponent<AudioSource>().Play();

            }
            else
            {
                currentGunCd = gunScript.GetMaxCd();
            }

            if (gunScript)
            {
                result = gunScript.SubtractAmmo();
                if (!result)
                {
                    Destroy(gunScript.gameObject);
                }
                weaponController temp = getCorrectGunScript();
                if (temp == defaultGunScript)
                {
                    GameObject.Find("/Audio Objects/SFX/ShotgunShoot").GetComponent<AudioSource>().Play();
                    onShootingEvent.Invoke(temp.GetAmmo() % temp.GetClipSize(), temp.GetClipSize(), -1, autoFire);
                }
                else {
                   // GameObject.Find("/Audio Objects/SFX/RifleShoot").GetComponent<AudioSource>().Play();
                    onShootingEvent.Invoke(temp.GetAmmo() % temp.GetClipSize(), temp.GetClipSize(), temp.GetRemainingClips(), autoFire);
                }
            }
        }
        if (gunScript != gunScriptLastFrame)
        {
            weaponController temp = getCorrectGunScript();
            if (gunScript == defaultGunScript)
            {
                UpdateUsedGun(false);
                //onSwapWeapon.Invoke(temp.GetAmmo(), temp.GetClipSize(), -1, temp.GetAutoFire());
            }
            else
            {
                UpdateUsedGun(true);
                //onSwapWeapon.Invoke(temp.GetAmmo(), temp.GetClipSize(), temp.GetRemainingClips(), temp.GetAutoFire());
            }
        }


        weaponController tmp = getCorrectGunScript();
        //Debug.Log(tmp.GetRemainingClips());

        //Debug.Log(currentGunCd > -Time.deltaTime && currentGunCd < 0f && tmp.GetAmmo() == 0);
        if (currentGunCd > -Time.deltaTime && currentGunCd < 0f && tmp.GetAmmo() == tmp.GetClipSize())
        {
            GetComponent<ReloadBar>().EndReload();
            onEndReload.Invoke(tmp.GetAmmo(), tmp.GetClipSize(), tmp.GetRemainingClips(), tmp.GetAutoFire());
        }

        gunScriptLastFrame = getCorrectGunScript();
    }

    public void UpdateUsedGun(bool hasSpecialGun)
    {
        if (hasSpecialGun)
        {
            defaultGunScript.gameObject.SetActive(false);
            currentGunScript.gameObject.SetActive(true);
        }
        else
        {
            defaultGunScript.gameObject.SetActive(true);
            if (currentGunScript != null)
            {
                Destroy(currentGunScript.gameObject);
            }
        }
        weaponController temp = getCorrectGunScript();
        onSwapWeapon.Invoke(temp.GetAmmo(), temp.GetClipSize(), temp.GetRemainingClips(), temp.GetAutoFire());
    }

    public void setGunScript(weaponController gun)
    {
        currentGunScript = gun;

        currentGunScript.transform.rotation = Quaternion.Euler(Vector3.zero);

        if (rotateWeapon) ChangeWeaponDirection();
    }

    public void RotateWeapon(bool rotate = true)
    {
        rotateWeapon = rotate;
        ChangeWeaponDirection();
    }

    private void ChangeWeaponDirection()
    {
        GameObject weapon = getCorrectGunScript().gameObject;
        if (rotateWeapon)
        {
            weapon.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
        else
        {
            weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public weaponController getGunScript()
    {
        return currentGunScript;
    }

    public weaponController getCorrectGunScript()
    {
        return currentGunScript == null ? defaultGunScript : currentGunScript;
    }

    
}