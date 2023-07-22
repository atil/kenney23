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
    }
}