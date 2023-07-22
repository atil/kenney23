using JamKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

namespace Game
{
    public partial class GameMain : MonoBehaviour
    {
        [Header("Game")]
        [SerializeField] private Globals _globals;
        [SerializeField] private GameUi _ui;

        [SerializeField] private int _forceLevel = -1;

        private int _currentLevel = 0;

        [SerializeField] private Transform _player;
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private Transform _weaponCamera;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private Transform _groundCollider;
        [SerializeField] private NavMeshSurface _navmeshSurface;

        private Dictionary<Color, GameObject> _prefabs = new();

        private int _playerHealth;
        private bool IsPlayerDead => _playerHealth <= 0;

        private void Start()
        {
            Sfx.Instance.Play("LevelStart");

            _prefabs.Add(_globals.FloorColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.PlayerColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.WallColor, _globals.WallPrefab);
            _prefabs.Add(_globals.ExitColor, _globals.ExitPrefab);
            _prefabs.Add(_globals.Enemy1Color, _globals.FloorPrefab);

            if (_forceLevel == -1)
            {
                _currentLevel = PlayerPrefs.GetInt("kenney.currentLevel", 0);
            }
            else
            {
                _currentLevel = _forceLevel;
            }

            _playerHealth = _globals.PlayerHealth;

            _ui.SetHealth(_playerHealth, null);

            LoadLevelFrom(_globals.Levels[_currentLevel]);

            _navmeshSurface.BuildNavMesh();

        }

        private void LoadLevelFrom(Texture2D levelTexture)
        {
            int w = levelTexture.width;
            int h = levelTexture.width;

            _groundCollider.transform.position = new Vector3(w / 2.0f, 0, h / 2.0f);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Vector3 tilePos = new(i, 0, j);
                    Color color = levelTexture.GetPixel(i, j);
                    if (_prefabs.TryGetValue(color, out GameObject prefab))
                    {
                        if (color == _globals.PlayerColor)
                        {
                            _player.transform.position = tilePos + new Vector3(0, 0.5f, 0);
                        }

                        if (color == _globals.Enemy1Color)
                        {
                            AddEnemy(color, tilePos);
                        }

                        bool isBorder = (i == 0 || i == w - 1 || j == 0 || j == h - 1);
                        if (prefab == _globals.WallPrefab && isBorder)
                        {
                            prefab = _globals.BorderPrefab;
                        }

                        GameObject tileGo = Instantiate(prefab, _levelRoot);
                        tileGo.transform.position = tilePos;
                    }
                    else
                    {
                        Debug.LogError($"Unrecogized color: {color}. Instantiating floor at {tilePos}");
                        GameObject tileGo = Instantiate(_globals.FloorPrefab, _levelRoot);
                        tileGo.transform.position = tilePos;
                    }
                }
            }
        }

        private void Update()
        {
            UpdatePlayerAttack();
            UpdateEnemies();
        }

        public void OnExitTriggered()
        {
            _currentLevel++;
            PlayerPrefs.SetInt("kenney.currentLevel", _currentLevel);

            Sfx.Instance.Play("LevelPass");
            string nextSceneName = _currentLevel == _globals.Levels.Length ? "End" : "Game";
            LevelEnd(nextSceneName);
        }

        private void LevelEnd(string nextSceneName)
        {
            _ui.FadeOut(_globals.SceneTransitionParams);
            _player.GetComponent<FpsController>().CanControl = false;
            CoroutineStarter.RunDelayed(_globals.SceneTransitionParams.Duration + 0.01f, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }

        private void DrawPath(Vector3[] path)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 p1 = path[i];
                Vector3 p2 = path[i + 1];
                Debug.DrawLine(p1, p2);
            }
        }
    }
}