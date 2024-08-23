using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    public Sprite emptySlot;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Update() {
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        Weapon unActiveWeapon = GetUnActiveWeaponSlot().GetComponentInChildren<Weapon>();

        if (activeWeapon) {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletsPerBurst}";
            totalAmmoUI.text = $"{activeWeapon.magazineSize / activeWeapon.bulletsPerBurst}";

            WeaponModel model = activeWeapon.thisWeaponModel;
            ammoTypeUI.sprite = GetAmmoSprite(model);

            activeWeaponUI.sprite = GetWeaponSprite(model);

            if (unActiveWeapon) {
                unActiveWeaponUI.sprite = GetWeaponSprite(unActiveWeapon.thisWeaponModel);
            }
        } else {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";

            ammoTypeUI.sprite = emptySlot;

            activeWeaponUI.sprite = emptySlot;
            unActiveWeaponUI.sprite = emptySlot;
        }
    }

    private GameObject GetUnActiveWeaponSlot() {
        foreach (GameObject weaponSlot in WeaponManager.Instance.weaponSlots) {
            if (weaponSlot != WeaponManager.Instance.activeWeaponSlot) {
                return weaponSlot;
            }
        }

        return null;
    }

    private Sprite GetWeaponSprite(WeaponModel model) {
        switch (model) {
            case WeaponModel.Pistol1911:
                return Resources.Load<GameObject>("Pistol1911_Weapon").GetComponent<SpriteRenderer>().sprite;
            case WeaponModel.M4:
                return Resources.Load<GameObject>("M4_Weapon").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private Sprite GetAmmoSprite(WeaponModel model) {
        switch (model) {
            case WeaponModel.Pistol1911:
                return Resources.Load<GameObject>("Pistol_Ammo").GetComponent<SpriteRenderer>().sprite;
            case WeaponModel.M4:
                return Resources.Load<GameObject>("Rifle_Ammo").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }
}
