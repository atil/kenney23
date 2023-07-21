using System;
using System.Collections;
using UnityEngine;

namespace JamKit
{
    [CreateAssetMenu(menuName = "Torreng/Curve")]
    public class Curve : ScriptableObject
    {
        private static bool _isInited = false;
        private static Curve _instance;

        public static Curve Instance
        {
            get
            {
                if (!_isInited) // This is way cheaper than null check
                {
                    _instance = Resources.Load<Curve>("Curves");
                    _isInited = true;
                }

                return _instance;
            }
        }

        public static Coroutine TweenCoroutine(AnimationCurve curve, float duration, Action<float> perTickAction)
        {
            return CoroutineStarter.Run(TweenCoroutine(curve, duration, perTickAction, () => { }));
        }

        public static void Tween(AnimationCurve curve, float duration, Action<float> perTickAction, Action postAction)
        {
            CoroutineStarter.Run(TweenCoroutine(curve, duration, perTickAction, postAction));
        }

        private static IEnumerator TweenCoroutine(AnimationCurve curve, float duration, Action<float> perTickAction, Action postAction)
        {
            for (float f = 0f; f < duration; f += Time.deltaTime)
            {
                perTickAction(curve.Evaluate(f / duration));
                yield return null;
            }

            postAction();
        }

        public static void TweenInfinite(AnimationCurve curve, float duration, Action<float> perTickAction)
        {
            CoroutineStarter.Run(TweenInfiniteCoroutine(curve, duration, perTickAction));
        }

        private static IEnumerator TweenInfiniteCoroutine(AnimationCurve curve, float duration, Action<float> perTickAction)
        {
            for (float f = 0f; ; f += Time.deltaTime)
            {
                perTickAction(curve.Evaluate(f / duration));
                if (f >= duration)
                {
                    f = 0;
                }

                yield return null;
            }
        }

        public static void TweenDiscrete(AnimationCurve curve, float duration, float tickInterval, Action<float> perTickAction, Action postAction)
        {
            CoroutineStarter.Run(TweenDiscreteCoroutine(curve, duration, tickInterval, perTickAction, postAction));
        }

        private static IEnumerator TweenDiscreteCoroutine(AnimationCurve curve, float duration, float tickInterval, Action<float> perTickAction, Action postAction)
        {
            int tickCount = (int)(duration / tickInterval);

            for (int i = 0; i < tickCount; i++)
            {
                float t = (float)i / tickCount;
                perTickAction(curve.Evaluate(t));
                yield return new WaitForSeconds(tickInterval);
            }

            postAction();
        }

    }
}