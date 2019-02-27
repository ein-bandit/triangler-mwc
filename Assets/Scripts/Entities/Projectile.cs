using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 5f;

    private IPlayer player;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(true);

        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;

        _rigidbody.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((IPlayer)other.GetComponent<Player>() == player)
        {
            return;
        }
        if (other.tag == "Wall")
        {
            ResetProjectile();
        }
        else if (other.tag == "Player" || other.tag == "AIPlayer")
        {
            ResetProjectile();
            (other.tag == "Player"
                ? (IPlayer)other.GetComponent<Player>()
                : (IPlayer)other.GetComponent<AIPlayer>()
            ).HitByProjectile();
        }
    }

    private void ResetProjectile()
    {
        gameObject.SetActive(false);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;
    }

    public void Initialize(IPlayer player)
    {
        this.player = player;
    }
}