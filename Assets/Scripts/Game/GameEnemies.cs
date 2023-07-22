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

        private class Enemy
        {
            public GameObject Go;
            public EnemyState State;
            public NavMeshPath WalkPath;
        }

        [Header("Enemies")]
        [SerializeField] private Transform _enemiesRoot;
        [SerializeField] private float _enemyMoveSpeed = 1.0f;

        private List<Enemy> _enemies = new();

        private RaycastHit[] _raycastResults = new RaycastHit[20];

        private void AddEnemy(Color enemyType, Vector3 tilePos)
        {
            GameObject enemyGo = Instantiate(_globals.Enemy1Prefab, _enemiesRoot);
            enemyGo.transform.position = tilePos;
            _enemies.Add(new Enemy
            {
                Go = enemyGo,
                State = EnemyState.Sleep,
                WalkPath = new(),
            });
        }

        private void UpdateEnemies()
        {
            foreach (Enemy enemy in _enemies)
            {
                Transform enemyVisual = enemy.Go.transform.Find("Visual");
                enemyVisual.forward = _player.forward;

                switch (enemy.State)
                {
                    case EnemyState.Sleep:

                        Vector3 toPlayer = _player.transform.position - enemy.Go.transform.position;
                        Ray ray = new(enemy.Go.transform.position, toPlayer.normalized);
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

                        if (!hasWall)
                        {
                            // Awakened!
                            enemy.State = EnemyState.Move;
                        }

                        break;
                    case EnemyState.Move:
                        Vector3 source = enemy.Go.transform.position.WithY(0.01f);
                        Vector3 target = _player.position.WithY(0.01f);
                        bool succ = NavMesh.CalculatePath(source, target, NavMesh.AllAreas, enemy.WalkPath);
                        Debug.Assert(succ, $"There should always be path src: {source} target: {target}");

                        Vector3[] corners = enemy.WalkPath.corners;
                        DrawPath(corners);
                        Debug.Assert(corners.Length >= 2, $"It should be at least a straight line src: {source} target: {target}");

                        Vector3 dir = (corners[1] - enemy.Go.transform.position).normalized;
                        Vector3 deltaMove = dir * (_enemyMoveSpeed * Time.deltaTime);
                        enemy.Go.transform.position += deltaMove;

                        break;
                    case EnemyState.AttackCharge:
                        break;
                    case EnemyState.Attack:
                        break;
                    default:
                        break;
                }
            }
        }

    }
}