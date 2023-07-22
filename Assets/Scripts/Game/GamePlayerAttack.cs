using JamKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class GameMain
    {
        private enum WeaponType
        {
            Sword,
            Crossbow
        }

        [Header("Player Attack")]
        [SerializeField] private Transform _swordTransform;
        [SerializeField] private Transform _swordCharge;
        [SerializeField] private Transform _swordDown;
        [SerializeField] private SphereCollider _swordDamageZone;
        [SerializeField] private Transform _crossbowTransform;

        private bool _isSwordAttacking = false;
        private Coroutine _attackCoroutine = null;
        private WeaponType _currentWeapon = WeaponType.Sword;
        private bool _isSwitchingWeapons = false;

        private Dictionary<WeaponType, GameObject> _weaponGos = new();

        private void StartPlayerAttack()
        {
            _weaponGos.Add(WeaponType.Sword, _swordTransform.gameObject);
            _weaponGos.Add(WeaponType.Crossbow, _crossbowTransform.gameObject);
        }

        private void UpdatePlayerAttack()
        {
            if (IsPlayerDead) return;

            switch (_currentWeapon)
            {
                case WeaponType.Sword:
                    if (!_isSwordAttacking && !_isSwitchingWeapons)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            SwordAttack();
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            CoroutineStarter.Run(ChangeWeapon(WeaponType.Sword, WeaponType.Crossbow));
                        }
                    }
                    break;
                case WeaponType.Crossbow:
                    if (!_isSwitchingWeapons)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            CrossbowAttack();
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            CoroutineStarter.Run(ChangeWeapon(WeaponType.Crossbow, WeaponType.Sword));
                        }
                    }
                    break;
            }
        }

        private IEnumerator ChangeWeapon(WeaponType from, WeaponType to)
        {
            _isSwitchingWeapons = true;
            const float SwitchDuration = 0.6f;

            _weaponGos[from].SetActive(false);

            yield return new WaitForSeconds(SwitchDuration);
            _weaponGos[to].SetActive(true);

            _isSwitchingWeapons = false;
            _currentWeapon = to;

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

            Sfx.Instance.Play("SwordSwing");
            TrySwordDamage();
            _swordTransform.SetLocalPositionAndRotation(_swordDown.localPosition, _swordDown.localRotation);

            yield return new WaitForSeconds(0.2f);

            _swordTransform.SetLocalPositionAndRotation(pos, rot);

            _isSwordAttacking = false;
        }

        private void TrySwordDamage()
        {
            foreach (Enemy enemy in _enemies)
            {
                Collider c1 = _swordDamageZone;
                Collider c2 = enemy.Collider;
                bool hit = Physics.ComputePenetration(c1, c1.transform.position, c1.transform.rotation,
                    c2, c2.transform.position, c2.transform.rotation,
                    out Vector3 _, out float _);

                if (hit)
                {
                    OnEnemyHit(enemy);
                }
            }

        }

        private void CrossbowAttack()
        {

        }

        private void OnEnemyHit(Enemy enemy)
        {
            enemy.Health--;

            // Interrupt enemy
            if (enemy.State == EnemyState.AttackCharge)
            {
                CoroutineStarter.Stop(enemy.AttackCoroutine);
                enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
                enemy.State = EnemyState.Move;
            }

            if (enemy.Health <= 0) // pwned
            {
                CoroutineStarter.Run(KillEnemyCoroutine(enemy));
            }
            else // hit
            {

                Sfx.Instance.PlayRandom("EnemyHit");
                PlayEnemyHitFX(enemy);
            }
        }

        private void PlayEnemyHitFX(Enemy enemy)
        {
            Vector3 originalScale = enemy.VisualTransform.localScale;
            Vector3 smallScale = originalScale.WithY(originalScale.y * 0.3f);
            enemy.VisualTransform.localScale = smallScale;
            enemy.GetDamagedCoroutine = Curve.TweenDiscrete(_globals.EnemyGetDamagedCurve, 0.3f, _globals.TweenTickDuration,
                t =>
                {
                    enemy.VisualTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
                },
                () =>
                {
                    enemy.VisualTransform.localScale = originalScale;
                    enemy.GetDamagedCoroutine = null;
                });
        }

        private IEnumerator KillEnemyCoroutine(Enemy enemy)
        {
            if (enemy.AttackCoroutine != null)
            {
                CoroutineStarter.Stop(enemy.AttackCoroutine);
            }

            enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
            enemy.State = EnemyState.Dead;

            Material enemyMaterial = enemy.VisualTransform.GetComponent<MeshRenderer>().material;
            enemyMaterial.SetTexture("_MainTex", _globals.Enemy1Die0);

            yield return new WaitForSeconds(_globals.DiscreteTickInterval);

            enemyMaterial.SetTexture("_MainTex", _globals.Enemy1Die1);

            enemy.Collider.enabled = false;
        }

    }
}
