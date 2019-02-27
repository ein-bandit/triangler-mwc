using UnityEngine;

public interface IPlayer
{
    void Initialize(Color color, Projectile projectile, int index);
    void StartMovement();
    void HitByProjectile();
    bool isAIControlled();
    void DestroyMe();
    void ActivatePlayerObject(bool active);
}