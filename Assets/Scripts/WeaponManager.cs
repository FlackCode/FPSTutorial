using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }

    public List<GameObject> weaponSlots;

    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        activeWeaponSlot = weaponSlots[0];
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
}
