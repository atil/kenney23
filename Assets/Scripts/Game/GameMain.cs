using JamKit;
using UnityEngine;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        [SerializeField] private Globals _globals;
        [SerializeField] private GameUi _ui;

        [SerializeField] private Texture2D _testLevel;

        [SerializeField] private Transform _levelRoot;
        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private GameObject _wallPrefab;

        private void Start()
        {
            int w = _testLevel.width;
            int h = _testLevel.width;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color color = _testLevel.GetPixel(i, j);
                    GameObject prefab = null;
                    if (color == Color.white)
                    {
                        prefab = _floorPrefab;
                    }
                    if (color == Color.black)
                    {
                        prefab = _wallPrefab;
                    }

                    GameObject tileGo = Instantiate(prefab, _levelRoot);
                    tileGo.transform.position = new(i, 0, j);
                }
            }
        }
    }
}