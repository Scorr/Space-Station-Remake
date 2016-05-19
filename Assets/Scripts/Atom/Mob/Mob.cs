using System;
using UnityEngine;

[RequireComponent(typeof(Movable))]
public class Mob : Atom {

    public Inventory inventory = new Inventory();

    [SerializeField]
    private bool dead;
    [SerializeField]
    private int health = 100;
    protected int Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health <= 0 && !dead) {
                Die();
            }
        }
    }
    public event Action OnDeath;

    public string deathsprite;

    private void Die() {
        dead = true;
        dense = false;

        if (OnDeath != null) {
            OnDeath.Invoke();
        }

        if (!string.IsNullOrEmpty(deathsprite)) {
            Sprites[0] = deathsprite;
            EventChangeSprite(Sprites);
        }
    }

    public void TakeDamage(int damage) {
        Health -= damage;
    }
}