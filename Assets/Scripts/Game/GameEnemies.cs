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
        }

        private enum EnemyType
        {
            Enemy1,
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

            public Coroutine AttackCoroutine;
            public Coroutine GetDamagedCoroutine;
            public Vector3 AttackStartPos;
            public Quaternion AttackStartRot;

            public Vector3 Pos
            {
                get => Go.transform.position;
                set => Go.transform.position = value;
            }
        }

        [Header("Enemies")]
        [SerializeField] private Transform _enemiesRoot;

        private List<Enemy> _enemies = new();

        private const int MaxRayCastResults = 20;
        private RaycastHit[] _raycastResults = new RaycastHit[MaxRayCastResults];

        private void AddEnemy(Color enemyTypeColor, Vector3 tilePos)
        {
            EnemyType type = EnemyType.Enemy1;
            GameObject enemyPrefab = _globals.Enemy1Prefab;
            float moveSpeed = _globals.Enemy1Speed;
            if (enemyTypeColor == _globals.Enemy1Color)
            {
                type = EnemyType.Enemy1;
                enemyPrefab = _globals.Enemy1Prefab;
                moveSpeed = _globals.Enemy1Speed;
            }

            GameObject enemyGo = Instantiate(enemyPrefab, _enemiesRoot);
            enemyGo.transform.position = tilePos;
            _enemies.Add(new Enemy
            {
                Type = type,
                MoveSpeed = moveSpeed,
                Go = enemyGo,
                Collider = enemyGo.transform.Find("Collision").GetComponent<Collider>(),
                Health = _globals.Enemy1Health,
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
        }

        private void UpdateEnemy(Enemy enemy)
        {
            enemy.RootTransform.forward = _player.forward;

            switch (enemy.State)
            {
                case EnemyState.Sleep:

                    Vector3 toPlayer = _player.transform.position - enemy.Pos;
                    Ray ray = new(enemy.Pos, toPlayer.normalized);
                    int hitAmount = Physics.RaycastNonAlloc(ray, _raycastResults, toPlayer.magnitude);
                    bool hasWall = false;
                    for (int i = 0; i < hitAmount; i++)
                    {
                        if (_raycastResults[i].transform.gameObject.CompareTag("Wall"))
                        {
                            hasWall = true;
                            break;
                        }
                    }

                    if (!hasWall && !IsPlayerDead)
                    {
                        // Awakened!
                        Sfx.Instance.PlayRandom("Enemy1Awake");
                        enemy.State = EnemyState.Move;
                    }

                    break;
                case EnemyState.Move:
                    Vector3 source = enemy.Pos.WithY(0.01f);
                    Vector3 target = _player.position.WithY(0.01f);
                    bool succ = NavMesh.CalculatePath(source, target, NavMesh.AllAreas, enemy.WalkPath);
                    Debug.Assert(succ, $"There should always be path src: {source} target: {target}");

                    Vector3[] corners = enemy.WalkPath.corners;
                    DrawPath(corners);
                    Debug.Assert(corners.Length >= 2, $"It should be at least a straight line src: {source} target: {target}");

                    Vector3 dir = (corners[1] - enemy.Pos).normalized;
                    Vector3 deltaMove = dir * (enemy.MoveSpeed * Time.deltaTime);
                    enemy.Pos += deltaMove;

                    bool inRange = Vector3.Distance(enemy.Pos.ToHorizontal(), _player.position.ToHorizontal()) < _globals.EnemyAttackRange;
                    if (inRange && enemy.GetDamagedCoroutine == null)
                    {
                        enemy.State = EnemyState.AttackCharge;

                        // Initiate charge
                        const float ChargeDuration = 0.5f;
                        enemy.AttackStartPos = enemy.ChargeSourceTransform.localPosition;
                        enemy.AttackStartRot = enemy.ChargeSourceTransform.localRotation;

                        Vector3 targetPos = enemy.ChargeTargetTransform.localPosition;
                        Quaternion targetRot = enemy.ChargeTargetTransform.localRotation;

                        enemy.AttackCoroutine = Curve.TweenDiscrete(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), ChargeDuration, _globals.TweenTickDuration,
                            t =>
                            {
                                enemy.VisualTransform.SetLocalPositionAndRotation(Vector3.Lerp(enemy.AttackStartPos, targetPos, t), Quaternion.Slerp(enemy.AttackStartRot, targetRot, t));
                            },
                            () =>
                            {
                                OnEnemyAttack(enemy);
                                enemy.AttackCoroutine = null;
                            });
                    }
                    break;
                case EnemyState.AttackCharge:
                    break;
                case EnemyState.Attack:
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

            if (Vector3.Distance(enemy.Pos.ToHorizontal(), _player.position.ToHorizontal()) < _globals.EnemyAttackRange) // Still in the attack range
            {
                _playerHealth--;

                if (_playerHealth > 0) // ow!
                {
                    Sfx.Instance.PlayRandom("PlayerHurt");
                    _ui.ShowDamage();
                    _ui.SetHealth(_playerHealth, null);

                    _playerCamera.Rotate(new(0, 0, 10), Space.Self);
                    CoroutineStarter.RunDelayed(0.3f, () =>
                    {
                        _playerCamera.Rotate(new(0, 0, -10), Space.Self);
                    });
                }
                else // me ded
                {
                    enemy.State = EnemyState.Sleep;
                    enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);

                    const float delay = 2f;
                    _ui.ShowDead(delay);
                    _player.GetComponent<FpsController>().CanControl = false;
                    if (_attackCoroutine != null)
                    {
                        CoroutineStarter.Stop(_attackCoroutine);
                    }

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

                    return;
                }
            }

            enemy.VisualTransform.SetLocalPositionAndRotation(enemy.AttackStartPos, enemy.AttackStartRot);
            CoroutineStarter.RunDelayed(1.0f, () =>
            {
                enemy.State = EnemyState.Move;
            });
        }
    }
}
