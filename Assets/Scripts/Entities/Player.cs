using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speedForce = 10f;
    public float rotationSpeed = 5f;
    public float boostForce = 15f;
    public float reactivateBoostTime = 2f;
    public float activateStealthDelay = 10f;

    public float playerInvisibleDelay = .15f;


    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Material _material;
    private PlayerManager playerManager;

    private Projectile projectile;


    private bool boostActivated = false;
    private bool canBoost = false;

    private bool canActivateStealth = false;

    private bool projectileReady = false;

    private float currentRotation = 0f;

    private Color playerColor;
    private float initalRotationX = float.MinValue;
    private float maxRotationX = 50;

    private bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GameManager.instance.GetComponent<PlayerManager>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _material = _renderer.material;

        _material.color = this.playerColor;
    }

    private void Update()
    {
        if (projectile && Input.GetKeyUp(KeyCode.F))
        {
            projectile.Fire();
        }
    }
    void FixedUpdate()
    {
        if (started)
        {
            _rigidbody.AddForce(transform.forward * speedForce);

            if (canBoost && boostActivated)
            {
                boostActivated = false;
                canBoost = false;
                Invoke("ReactivateBoost", reactivateBoostTime);
                _rigidbody.AddForce(transform.forward * boostForce, ForceMode.Impulse);
            }

            _rigidbody.AddRelativeTorque(transform.up * rotationSpeed * currentRotation, ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            ReflectPlayer(transform.forward, other.transform.forward);
        }
        else if (other.tag == "Player")
        {
            ReflectPlayer(transform.forward, transform.position - other.transform.position);
        }
    }

    private void ReflectPlayer(Vector3 forward, Vector3 normal)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.Reflect(forward, normal));
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(transform.forward * speedForce, ForceMode.Impulse);
    }

    public void StartPlayerMovement()
    {
        started = true;
        EnableFeatures();
    }

    public void ReceiveInput(InputDataType type, object inputData)
    {
        //Debug.Log($"received input in player: {type},{inputData}");
        switch (type)
        {
            case InputDataType.orientation:
                if (initalRotationX == float.MinValue)
                {
                    initalRotationX = ((Vector3)inputData).x;
                    //phone was initialized in default position - game can start.
                }
                float rotationX = ((Vector3)inputData).x - initalRotationX;
                //get float from -1 to 1. (lerp to new rotation ?)
                currentRotation = Mathf.Clamp(rotationX, -maxRotationX, maxRotationX) / maxRotationX;
                //Debug.Log($"calculated deviceorientation {rotationX}, {currentRotation}");
                break;
            case InputDataType.tap:
                //Debug.Log($"received string {(string)inputData}");
                switch ((string)inputData)
                {
                    case "tap-area-boost":
                        if (canBoost)
                        {
                            boostActivated = true;
                        }
                        break;
                    case "tap-area-stealth":
                        EnableStealth();
                        break;
                    case "tap-area-fire":
                        if (projectileReady)
                        {
                            Debug.Log("firing projectile");
                            projectile.Fire();
                            projectileReady = false;
                        }
                        break;
                    case "ready": //is handled in playermanager;
                        Debug.Log("should already be handled in playermanager");
                        break;
                    case "reset-orientation":
                        //reset position if something went wrong?
                        initalRotationX = ((Vector3)inputData).x;
                        break;
                    default:
                        //do nothing, tap not recognized.
                        break;
                }
                break;
            case InputDataType.proximity:
                Debug.Log($"received proximity {activateStealthDelay}");
                //false indicates sensor is blocked / covered.
                if ((bool)inputData == false)
                {
                    EnableStealth();
                }
                break;
            default:
                break;
        }
    }

    private void EnableStealth()
    {
        if (canActivateStealth)
        {
            _renderer.enabled = false;
            canActivateStealth = false;
            Invoke("ResetInvisible", 1f);
        }
    }

    public void Init(Color color, Projectile projectile, int index)
    {
        this.playerColor = color;
        this.projectile = projectile;

        transform.rotation = Quaternion.Euler(0f, 90 * index, 0f);
        transform.position = transform.position + new Vector3(
            transform.forward.x * transform.localScale.x * 2,
            transform.forward.y * transform.localScale.y * 2,
            transform.forward.z * transform.localScale.z * 2
        );
    }

    private void ResetInvisible()
    {
        _renderer.enabled = true;
        Invoke("ResetCanActivateStealth", activateStealthDelay);
    }

    private void ResetCanActivateStealth()
    {
        canActivateStealth = true;
    }

    private void ReactivateBoost()
    {
        canBoost = true;
    }

    private void ResetProjectileReady()
    {
        projectileReady = true;
    }

    private void EnableFeatures()
    {
        canBoost = true;
        projectileReady = true;
        canActivateStealth = true;
    }

    public void HitByProjectile()
    {
        started = false;
        _rigidbody.isKinematic = true;
        _rigidbody.velocity = Vector3.zero;
        StartCoroutine(DeathRotation());
        playerManager.SendMessageToClient(this, "hit");
    }

    private IEnumerator DeathRotation()
    {
        int step = 6;
        for (int i = 0; i <= 360 / step; i++)
        {
            yield return new WaitForEndOfFrame();
            transform.rotation = Quaternion.Euler(
                new Vector3(
                    transform.rotation.x,
                    transform.rotation.y + (i * step),
                    transform.rotation.z)
                );
        }
        yield return new WaitForSeconds(playerInvisibleDelay);
        gameObject.SetActive(false);
        playerManager.RegistratePlayerDeath(this);
    }
}
