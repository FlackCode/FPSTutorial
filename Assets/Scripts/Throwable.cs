using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] float delay = 3f;
    [SerializeField] float damageRadius = 30f;
    [SerializeField] float explosionForce = 1200f;

    public int grenadeDamage = 100;

    float countdown;

    bool hasExploded = false;
    public bool hasBeenThrown = false;

    public enum ThrowableType {
        None,
        Grenade,
        Smoke
    }

    public ThrowableType throwableType;

    private void Start() {
        countdown = delay;
    }

    private void Update() {
        if (hasBeenThrown) {
            countdown -= Time.deltaTime;

            if (countdown < 0f && !hasExploded) {
                Explode();
                hasExploded = true;
            }
        }
    }

    private void Explode() {
        GetThrowableEffect();
        Destroy(gameObject);
    }

    private void GetThrowableEffect() {
        switch (throwableType) {
            case ThrowableType.Grenade:
                GrenadeEffect();
                break;
            case ThrowableType.Smoke:
                SmokeGrenadeEffect();
                break;
        }
    }

    private void GrenadeEffect() {
        GameObject explosionEffect = GlobalReferences.Instance.grenadeExplosionEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        SoundManager.Instance.throwablesChannel.PlayOneShot(SoundManager.Instance.grenadeSound);

        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);

        Debug.Log("Colliders detected: " + colliders.Length);

        foreach (Collider objectInRange in colliders) {
            Rigidbody rigidBody = objectInRange.GetComponent<Rigidbody>();
            if (rigidBody != null) {
                rigidBody.AddExplosionForce(explosionForce, transform.position, damageRadius, 0.1f, ForceMode.Impulse);
            }

            if (objectInRange.gameObject.GetComponent<Enemy>()) {
                objectInRange.gameObject.GetComponent<Enemy>().TakeDamage(grenadeDamage);
            }
        }
    }

    private void SmokeGrenadeEffect() {
        GameObject smokeEffect = GlobalReferences.Instance.smokeGrenadeEffect;
        Instantiate(smokeEffect, transform.position, transform.rotation);

        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);

        Debug.Log("Colliders detected: " + colliders.Length);

        foreach (Collider objectInRange in colliders) {
            Rigidbody rigidBody = objectInRange.GetComponent<Rigidbody>();
            if (rigidBody != null) {
            }
        }
    }

}
