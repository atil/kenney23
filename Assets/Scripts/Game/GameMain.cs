using JamKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        [SerializeField] private Globals _globals;
        [SerializeField] private GameUi _ui;

        [SerializeField] private int _forceLevel = -1;

        private int _currentLevel = 0;

        [SerializeField] private Transform _player;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private Transform _groundCollider;
        [SerializeField] private NavMeshSurface _navmeshSurface;

        private Dictionary<Color, GameObject> _prefabs = new();

        private void Start()
        {
            _prefabs.Add(_globals.FloorColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.PlayerColor, _globals.FloorPrefab);
            _prefabs.Add(_globals.WallColor, _globals.WallPrefab);
            _prefabs.Add(_globals.ExitColor, _globals.ExitPrefab);

            if (_forceLevel == -1)
            {
                _currentLevel = PlayerPrefs.GetInt("kenney.currentLevel", 0);
            }
            else
            {
                _currentLevel = _forceLevel;
            }

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
                        Debug.Log($"Unrecogized color: {color}. Instantiating floor at {tilePos}");
                        GameObject tileGo = Instantiate(_globals.FloorPrefab, _levelRoot);
                        tileGo.transform.position = tilePos;
                    }
                }
            }
        }

        public void OnExitTriggered()
        {
            _currentLevel++;
            PlayerPrefs.SetInt("kenney.currentLevel", _currentLevel);
            _ui.FadeOut(_globals.SceneTransitionParams);
            _player.GetComponent<FpsController>().CanControl = false;
            CoroutineStarter.RunDelayed(_globals.SceneTransitionParams.Duration + 0.01f, () =>
            {
                if (_currentLevel == _globals.Levels.Length)
                {
                    SceneManager.LoadScene("End");
                }
                else
                {
                    SceneManager.LoadScene("Game");
                }
            });
        }
    }
}