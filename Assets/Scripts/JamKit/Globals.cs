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
        public GameObject Floor2Prefab;
        public GameObject WallPrefab;
        public GameObject Wall2Prefab;
        public GameObject Wall3Prefab;
        public GameObject BorderPrefab;
        public GameObject ExitPrefab;
        public GameObject Enemy1Prefab;
        public Texture2D Enemy1Die0;
        public Texture2D Enemy1Die1;
        public GameObject Enemy2Prefab;
        public GameObject ArrowPrefab;
        public GameObject FireballPrefab;

        [Space]
        public Color FloorColor = Color.white;
        public Color WallColor = Color.black;
        public Color PlayerColor = new(1, 1, 0, 1);
        public Color ExitColor = new(1, 0, 1, 1);
        public Color Enemy1Color = new(1, 0, 0, 1);
        public Color Enemy2Color = new(1, (128.0f / 255.0f), 0, 1);

        [Space]
        public float TweenTickDuration = 0.075f;
        public int PlayerHealth = 3;
        public float Enemy1Speed = 2.0f;
        public int Enemy1Health = 6;
        public float Enemy2Speed = 1.3f;
        public int Enemy2Health = 4;
        public float EnemyMeleeAttackRange = 1.0f;
        public float EnemyRangedAttackRange = 20.0f;
        public AnimationCurve EnemyGetDamagedCurve;
        public float ArrowSpeed = 5.0f;
        public int SwordDamage = 2;
        public int ArrowDamage = 1;
        public float FireballSpeed = 4.0f;

        [Space]
        public Sprite HearthFull;
        public Sprite HearthEmpty;
        public GameObject UiDamageFeedbackPrefab;
        public AnimationCurve UiDamageFeedbackCurve;
    }
}
