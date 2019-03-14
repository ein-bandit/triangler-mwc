using System;
using System.Collections;
using System.Collections.Generic;
using MobileWebControl.Network.Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIPlayer : PlayerMovement, IPlayer
{
    [Serializable]
    public enum AIType
    {
        chase,
        random
    }
    public float activateStealthDelay = 10f;
    public float stealthActiveTime = 2f;
    public float playerDeathResetDelay = .15f;


    private Renderer _renderer;
    private Renderer _noseRenderer;
    private Material _material;
    private Collider _collider;
    private PlayerManager playerManager;

    private Projectile projectile;


    private bool blockSteering = true;
    private bool blockSpecialAction = true;


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

    [SerializeField]
    public AIType aiType = AIType.random;
    private Player chasingPlayer;
    public float chasingRange = 250f;

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

    private IEnumerator CalculateAIActionRandom()
    {
        while (true)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, playerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

                int random = Random.Range(0, maxActionRandom);

                if ((random <= maxActionRandom * shootProbability) && projectileReady)
                {
                    Debug.Log("AI Fire");
                    projectile.Fire(transform.position, transform.rotation);
                    projectileReady = false;
                }
                else
                {
                    Debug.Log($"did not shoot {random} <= {maxActionRandom * shootProbability},{projectileReady}");
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            }

            if (!blockSpecialAction && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward * -1), out hit, Mathf.Infinity, playerMask))
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
                Debug.Log($"AI action blocked {randomWaitTime}");
                Invoke("ResetBlockSpecialAction", randomWaitTime);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * -1) * 1000, Color.blue);
            }

            if (!blockSteering && Random.Range(0f, maxActionRandom * steeringProbability) <= maxActionRandom * steeringProbability)
            {
                currentRotation = Random.Range(-1f, 1f);

                float waitTime = Random.Range(calculateActionTimeStepMin, calculateActionTimeStepMax);

                //Debug.Log($"AI reenable steering in {waitTime}");
                blockSteering = true;
                Invoke("ResetBlockSteering", waitTime);
            }

            yield return null;
        }
    }

    private IEnumerator CalculateAIActionChase()
    {
        while (true)
        {
            if (chasingPlayer == null)
            {
                //find new player;
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    float distanceSqr = (transform.position - player.transform.position).sqrMagnitude;
                    Debug.Log($"found player {player.GetInstanceID()}, {distanceSqr}");
                    if (distanceSqr < chasingRange)
                    {
                        chasingPlayer = player;
                    }
                    break;
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, playerMask))
            {
                if (projectileReady)
                {
                    projectile.Fire(transform.position, transform.rotation);
                    projectileReady = false;
                }

            }
            else if (chasingPlayer != null)
            {
                Vector3 target = Vector3.RotateTowards(transform.forward, chasingPlayer.transform.position - transform.position, Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(
                    target
                );
            }
            yield return null;
        }
    }


    private void ResetBlockSteering()
    {
        blockSteering = false;
    }

    private void ResetBlockSpecialAction()
    {
        blockSpecialAction = false;
    }


    private void EnableStealth()
    {
        _renderer.enabled = false;
        _noseRenderer.enabled = false;
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
    }

    public void ProjectileDetonated()
    {
        projectileReady = true;
        if (aiType == AIType.chase)
        {
            chasingPlayer = null;
        }
    }

    private void EnableFeatures()
    {
        projectileReady = true;

        blockSpecialAction = false;
        blockSteering = false;
    }

    public void HitByProjectile()
    {
        StopCoroutine(CalculateActionCoroutine);
        StopMovement();
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
        CalculateActionCoroutine = StartCoroutine(
            aiType == AIType.random ? CalculateAIActionRandom() : CalculateAIActionChase()
        );
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
