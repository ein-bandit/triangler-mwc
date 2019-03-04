using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class AIPlayer : PlayerMovement, IPlayer
{
    public float activateStealthDelay = 10f;
    public float stealthActiveTime = 2f;
    public float playerDeathResetDelay = .15f;


    private Renderer _renderer;
    private Renderer _noseRenderer;
    private Material _material;
    private Collider _collider;
    private PlayerManager playerManager;

    private Projectile projectile;


    private bool blockSteering = false;

    private bool canActivateStealth = false;

    private bool projectileReady = false;

    private LayerMask playerMask;

    private Color playerColor;

    public int maxActionRandom = 100;
    [Range(0f, 1f)]
    public float shootProbability = .8f;
    [Range(0f, 1f)]
    public float stealthOrBoost = .5f;
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
        _collider = GetComponent<Collider>();

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

                int random = Random.Range(0, maxActionRandom);

                if (random <= maxActionRandom * shootProbability && projectileReady)
                {
                    Debug.Log("AI Fire");
                    projectile.Fire(transform.position, transform.rotation);
                }
                else
                {
                    Debug.Log($"did not shoot {random} <= {maxActionRandom * shootProbability},{projectileReady}");
                }

                float randomWaitTime = Random.Range(calculateActionTimeStepMin, calculateActionTimeStepMax);
                Debug.Log($"AI waiting {randomWaitTime}");
                yield return new WaitForSeconds(randomWaitTime);
            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward * -1), out hit, Mathf.Infinity, playerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * -1) * hit.distance, Color.yellow);

                int random = Random.Range(0, maxActionRandom);

                if (random <= maxActionRandom * stealthOrBoost)
                {
                    Debug.Log("AI Boost");
                    boostActive = true;
                }
                else
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
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * -1) * 1000, Color.blue);
                if (!blockSteering && Random.Range(0f, maxActionRandom * steeringProbability) <= maxActionRandom * steeringProbability)
                {
                    currentRotation = Random.Range(-1f, 1f);

                    float waitTime = Random.Range(calculateActionTimeStepMin, calculateActionTimeStepMax);

                    //Debug.Log($"AI reenable steering in {waitTime}");
                    blockSteering = true;
                    Invoke("ResetBlockSteering", waitTime);
                }
            }

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
    }

    public void ProjectileDetonated()
    {
        projectileReady = true;
    }

    private void EnableFeatures()
    {
        projectileReady = true;
        canActivateStealth = true;
    }

    public void HitByProjectile()
    {
        Debug.Log("ai hit by projectile");
        StopCoroutine(CalculateActionCoroutine);
        StopMovement();
        StartCoroutine(DeathRotation());
    }

    public void ActivatePlayerObject(bool active)
    {
        Debug.Log($"de/activating ai player {active}");
        gameObject.SetActive(active);
        if (active)
        {
            ResetPlayerMovement();
        }
    }

    public void DisablePlayer()
    {
        Debug.Log("disabling ai player");
        DeactivateMovement();
    }

    private IEnumerator DeathRotation()
    {
        _collider.enabled = false;
        yield return ExecuteDeathRotation();
        yield return new WaitForSeconds(playerDeathResetDelay);
        DeactivateMovement();
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
        _collider.enabled = true;
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
