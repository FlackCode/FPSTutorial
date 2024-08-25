using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum WeaponModel {
        Pistol1911,
        M4
}

public class Weapon : MonoBehaviour
{
    //public Camera playerCamera;

    public bool isActiveWeapon;
    public int weaponDamage;

    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;

    private float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    public WeaponModel thisWeaponModel;

    public enum ShootingMode {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabLifeTime = 3f;

    public GameObject muzzleEffect;

    //internal - can access from other scripts
    internal Animator animator;

    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    private bool isADS;

    void Awake() {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;
    }

    void Update()
    {
        if (isActiveWeapon) {

            gameObject.layer = LayerMask.NameToLayer("WeaponRender");

            foreach (Transform child in transform) {
                child.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                Debug.Log(child);
            }

            if (Input.GetMouseButtonDown(1)) {
                EnterADS();
            }

            if (Input.GetMouseButtonUp(1)) {
                ExitADS();
            }

            GetComponent<Outline>().enabled = false;

            if ((WeaponManager.Instance.totalPistolAmmo == 0 && Input.GetKeyDown(KeyCode.R)) || WeaponManager.Instance.totalRifleAmmo == 0 && Input.GetKeyDown(KeyCode.R)) {
                SoundManager.Instance.emptyMagazineSound1911.Play();
            }

            if (bulletsLeft == 0 && isShooting) {
                SoundManager.Instance.emptyMagazineSound1911.Play();
            }

            if (currentShootingMode == ShootingMode.Auto) {
                //hold to shoot
                isShooting = Input.GetKey(KeyCode.Mouse0);
            } else if(currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst) {
                //click to shoot
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && isActiveWeapon && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0) {
                Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0) {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }
        } else {
            gameObject.layer = LayerMask.NameToLayer("Default");

            foreach (Transform child in transform) {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    private void EnterADS() {
        animator.SetTrigger("enterADS");
        isADS = true;
        HUDManager.Instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS() {
        animator.ResetTrigger("RECOIL_ADS");
        animator.SetTrigger("exitADS");
        isADS = false;
        HUDManager.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon() {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isADS) {
            animator.SetTrigger("RECOIL_ADS");
        } else {
            animator.SetTrigger("RECOIL");
        }

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        //instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        Bullet bul = bullet.GetComponent<Bullet>();
        bul.bulletDamage = weaponDamage;

        bullet.transform.forward = shootingDirection;

        //shoot
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        // destroy after lifetime ends
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if (allowReset) {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) {
            burstBulletsLeft--;
            FireWeapon();
        }
    }

    private void Reload() {
        //SoundManager.Instance.reloadingSound1911.Play();

        SoundManager.Instance.PlayReloadSound(thisWeaponModel);

        animator.SetTrigger("RELOAD");

        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted() {
        if (WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > magazineSize) {
            bulletsLeft = magazineSize;
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        } else {
            bulletsLeft = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }
        isReloading = false;
    }

    private void ResetShot() {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread() {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit)) {
            targetPoint = hit.point;
        } else {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = (targetPoint - bulletSpawn.position).normalized;

        //Reworked Spread

        float spreadX = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float spreadY = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        Vector3 spread = (Camera.main.transform.right * spreadX + Camera.main.transform.up * spreadY) / 10;

        Vector3 finalDirection = (direction + spread).normalized;

        return finalDirection;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
