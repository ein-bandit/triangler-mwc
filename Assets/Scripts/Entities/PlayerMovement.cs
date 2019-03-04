using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rigidbody;

    public float speedForce = 10f;
    public float rotationSpeed = 5f;
    public float boostForce = 15f;

    protected float currentRotation = 0f;

    protected bool boostActive;

    protected int playerIndex;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (!_rigidbody.isKinematic)
        {
            _rigidbody.AddForce(transform.forward * speedForce);

            if (boostActive)
            {
                _rigidbody.AddForce(transform.forward * boostForce, ForceMode.Impulse);
                boostActive = false;

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

    protected void ActivateMovement()
    {
        _rigidbody.isKinematic = false;
    }
    protected void DeactivateMovement()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.velocity = Vector3.zero;
    }

    protected IEnumerator ExecuteDeathRotation()
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
    }

    protected void InitializeMovement()
    {
        transform.rotation = Quaternion.Euler(0f, 90 * this.playerIndex, 0f);
        transform.position = transform.position + new Vector3(
            transform.forward.x * transform.localScale.x * 2,
            transform.forward.y * transform.localScale.y * 2,
            transform.forward.z * transform.localScale.z * 2
        );
    }

    protected void ResetPlayerMovement()
    {
        transform.position = Vector3.zero;
        InitializeMovement();
    }
}
