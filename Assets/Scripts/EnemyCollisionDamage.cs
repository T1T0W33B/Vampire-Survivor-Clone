using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionDamage : MonoBehaviour
{
    public float damage = 10f;
    public float damageCooldown = 1f;

    float lastDamageTime;

    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (Time.time < lastDamageTime + damageCooldown)
            return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damage);
        lastDamageTime = Time.time;

        Debug.Log("Player hit by enemy");
    }
}
