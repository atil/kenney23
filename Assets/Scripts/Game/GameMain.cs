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
        [SerializeField] private FpsController _playerController;
        [SerializeField] private Footsteps _playerFootsteps;
        [SerializeField] private Transform _weaponCamera;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private Transform _groundCollider;
        [SerializeField] private NavMeshSurface _navmeshSurface;

        private Dictionary<Color, GameObject> _prefabs = new();

        private List<GameObject> _props = new();

        private int _playerHealth;
        private bool IsPlayerDead => _playerHealth <= 0;

        private void Start()
        {
            _prefabs.Add(_globals.FloorColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.PlayerColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.WallColor, _globals.WallPrefab);
            _prefabs.Add(_globals.ExitColor, _globals.ExitPrefab);
            _prefabs.Add(_globals.Enemy1Color, _globals.FloorPrefab);
            _prefabs.Add(_globals.Enemy2Color, _globals.FloorPrefab);
            _prefabs.Add(_globals.PropColor, _globals.FloorPrefab);

#if UNITY_EDITOR
            if (_forceLevel == -1)
            {
                _currentLevel = PlayerPrefs.GetInt("kenney.currentLevel", 0);
            }
            else
            {
                _currentLevel = _forceLevel;
            }
#else
            _currentLevel = PlayerPrefs.GetInt("kenney.currentLevel", 0);
#endif


            _playerController.Sensitivity = PlayerPrefs.GetFloat("kenney.sensitivity", 150); // 150 is the default value on script
            _playerHealth = _globals.PlayerHealth;
            StartPlayerAttack();

            _ui.SetHealth(_playerHealth, null);

            LoadLevelFrom(_globals.Levels[_currentLevel]);

            _navmeshSurface.BuildNavMesh();

            Sfx.Instance.Play("LevelStart");

        }

        private void LoadLevelFrom(Texture2D levelTexture)
        {
            if (levelTexture.name == "LevelCrossbowIntro")
            {
                _ui.ShowSwitchWeaponHint();
            }

            int w = levelTexture.width;
            int h = levelTexture.height;

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
                            AddEnemy(tilePos, EnemyType.Enemy1);
                        }
                        if (color == _globals.Enemy2Color)
                        {
                            AddEnemy(tilePos, EnemyType.Enemy2);
                        }
                        if (color == _globals.PropColor)
                        {
                            AddProp(tilePos);
                        }

                        if (prefab == _globals.WallPrefab)
                        {
                            bool isBorder = (i == 0 || i == w - 1 || j == 0 || j == h - 1);
                            if (isBorder)
                            {
                                prefab = _globals.BorderPrefab;
                            }
                            else
                            {
                                float rand = UnityEngine.Random.value;
                                if (rand < 0.15f)
                                {
                                    prefab = _globals.Wall2Prefab;
                                }
                                else if (rand < 0.3f)
                                {
                                    prefab = _globals.Wall3Prefab;
                                }
                            }
                        }

                        if (prefab == _globals.FloorPrefab && UnityEngine.Random.value < 0.15f)
                        {
                            prefab = _globals.Floor2Prefab;
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

        private void AddProp(Vector3 tilePos)
        {
            GameObject propPrefab = UnityEngine.Random.value > 0.5f ? _globals.Prop1Prefab : _globals.Prop2Prefab;
            GameObject propGo = Instantiate(propPrefab, _levelRoot);
            const float PropVisualHorizontalOffset = 0.15f;
            const float PropVisualHeight = 0.08f;
            propGo.transform.position = tilePos.WithY(PropVisualHeight) + new Vector3(UnityEngine.Random.value * PropVisualHorizontalOffset, 0, UnityEngine.Random.value * PropVisualHorizontalOffset);
            _props.Add(propGo);
        }

        private void Update()
        {
            UpdatePlayerAttack();
            UpdateEnemies();
            UpdateProps();
        }

        private void UpdateProps()
        {
            foreach (GameObject propGo in _props)
            {
                propGo.transform.forward = _player.forward;
            }
        }

        public void OnExitTriggered()
        {
            if (_enemies.Exists(x => x.IsAlive))
            {
                Sfx.Instance.Play("LevelEndWarning");
                _ui.ShowLevelEndWarning();
                return;
            }
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
                PlayerPrefs.SetFloat("kenney.sensitivity", _playerController.Sensitivity);
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