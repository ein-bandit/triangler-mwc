using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Projectile : MonoBehaviour
{
    public float projectileResetDelay = 1f;

    public float projectileSpeed = 5f;

    private Player player;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Fire()
    {
        transform.rotation = player.transform.rotation;
        transform.position = player.transform.position;
        gameObject.SetActive(true);
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;

        _rigidbody.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Player>() == player)
        {
            return;
        }
        if (other.tag == "Wall" || other.tag == "Player")
        {
            ResetProjectile();

            if (other.tag == "Player" && other.GetComponentInParent<Player>() != player)
            {
                other.GetComponentInParent<Player>().HitByProjectile();
            }
        }
    }

    private void ResetProjectile()
    {
        gameObject.SetActive(false);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;

        player.Invoke("ResetProjectileReady", projectileResetDelay);
    }

    public void Init(Player player, Color playerColor)
    {
        this.player = player;
        //_render.material.color = playerColor;
    }
}