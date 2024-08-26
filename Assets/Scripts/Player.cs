using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private int HP = 100;

    public GameObject bloodyScreen;

    public TextMeshProUGUI playerHealthUI;
    public GameObject gameOverUI;

    public bool isDead;

    private void Start() {
        playerHealthUI.text = $"Health: {HP}";
    }

    public void TakeDamage(int damageAmount) {
        HP -= damageAmount;

        if (HP <= 0) {
            Debug.Log("Player Dead");

            PlayerDead();
            isDead = true;
            GetComponent<ScreenFader>().StartFade();
            StartCoroutine(ShowGameOverUI());
            
        } else {
            Debug.Log("Player Hit");
            StartCoroutine(BloodyScreenEffect());
            playerHealthUI.text = $"Health: {HP}";
        }
    }

    private IEnumerator ShowGameOverUI() {
        yield return new WaitForSeconds(1f);
        gameOverUI.gameObject.SetActive(true);
    }

    private IEnumerator BloodyScreenEffect() {
        if (!bloodyScreen.activeInHierarchy) {
            bloodyScreen.SetActive(true);
        }

        var image = bloodyScreen.GetComponentInChildren<UnityEngine.UI.Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null; ; // Wait for the next frame.
        }

        if (bloodyScreen.activeInHierarchy) {
            bloodyScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("ZombieHand")) {
            if (!isDead) {
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

    private void PlayerDead() {
        GetComponent<MouseMovement>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        GetComponentInChildren<Animator>().enabled = true;
        playerHealthUI.gameObject.SetActive(false);
    }
}
