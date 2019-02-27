using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class AIPlayer : PlayerMovement, IPlayer
{
    public float reactivateBoostTime = 2f;
    public float activateStealthDelay = 10f;
    public float stealthActiveTime = 2f;
    public float playerDeathResetDelay = .15f;


    private Renderer _renderer;
    private Renderer _noseRenderer;
    private Material _material;
    private PlayerManager playerManager;

    private Projectile projectile;


    private bool blockSteering = false;
    private bool boostActivated = false;
    private bool canBoost = false;

    private bool canActivateStealth = false;

    private bool projectileReady = false;

    private LayerMask playerMask;

    private Color playerColor;

    private float steeringRangeInPercent = .25f;

    public int maxActionRandom = 100;
    [Range(0f, 1f)]
    public float shootProbability = .8f;
    [Range(0f, 1f)]
    public float boostProbability = .5f;
    [Range(0f, 1f)]
    public float stealthProbability = .1f;
    [Range(0f, 1f)]
    public float steeringProbability = .75f;

    public float calculateActionTimeStepMin = .25f;
    public float calculateActionTimeStepMax = 5f;

    private Coroutine CalculateActionCoroutine;

    void Start()
    {
        playerManager = GameManager.instance.GetComponent<PlayerManager>();
        _renderer = GetComponent<Renderer>();
        _noseRenderer = transform.Find("nose").GetComponent<Renderer>();
        _material = _renderer.material;

        _material.color = this.playerColor;

        playerMask = LayerMask.GetMask(new string[] { "Player" });
    }


    private IEnumerator CalculateAIAction()
    {
        while (true)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, playerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

                int random = Random.Range(0, 3);

                if (random == 0)
                {
                    Debug.Log("AI Fire");
                    projectile.Fire(transform.position, transform.rotation);
                }
                else if (random == 1)
                {
                    Debug.Log("AI Boost");
                    boostActivated = true;
                }
                else if (random == 2)
                {
                    Debug.Log("AI Stealth");
                    EnableStealth();
                }

                float randomWaitTime = Random.Range(calculateActionTimeStepMin, calculateActionTimeStepMax);
                Debug.Log($"AI waiting {randomWaitTime}");
                yield return new WaitForSeconds(randomWaitTime);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
                if (!blockSteering && Random.Range(0f, maxActionRandom * steeringProbability) <= maxActionRandom * steeringProbability)
                {
                    currentRotation = Random.Range(-1f, 1f);

                    float waitTime = Random.Range(calculateActionTimeStepMin, calculateActionTimeStepMax);

                    //Debug.Log($"AI reenable steering in {waitTime}");
                    blockSteering = true;
                    Invoke("ResetBlockSteering", waitTime);
                }
            }
            //does not happen
            yield return null;
        }
    }

    private void ResetBlockSteering()
    {
        blockSteering = false;
    }


    private void EnableStealth()
    {
        if (canActivateStealth)
        {
            _renderer.enabled = false;
            _noseRenderer.enabled = false;
            canActivateStealth = false;
            Invoke("ResetInvisible", stealthActiveTime);
        }
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
        StopCoroutine(CalculateActionCoroutine);
        StartCoroutine(DeathRotation());
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

    private void OnDestroy()
    {
        Destroy(projectile);
    }

    public bool isAI()
    {
        return true;
    }

    public void StartMovement()
    {
        ActivateMovement();
        CalculateActionCoroutine = StartCoroutine(CalculateAIAction());
        EnableFeatures();
    }

    public bool isAIControlled()
    {
        return true;
    }

    public void DestroyMe()
    {
        Destroy(projectile);
    }
}
