using JamKit;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        [SerializeField] private Globals _globals;
        [SerializeField] private GameUi _ui;

        [SerializeField] private Texture2D _testLevel;

        [SerializeField] private Transform _player;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private Transform _groundCollider;

        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _borderPrefab;

        private Dictionary<Color, GameObject> _prefabs = new();

        private void Start()
        {
            Color playerColor = new(1, 1, 0, 1);

            _prefabs.Add(Color.white, _floorPrefab);
            _prefabs.Add(playerColor, _floorPrefab);
            _prefabs.Add(Color.black, _wallPrefab);

            int w = _testLevel.width;
            int h = _testLevel.width;

            _groundCollider.transform.position = new Vector3(w / 2.0f, 0, h / 2.0f);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Vector3 tilePos = new(i, 0, j);
                    Color color = _testLevel.GetPixel(i, j);
                    if (_prefabs.TryGetValue(color, out GameObject prefab))
                    {
                        if (color == playerColor)
                        {
                            _player.transform.position = tilePos + new Vector3(0, 0.5f, 0);
                        }
                        if (prefab == _wallPrefab && (i == 0 || i == w - 1 || j == 0 || j == h - 1))
                        {
                            prefab = _borderPrefab;
                        }
                        GameObject tileGo = Instantiate(prefab, _levelRoot);
                        tileGo.transform.position = tilePos;
                    }
                    else
                    {
                        Debug.Log($"Unrecogized color: {color}. Instantiating floor at {tilePos}");
                        GameObject tileGo = Instantiate(_floorPrefab, _levelRoot);
                        tileGo.transform.position = tilePos;
                    }
                }
            }
        }
    }
}