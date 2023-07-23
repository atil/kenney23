using JamKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public partial class GameMain
    {
        private enum EnemyState
        {
            Sleep,
            Move,
            AttackCharge,
            Attack,
            Dead,
        }

        private enum EnemyType
        {
            Enemy1,
            Enemy2,
        }

        private class Enemy
        {
            public EnemyType Type;
            public float MoveSpeed;
            public int Health;
            public GameObject Go;
            public Collider Collider;
            public EnemyState State;
            public NavMeshPath WalkPath;

            public Transform RootTransform;
            public Transform VisualTransform;
            public Transform ChargeSourceTransform;
            public Transform ChargeTargetTransform;

            public Coroutine AttackChargeCoroutine;
            public Coroutine GetDamagedCoroutine;
            public Coroutine PostAttackWaitCoroutine;
            public Vector3 AttackStartPos;
            public Quaternion AttackStartRot;

            public Vector3 Pos
            {
                get => Go.transform.position;
                set => Go.transform.position = value;
            }
            public bool IsAlive => Health > 0;
        }

        [Header("Enemies")]
        [SerializeField] private Transform _enemiesRoot;
        [SerializeField] private Transform _fireballsRoot;

        private List<Enemy> _enemies = new();
        private List<GameObject> _fireballs = new();

        private const int MaxRayCastResults = 20;
        private RaycastHit[] _raycastResults = new RaycastHit[MaxRayCastResults];

        private Coroutine _playerDamagedCameraFxCoroutine = null;

        private void AddEnemy(Vector3 tilePos, EnemyType type)
        {
            GameObject enemyPrefab = _globals.Enemy1Prefab;
            float moveSpeed = _globals.Enemy1Speed;
            int health = _globals.Enemy1Health;
            if (type == EnemyType.Enemy1)
            {
                enemyPrefab = _globals.Enemy1Prefab;
                moveSpeed = _globals.Enemy1Speed;
                health = _globals.Enemy1Health;
            }
            else if (type == EnemyType.Enemy2)
            {
                enemyPrefab = _globals.Enemy2Prefab;
                moveSpeed = _globals.Enemy2Speed;
                health = _globals.Enemy2Health;
            }

            GameObject enemyGo = Instantiate(enemyPrefab, _enemiesRoot);
            enemyGo.transform.position = tilePos;
            _enemies.Add(new Enemy
            {
                Type = type,
                MoveSpeed = moveSpeed,
                Go = enemyGo,
                Collider = enemyGo.transform.Find("Collision").GetComponent<Collider>(),
                Health = health,
                State = EnemyState.Sleep,
                WalkPath = new(),
                RootTransform = enemyGo.transform.Find("Root"),
                VisualTransform = enemyGo.transform.Find("Root").Find("Renderer"),
                ChargeSourceTransform = enemyGo.transform.Find("Root").Find("ChargeSourceTransform"),
                ChargeTargetTransform = enemyGo.transform.Find("Root").Find("ChargeTargetTransform"),
            });
        }

        private void UpdateEnemies()
        {
            foreach (Enemy enemy in _enemies)
            {
                UpdateEnemy(enemy);
            }

            List<GameObject> destroyedFireballs = new();
            foreach (GameObject fireball in _fireballs)
            {
                UpdateFireball(fireball, out bool isDestroyed);
                if (isDestroyed) destroyedFireballs.Add(fireball);
            }
            foreach (GameObject destroyedFireball in destroyedFireballs)
            {
                _fireballs.Remove(destroyedFireball);
                Destroy(destroyedFireball);
            }
        }

        private bool CanSeePlayer(Enemy enemy)
        {
            Vector3 toPlayer = _player.transform.position - enemy.Pos;
            float castDistance = toPlayer.magnitude - 0.3f;
            int hitAmount = 0;
            if (enemy.Type == EnemyType.Enemy1)
            {
                hitAmount = Physics.RaycastNonAlloc(new Ray(enemy.Pos, toPlayer.normalized), _raycastResults, castDistance);
            }
            else if (enemy.Type == EnemyType.Enemy2)
            {
                const float SphereCastRadius = 0.2f;
                hitAmount = Physics.SphereCastNonAlloc(enemy.Pos, SphereCastRadius, toPlayer.normalized, _raycastResults, castDistance);
            }

            bool hasWall = false;
            for (int i = 0; i < hitAmount; i++)
            {
                if (_raycastResults[i].transform.gameObject.CompareTag("Wall"))
                {
                    hasWall = true;
                    break;
                }
            }

            return !hasWall;
        }

        private void UpdateEnemy(Enemy enemy)
        {
            enemy.RootTransform.forward = _player.forward;

            switch (enemy.State)
            {
                case EnemyState.Sleep:
                    if (CanSeePlayer(enemy) && !IsPlayerDead) // Awakened!
                    {
                        string sfxName = enemy.Type == EnemyType.Enemy1 ? "Enemy1Awake" : "Enemy2Awake";
                        Sfx.Instance.PlayRandom(sfxName);
                        enemy.State = EnemyState.Move;
                    }

                    break;
                case EnemyState.Move:
                    Vector3 source = enemy.Pos.WithY(0.01f);
                    Vector3 target = _player.position.WithY(0.01f);
                    bool succ = NavMesh.CalculatePath(source, target, NavMesh.AllAreas, enemy.WalkPath);

                    Vector3[] corners = enemy.WalkPath.corners;
#if UNITY_EDITOR
                    DrawPath(corners);
#endif
                    Vector3 currentMoveTarget;
                    if (corners.Length >= 2) currentMoveTarget = corners[1];
                    else if (corners.Length == 1) currentMoveTarget = corners[0];
                    else currentMoveTarget = target;

                    Vector3 dir = (currentMoveTarget - enemy.Pos).normalized;
                    Vector3 deltaMove = dir * (enemy.MoveSpeed * Time.deltaTime);
                    enemy.Pos += deltaMove;

                    float range = enemy.Type == EnemyType.Enemy1 ? _globals.EnemyMeleeAttackRange : _globals.EnemyRangedAttackRange;
                    bool inRange = Vector3.Distance(enemy.Pos.ToHorizontal(), _player.position.ToHorizontal()) < range;
                    if (inRange && CanSeePlayer(enemy) && enemy.GetDamagedCoroutine == null)
                    {
                        enemy.State = EnemyState.AttackCharge;

                        // Initiate charge
                        const float ChargeDuration = 0.5f;
                        enemy.AttackStartPos = enemy.ChargeSourceTransform.localPosition;
                        enemy.AttackStartRot = enemy.ChargeSourceTransform.localRotation;

                        Vector3 targetPos = enemy.ChargeTargetTransform.localPosition;
                        Quaternion targetRot = enemy.ChargeTargetTransform.localRotation;

                        enemy.AttackChargeCoroutine = Curve.TweenDiscrete(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), ChargeDuration, _globals.TweenTickDuration,
                            t =>
                            {
                                enemy.VisualTransform.SetLocalPositionAndRotation(Vector3.Lerp(enemy.AttackStartPos, targetPos, t), Quaternion.Slerp(enemy.AttackStartRot, targetRot, t));
                            },
                            () =>
                            {
                                OnEnemyAttack(enemy);
                                enemy.AttackChargeCoroutine = null;
                            });
                    }
                    break;
                case EnemyState.AttackCharge:
                    break;
                case EnemyState.Attack:
                    break;
                case EnemyState.Dead:
                    break;
                default:
                    Debug.LogError($"Unrecognized state: {enemy.State}. Switching to move");
                    enemy.State = EnemyState.Move;
                    break;
            }
        }

        private void OnEnemyAttack(Enemy enemy)
        {
            if (IsPlayerDead) // Player was killed by something else while this enemy swings
            {
                enemy.State = EnemyState.Sleep;
                enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
                return;
            }

            enemy.State = EnemyState.Attack;

            if (enemy.Type == EnemyType.Enemy1)
            {
                if (Vector3.Distance(enemy.Pos.ToHorizontal(), _player.position.ToHorizontal()) < _globals.EnemyMeleeAttackRange) // Still in the attack range
                {
                    OnPlayerHit(out bool didPlayerDie);
                    if (didPlayerDie)
                    {
                        enemy.State = EnemyState.Sleep;
                        enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
                        return;
                    }
                }
            }
            else if (enemy.Type == EnemyType.Enemy2)
            {
                // Shoot fireball
                GameObject fireballGo = Instantiate(_globals.FireballPrefab, _fireballsRoot);

                Vector3 toPlayerDir = (_player.position.ToHorizontal() - enemy.Pos.ToHorizontal()).normalized;
                fireballGo.transform.forward = -toPlayerDir;
                fireballGo.transform.position = enemy.Pos.WithY(0.4f);

                _fireballs.Add(fireballGo);
            }

            enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
            const float PostAttackWaitDuration = 1.0f;
            enemy.PostAttackWaitCoroutine = CoroutineStarter.RunDelayed(PostAttackWaitDuration, () =>
            {
                enemy.State = EnemyState.Move;
                enemy.PostAttackWaitCoroutine = null;
            });
        }

        private void OnPlayerHit(out bool didPlayerDie)
        {
            _playerHealth--;

            if (_playerHealth > 0) // ow!
            {
                Sfx.Instance.PlayRandom("PlayerHurt");
                _ui.ShowDamage();
                _ui.SetHealth(_playerHealth, null);

                CoroutineStarter.Stop(_playerDamagedCameraFxCoroutine);

                _playerCamera.localRotation = Quaternion.Euler(0, 0, 10);
                _playerDamagedCameraFxCoroutine = CoroutineStarter.RunDelayed(0.3f, () =>
                {
                    _playerCamera.localRotation = Quaternion.Euler(0, 0, 0);
                    _playerDamagedCameraFxCoroutine = null;
                });

                didPlayerDie = false;
            }
            else // me ded
            {
                const float delay = 2f;
                _ui.ShowDead(delay);
                _player.GetComponent<FpsController>().CanControl = false;
                CoroutineStarter.Stop(_swordAttackCoroutine);

                // Sinking FX
                _playerCamera.Rotate(new(0, 0, 10), Space.Self);
                _weaponCamera.gameObject.SetActive(false);
                Vector3 startPos = _playerCamera.localPosition;
                Vector3 endPos = startPos - new Vector3(0, 0.3f, 0);
                Coroutine cameraDeadSinkCoroutine = Curve.Tween(AnimationCurve.Linear(0, 0, 1, 1), delay * 0.5f,
                    t =>
                    {
                        _playerCamera.localPosition = Vector3.Lerp(startPos, endPos, t);
                    },
                    () =>
                    {
                        cameraDeadSinkCoroutine = null;
                    });

                Sfx.Instance.Play("PlayerDie");

                CoroutineStarter.RunDelayed(delay, () =>
                {
                    LevelEnd("Game");
                });

                didPlayerDie = true;

                foreach (Enemy enemy in _enemies) // Enemies go sleep
                {
                    if (enemy.State == EnemyState.AttackCharge)
                    {
                        enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
                    }

                    enemy.State = EnemyState.Sleep;
                    CoroutineStarter.Stop(enemy.AttackChargeCoroutine);
                    CoroutineStarter.Stop(enemy.PostAttackWaitCoroutine);
                }
            }
        }

        private void UpdateFireball(GameObject fireball, out bool isDestroyed)
        {
            Transform t = fireball.transform;
            Vector3 moveDir = -t.forward;
            t.position += moveDir * (_globals.FireballSpeed * Time.deltaTime);

            t.Find("Visual").forward = _player.forward;

            isDestroyed = false;
            const float FireballHitRadius = 0.1f;
            Collider[] hits = Physics.OverlapSphere(t.position, FireballHitRadius);
            foreach (Collider hitCollider in hits)
            {
                if (hitCollider.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
                {
                    OnPlayerHit(out bool _);

                    isDestroyed = true;
                    break;
                }

                if (hitCollider.gameObject.CompareTag("Wall"))
                {
                    isDestroyed = true;
                    break;
                }
            }
        }

    }
}
