using System;
using UnityEngine;

namespace JamKit
{
    [Serializable]
    public class SceneTransitionParams
    {
        [SerializeField] private Color _Color;
        public Color Color => _Color;

        [SerializeField] private float _Duration;
        public float Duration => _Duration;

        [SerializeField] private AnimationCurve _Curve;
        public AnimationCurve Curve => _Curve;

        [SerializeField] private bool _IsDiscrete;
        public bool IsDiscrete => _IsDiscrete;
    }

    [CreateAssetMenu(menuName = "Torreng/Globals")]
    public class Globals : ScriptableObject
    {
        [Header("Misc")]
        [SerializeField] private float _discreteTickInteval;
        public float DiscreteTickInterval => _discreteTickInteval;

        [Header("Scene Transition")]
        [SerializeField] private SceneTransitionParams _sceneTransitionParams;
        public SceneTransitionParams SceneTransitionParams => _sceneTransitionParams;

        [Header("Auxillary Scenes")]
        [SerializeField] private Color _splashSceneCameraBackgroundColor;
        public Color SplashSceneCameraBackgroundColor => _splashSceneCameraBackgroundColor;

        [SerializeField] private Color _endSceneCameraBackgroundColor;
        public Color EndSceneCameraBackgroundColor => _endSceneCameraBackgroundColor;

        [Header("Build")]
        [SerializeField] private Color _buildSplashBackgroundColor;
        public Color BuildSplashBackgroundColor => _buildSplashBackgroundColor;

        [Header("KENNEY!")]
        public Texture2D[] Levels;
        public GameObject FloorPrefab;
        public GameObject WallPrefab;
        public GameObject BorderPrefab;
        public GameObject ExitPrefab;

        public Color FloorColor = Color.white;
        public Color WallColor = Color.black;
        public Color PlayerColor = new(1, 1, 0, 1);
        public Color ExitColor = new(1, 0, 1, 1);
    }
}
