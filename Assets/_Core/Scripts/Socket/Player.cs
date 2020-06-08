using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public float gravity = -9.81f;

    public float health;
    public float maxHealth = 100f;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Move();
    }

    private void Move()
    {
        //todo moving code

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;

            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;

        ServerSend.PlayerRespawned(this);
    }
}
