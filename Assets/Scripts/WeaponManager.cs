using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }

    public List<GameObject> weaponSlots;

    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

    [Header("Throwables")]
    public float throwForce = 10f;
    public GameObject grenadePrefab;
    public GameObject smokePrefab;
    public GameObject throwableSpawn;
    public float forceMultiplier = 0;
    public float forceMultiplierLimit = 2f;

    [Header("Lethals")]
    public int lethalsCount = 0;
    public int maxLethals = 2;
    public Throwable.ThrowableType equippedLethalType;

    [Header("Tacticals")]
    public int tacticalsCount = 0;
    public int maxTacticals = 1;
    public Throwable.ThrowableType equippedTacticalType;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        activeWeaponSlot = weaponSlots[0];

        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;
    }

    private void Update() {
        foreach (GameObject weaponSlot in weaponSlots) {
            if (weaponSlot == activeWeaponSlot) {
                weaponSlot.SetActive(true);
            } else {
                weaponSlot.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwitchActiveSlot(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwitchActiveSlot(1);
        }

        if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.T)) {
            forceMultiplier += Time.deltaTime;

            if (forceMultiplier > forceMultiplierLimit) {
                forceMultiplier = forceMultiplierLimit;
            }
        }

        if (Input.GetKeyUp(KeyCode.G)) {
            if (lethalsCount > 0) {
                ThrowLethal();
            }

            forceMultiplier = 0;
        }

        if (Input.GetKeyUp(KeyCode.T)) {
            if (tacticalsCount > 0) {
                ThrowTactical();
            }

            forceMultiplier = 0;
        }
    }

    public void PickupWeapon(GameObject weapon) {
        AddWeaponIntoActiveSlot(weapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon) {

        DropCurrentWeapon(pickedUpWeapon);

        pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        Weapon weapon = pickedUpWeapon.GetComponent<Weapon>();

        pickedUpWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);

        weapon.isActiveWeapon = true;
        weapon.animator.enabled = true;
    }

    private void DropCurrentWeapon(GameObject pickedUpWeapon) {
        if (activeWeaponSlot.transform.childCount > 0) {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;

            weaponToDrop.GetComponent<Weapon>().isActiveWeapon = false;
            weaponToDrop.GetComponent<Weapon>().animator.enabled = false;

            weaponToDrop.transform.SetParent(pickedUpWeapon.transform.parent);
            weaponToDrop.transform.localPosition = pickedUpWeapon.transform.localPosition;
            weaponToDrop.transform.localRotation = pickedUpWeapon.transform.localRotation;
        }
    }

    public void SwitchActiveSlot(int slotNumber) {
        if (activeWeaponSlot.transform.childCount > 0) {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot.transform.childCount > 0) {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
        }
    }

    public void PickupAmmo(AmmoBox ammo) {
        switch (ammo.ammoType) {
            case AmmoBox.AmmoType.PistolAmmo:
                totalPistolAmmo += ammo.ammoAmount;
                break;
            case AmmoBox.AmmoType.RifleAmmo:
                totalRifleAmmo += ammo.ammoAmount;
                break;
        }
    }

    internal void DecreaseTotalAmmo(int bulletsToDecrease, WeaponModel thisWeaponModel) {
        switch (thisWeaponModel) {
            case WeaponModel.M4:
                totalRifleAmmo -= bulletsToDecrease;
                break;
            case WeaponModel.Pistol1911:
                totalPistolAmmo -= bulletsToDecrease;
                break;
        }
    }

    public int CheckAmmoLeftFor(WeaponModel thisWeaponModel) {
        switch (thisWeaponModel) {
            case WeaponModel.M4:
                return totalRifleAmmo;
            case WeaponModel.Pistol1911:
                return totalPistolAmmo;
            default:
                return 0;
        }
    }

    public void PickupThrowable(Throwable throwable) {
        switch (throwable.throwableType) {
            case Throwable.ThrowableType.Grenade:
                PickupThrowableAsLethal(Throwable.ThrowableType.Grenade);
                break;
            case Throwable.ThrowableType.Smoke:
                PickupThrowableAsTactical(Throwable.ThrowableType.Smoke);
                break;

        }
    }

    private void PickupThrowableAsLethal(Throwable.ThrowableType lethal) {
        if (equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None) {
            equippedLethalType = lethal;

            if (lethalsCount < maxLethals) {
                lethalsCount += 1;
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowables();
            } else {
                Debug.Log("Lethals limit reached");
            }
        }
    }

    private void PickupThrowableAsTactical(Throwable.ThrowableType tactical) {
        if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None) {
            equippedTacticalType = tactical;

            if (tacticalsCount < maxTacticals) {
                tacticalsCount += 1;
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowables();
            } else {
                Debug.Log("Tacticals limit reached");
            }
        }
    }

    private void ThrowLethal() {
        GameObject lethalPrefab = GetThrowablePrefab(equippedLethalType);

        GameObject throwable = Instantiate(lethalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rigidBody = throwable.GetComponent<Rigidbody>();

        rigidBody.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<Throwable>().hasBeenThrown = true;

        lethalsCount -= 1;

        if (lethalsCount <= 0) {
            equippedLethalType = Throwable.ThrowableType.None; 
        }
        HUDManager.Instance.UpdateThrowables();
    }

    private GameObject GetThrowablePrefab(Throwable.ThrowableType throwableType) {
        switch (throwableType) {
            case Throwable.ThrowableType.Grenade:
                return grenadePrefab;
            case Throwable.ThrowableType.Smoke:
                return smokePrefab;
        }
        return new();
    }

    private void ThrowTactical() {
        GameObject tacticalPrefab = GetThrowablePrefab(equippedTacticalType);

        GameObject throwable = Instantiate(tacticalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rigidBody = throwable.GetComponent<Rigidbody>();

        rigidBody.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<Throwable>().hasBeenThrown = true;

        tacticalsCount -= 1;

        if (tacticalsCount <= 0) {
            equippedTacticalType = Throwable.ThrowableType.None; 
        }
        HUDManager.Instance.UpdateThrowables();
    }
}
