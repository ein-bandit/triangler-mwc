using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class Player : PlayerMovement, IPlayer
{
    public float reactivateBoostTime = 2f;
    public float activateStealthDelay = 10f;
    public float stealthActiveTime = 2f;
    public float playerDeathResetDelay = .15f;
    public float projectileResetDelay = 1f;


    private Renderer _renderer;
    private Renderer _noseRenderer;
    private Material _material;
    private PlayerManager playerManager;

    private Projectile projectile;

    private bool canBoost = false;
    private bool canActivateStealth = false;
    private bool projectileReady = false;

    private Color playerColor;
    private float sinInitialRotation = float.MinValue;
    private float steeringRangeInPercent = .25f;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GameManager.instance.GetComponent<PlayerManager>();
        _renderer = GetComponent<Renderer>();
        _noseRenderer = transform.Find("nose").GetComponent<Renderer>();
        _material = _renderer.material;

        _material.color = this.playerColor;
    }

    private void Update()
    {
        if (projectile && Input.GetKeyUp(KeyCode.F))
        {
            projectile.Fire(transform.position, transform.rotation);
        }
    }

    public void StartMovement()
    {
        EnableFeatures();
        ActivateMovement();
    }

    public void ReceiveInput(InputDataType type, object inputData)
    {
        //Debug.Log($"received input in player: {type},{inputData}");
        switch (type)
        {
            case InputDataType.orientation:
                if (sinInitialRotation == float.MinValue)
                {
                    sinInitialRotation = Mathf.Cos(((Vector3)inputData).x * Mathf.Deg2Rad);
                    //phone was initialized in default position - game can start.
                }
                //TODO: find a fix for holding phone inverted.
                float sinRotation = Mathf.Cos(((Vector3)inputData).x * Mathf.Deg2Rad);

                currentRotation = Mathf.Clamp(sinRotation - sinInitialRotation, -steeringRangeInPercent, steeringRangeInPercent) / steeringRangeInPercent;
                //Debug.Log($"calculated deviceorientation {((Vector3)inputData).x}, {sinRotation} - {sinInitialRotation} : +-{steeringRangeInPercent}= {currentRotation}");
                break;
            case InputDataType.tap:
                //Debug.Log($"received string {(string)inputData}");
                switch ((string)inputData)
                {
                    case "tap-area-boost":
                        if (canBoost)
                        {
                            boostActive = true;
                            canBoost = false;
                            Invoke("ReactivateBoost", reactivateBoostTime);
                        }
                        break;
                    case "tap-area-stealth":
                        if (canActivateStealth)
                        {
                            EnableStealth();
                        }
                        break;
                    case "tap-area-fire":
                        if (projectileReady)
                        {
                            projectile.Fire(transform.position, transform.rotation);
                            projectileReady = false;
                        }
                        break;
                    case "reset-orientation":
                        //reset position if something went wrong?
                        sinInitialRotation = ((Vector3)inputData).x;
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
        _renderer.enabled = false;
        _noseRenderer.enabled = false;
        canActivateStealth = false;
        Invoke("ResetInvisible", stealthActiveTime);
    }

    public void Initialize(Color color, Projectile projectile, int index)
    {
        this.playerColor = color;
        this.projectile = projectile;

        InitializeMovement(index);
    }

    private void ResetInvisible()
    {
        _renderer.enabled = true;
        _noseRenderer.enabled = true;
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

    public void ProjectileDetonated()
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
        DeactivateMovement();
        StartCoroutine(DeathRotation());
        playerManager.SendMessageToClient(this, "hit");
    }

    public void ActivatePlayerObject(bool active)
    {
        gameObject.SetActive(active);
    }
    public void DisablePlayer()
    {
        DeactivateMovement();
    }

    private IEnumerator DeathRotation()
    {
        ExecuteDeathRotation();
        yield return new WaitForSeconds(playerDeathResetDelay);
        gameObject.SetActive(false);
        playerManager.HandlePlayerDeath(this);
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Destroy(projectile);
    }

    public bool isAIControlled()
    {
        return false;
    }

    public string GetUIIdentifier()
    {
        return playerColor.ToString();
    }
}
