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

    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;

    public float spreadIntensity;

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

    void Awake() {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;
    }

    void Update()
    {
        if (isActiveWeapon) {

            GetComponent<Outline>().enabled = false;

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

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && isActiveWeapon) {
                Reload();
            }

            //if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0) {
            //    Reload();
            //}

            if (readyToShoot && isShooting && bulletsLeft > 0) {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }

            if (AmmoManager.Instance.ammoDisplay != null) {
                AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft/bulletsPerBurst}/{magazineSize/bulletsPerBurst}";
            }
        }
    }

    private void FireWeapon() {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        animator.SetTrigger("RECOIL");

        //SoundManager.Instance.shootingSound1911.Play();

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        //instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

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
        bulletsLeft = magazineSize;
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

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
