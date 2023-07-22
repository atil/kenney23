using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;

namespace Game
{
    public partial class GameMain
    {
        [Header("Player Attack")]
        [SerializeField] private Transform _swordTransform;
        [SerializeField] private Transform _swordCharge;
        [SerializeField] private Transform _swordDown;
        [SerializeField] private SphereCollider _swordDamageZone;

        private bool _isSwordAttacking = false;
        private Coroutine _attackCoroutine = null;

        private void UpdatePlayerAttack()
        {
            if (!_isSwordAttacking && Input.GetMouseButtonDown(0))
            {
                SwordAttack();
            }
        }

        private void SwordAttack()
        {
            _attackCoroutine = CoroutineStarter.Run(SwordAttackCorotuine());
        }

        private IEnumerator SwordAttackCorotuine()
        {
            _isSwordAttacking = true;

            Vector3 pos = _swordTransform.localPosition;
            Quaternion rot = _swordTransform.localRotation;
            yield return new WaitForSeconds(0.1f);
            _swordTransform.SetLocalPositionAndRotation(_swordCharge.localPosition, _swordCharge.localRotation);
            yield return new WaitForSeconds(0.3f);
            TrySwordDamage();
            _swordTransform.SetLocalPositionAndRotation(_swordDown.localPosition, _swordDown.localRotation);
            yield return new WaitForSeconds(0.2f);
            _swordTransform.SetLocalPositionAndRotation(pos, rot);

            _isSwordAttacking = false;
        }

        private void TrySwordDamage()
        {
            List<Enemy> killedEnemies = new();
            foreach (Enemy enemy in _enemies)
            {
                Collider c1 = _swordDamageZone;
                Collider c2 = enemy.Collider;
                bool hit = Physics.ComputePenetration(c1, c1.transform.position, c1.transform.rotation,
                    c2, c2.transform.position, c2.transform.rotation,
                    out Vector3 _, out float _);

                if (hit)
                {
                    Debug.Log("Hit!");
                    enemy.Health--;
                    if (enemy.Health <= 0)
                    {
                        KillEnemy(enemy);
                        killedEnemies.Add(enemy);
                    }
                }
            }

            foreach (Enemy killedEnemy in killedEnemies)
            {
                _enemies.Remove(killedEnemy);
            }
        }

        private void KillEnemy(Enemy enemy)
        {
            if (enemy.AttackCoroutine != null)
            {
                CoroutineStarter.Stop(enemy.AttackCoroutine);
            }

            // TODO: FX

            Destroy(enemy.Go);
        }

    }
}
