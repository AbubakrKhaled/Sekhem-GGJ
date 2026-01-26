using UnityEngine;

public abstract class Enemies : MonoBehaviour
{
    // stats that show up in the inspector
    public string enemyName;
    public float health = 100f;
    public float damage = 10f;
    public float speed = 3f;
    public float attackRange = 1.5f;
    public float attackSpeed = 1f; // wait time between attacks

    // hidden because we find it automatically in code
    [HideInInspector]
    public Transform player;

    public virtual void Start()
    {
        // find the object named "Player" so we know who to chase
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    public void TakeDamage(float amount)
    {
        // lower health
        health -= amount;

        // if health hits zero, destroy the object
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    // empty rule: every enemy must have an attack, but we don't know what it is yet
    public abstract void Attack();
}