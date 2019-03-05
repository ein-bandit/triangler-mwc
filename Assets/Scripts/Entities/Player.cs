using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MobileWebControl.Network.Input;
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
    private Collider _collider;
    private PlayerManager playerManager;

    private Projectile projectile;

    private bool canBoost = false;
    private bool canActivateStealth = false;
    private bool projectileReady = false;

    private Color playerColor;
    private float sinInitialRotation = float.MinValue;
    private float steeringRangeInPercent = .25f;

    void Start()
    {
        playerManager = GameManager.instance.GetComponent<PlayerManager>();
        _renderer = GetComponent<Renderer>();
        _noseRenderer = transform.Find("nose").GetComponent<Renderer>();
        _material = _renderer.material;

        _material.color = this.playerColor;

        _collider = GetComponent<Collider>();
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
        _collider.enabled = true;
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
                    Debug.Log("calculating new initial pos");
                    sinInitialRotation = Mathf.Cos(((Vector3)inputData).x * Mathf.Deg2Rad);
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
                            playerManager.SendMessageToClient(this, PlayerClientAction.boost_activated);
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
                            playerManager.SendMessageToClient(this, PlayerClientAction.fire_activated);
                        }
                        break;
                    case "tap-area-reset_orientation":
                        sinInitialRotation = float.MinValue;
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
        playerManager.SendMessageToClient(this, PlayerClientAction.stealth_activated);
        Invoke("ResetInvisible", stealthActiveTime);
    }

    public void Initialize(Color color, Projectile projectile, int index)
    {
        this.playerColor = color;
        this.projectile = projectile;
        this.playerIndex = index;

        InitializeMovement();
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
        playerManager.SendMessageToClient(this, PlayerClientAction.stealth_available);

    }

    private void ReactivateBoost()
    {
        canBoost = true;
        playerManager.SendMessageToClient(this, PlayerClientAction.boost_available);
    }

    public void ProjectileDetonated()
    {
        projectileReady = true;
        playerManager.SendMessageToClient(this, PlayerClientAction.fire_available);

    }

    private void EnableFeatures()
    {
        canBoost = true;
        projectileReady = true;
        canActivateStealth = true;
    }

    public void HitByProjectile()
    {
        StopMovement();
        playerManager.CommunicatePlayerDeathToClient(this);
        StartCoroutine(DeathRotation());
    }

    public void ActivatePlayerObject(bool active)
    {
        gameObject.SetActive(active);
        if (active)
        {
            ResetPlayerMovement();
        }
    }
    public void DisablePlayer()
    {
        DeactivateMovement();
    }

    private IEnumerator DeathRotation()
    {
        _collider.enabled = false;
        yield return ExecuteDeathRotation();
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

    public string GetUIIdentifier(bool inRGBA = false)
    {
        if (inRGBA)
        {
            return ColorUtility.ToHtmlStringRGBA(playerColor);
        }
        else
        {
            var props = playerColor.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in props)
            {
                if ((Color)prop.GetValue(null) == playerColor) { return prop.Name; }
            }
            return playerColor.ToString();
        }
    }
}
