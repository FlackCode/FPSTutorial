using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }

    public Weapon hoveredWeapon;
    public AmmoBox hoveredAmmoBox;
    public Throwable hoveredThrowable;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Update() {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            GameObject target = hit.transform.gameObject;
            Weapon weapon = target.GetComponent<Weapon>();
            if (weapon && weapon.isActiveWeapon == false) {
                hoveredWeapon = weapon;
                    hoveredWeapon.GetComponent<Outline>().enabled = true;

                    if (Input.GetKeyDown(KeyCode.E)) {
                        WeaponManager.Instance.PickupWeapon(target.gameObject);
                    }
            } else {
                if (hoveredWeapon) {
                    hoveredWeapon.GetComponent<Outline>().enabled = false;
                }
            }

            if (target.GetComponent<AmmoBox>()) {
                hoveredAmmoBox = target.GetComponent<AmmoBox>();
                hoveredAmmoBox.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.E)) {
                    WeaponManager.Instance.PickupAmmo(hoveredAmmoBox);
                    Destroy(target);
                }
            } else {
                if (hoveredAmmoBox) {
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false;
                }
            }

            if (target.GetComponent<Throwable>()) {
                hoveredThrowable = target.GetComponent<Throwable>();
                hoveredThrowable.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.E)) {
                    WeaponManager.Instance.PickupThrowable(hoveredThrowable);
                }
            } else {
                if (hoveredThrowable) {
                    hoveredThrowable.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }
}
