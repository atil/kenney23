using System;
using UnityEngine;
using UnityEngine.UI;

namespace JamKit
{
    public abstract class UiBase : MonoBehaviour
    {
        private enum FadeType
        {
            FadeIn, FadeOut
        }

        [SerializeField] private Globals _globals;
        protected Globals Globals => _globals;

        [SerializeField] private Camera _camera;
        protected Camera Camera => _camera;

        [SerializeField] private Image _coverImage = default;
        protected Image CoverImage => _coverImage;

        public virtual SceneTransitionParams SceneTransitionParams => _globals.SceneTransitionParams;

        protected void FadeIn(SceneTransitionParams sceneTransitionParams = null, Action postAction = null)
        {
            Fade(FadeType.FadeIn, sceneTransitionParams, postAction);
        }

        protected void FadeOut(SceneTransitionParams sceneTransitionParams = null, Action postAction = null)
        {
            Fade(FadeType.FadeOut, sceneTransitionParams, postAction);
        }

        private void Fade(FadeType type, SceneTransitionParams sceneTransitionParams, Action postAction)
        {
            if (sceneTransitionParams == null)
            {
                sceneTransitionParams = _globals.SceneTransitionParams;
            }

            Color srcColor = type == FadeType.FadeIn ? sceneTransitionParams.Color : Color.clear;
            Color targetColor = type == FadeType.FadeIn ? Color.clear : sceneTransitionParams.Color;

            if (sceneTransitionParams.IsDiscrete)
            {
                Curve.TweenDiscrete(sceneTransitionParams.Curve,
                    sceneTransitionParams.Duration,
                    _globals.DiscreteTickInterval,
                    t => { _coverImage.color = Color.Lerp(srcColor, targetColor, t); },
                    () =>
                    {
                        _coverImage.color = targetColor;
                        postAction?.Invoke();
                    });
            }
            else
            {
                Curve.Tween(sceneTransitionParams.Curve,
                sceneTransitionParams.Duration,
                t => { _coverImage.color = Color.Lerp(srcColor, targetColor, t); },
                () =>
                {
                    _coverImage.color = targetColor;
                    postAction?.Invoke();
                });
            }
        }

    }
}