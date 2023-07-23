using JamKit;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameUi : UiBase
    {
        [SerializeField] private Image[] _hearts;

        [SerializeField] private Transform _heartsRoot;
        [SerializeField] private Transform _damageFeedbackRoot;
        [SerializeField] private Transform _deadFeedbackRoot;
        [SerializeField] private GameObject _levelEndWarningText;
        [SerializeField] private GameObject _switchWeaponHintText;

        void Start()
        {
            FadeIn();
        }

        public void SetHealth(int current, int? before)
        {
            for (int i = 0; i < Globals.PlayerHealth; i++)
            {
                if (i < current)
                {
                    _hearts[i].sprite = Globals.HearthFull;
                }
                else
                {
                    _hearts[i].sprite = Globals.HearthEmpty;
                }
            }

        }

        public void ShowDamage()
        {
            GameObject damageFeedbackGo = Instantiate(Globals.UiDamageFeedbackPrefab, _damageFeedbackRoot);

            Curve.Tween(Globals.UiDamageFeedbackCurve, 1.0f,
                t =>
                {
                    if (damageFeedbackGo == null) return;
                    float alpha = Mathf.Lerp(1, 0, t);
                    damageFeedbackGo.GetComponent<Image>().SetAlpha(alpha);
                },
                () =>
                {
                    if (damageFeedbackGo == null) return;
                    Destroy(damageFeedbackGo);
                });
        }

        public void ShowDead(float duration)
        {
            _heartsRoot.gameObject.SetActive(false);
            _deadFeedbackRoot.gameObject.SetActive(true);
        }

        public void ShowLevelEndWarning()
        {
            _levelEndWarningText.SetActive(true);

            const float LevelEndWarningDuration = 0.9f;
            CoroutineStarter.RunDelayed(LevelEndWarningDuration, () =>
            {
                if (_levelEndWarningText != null)
                {
                    _levelEndWarningText.SetActive(false);
                }
            });
        }

        public void ShowSwitchWeaponHint()
        {
            _switchWeaponHintText.SetActive(true);

            const float HintDuration = 2.0f;
            CoroutineStarter.RunDelayed(HintDuration, () =>
            {
                if (_switchWeaponHintText != null)
                {
                    _switchWeaponHintText.SetActive(false);
                }
            });
        }
    }
}