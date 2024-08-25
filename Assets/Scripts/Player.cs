using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int HP = 100;

    public void TakeDamage(int damageAmount) {
        HP -= damageAmount;

        if (HP <= 0) {
            Debug.Log("Player Dead");
            
        } else {
            Debug.Log("Player Hit");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("ZombieHand")) {
            var zombieHand = other.gameObject.GetComponent<ZombieHand>();
            if (zombieHand != null) {
                Debug.Log("ZombieHand found. Damage: " + zombieHand.damage);
                TakeDamage(zombieHand.damage);
            } else {
                Debug.LogError("No ZombieHand component found on collided object.");
            }
        }
    }
}
