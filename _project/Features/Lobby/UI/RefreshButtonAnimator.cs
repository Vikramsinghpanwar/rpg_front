using UnityEngine;
using UnityEngine.UI;

namespace Features.Lobby.UI
{
    public class RefreshButtonAnimator : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] public RectTransform targetRect;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = 1f;
        [SerializeField] private float spinAngle = 360f;
        [SerializeField] private float minVisibleDuration = 0.75f;

        private bool isSpinning;
        private int activeTweenId = -1;
        private int pendingStopId = -1;
        private float spinStartTime;

        public bool IsSpinning => isSpinning;

        public void StartSpin()
        {
            if (isSpinning || targetRect == null) return;

            if (pendingStopId != -1)
            {
                LeanTween.cancel(pendingStopId);
                pendingStopId = -1;
            }

            isSpinning = true;
            spinStartTime = Time.time;
            activeTweenId = LeanTween.rotateZ(targetRect.gameObject, spinAngle, spinDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setLoopClamp()
                .id;
        }

        public void StopSpin()
        {
            if (!isSpinning || targetRect == null) return;

            float elapsed = Time.time - spinStartTime;
            if (elapsed < minVisibleDuration)
            {
                pendingStopId = LeanTween.delayedCall(minVisibleDuration - elapsed, () =>
                {
                    pendingStopId = -1;
                    if (isSpinning) InternalStopSpin();
                }).id;
            }
            else
            {
                InternalStopSpin();
            }
        }

        public void ForceStop()
        {
            if (pendingStopId != -1)
            {
                LeanTween.cancel(pendingStopId);
                pendingStopId = -1;
            }

            if (isSpinning && targetRect != null)
            {
                LeanTween.cancel(activeTweenId);
                targetRect.localEulerAngles = Vector3.zero;
                isSpinning = false;
            }
        }

        private void InternalStopSpin()
        {
            if (!isSpinning || targetRect == null) return;

            LeanTween.cancel(activeTweenId);
            targetRect.localEulerAngles = Vector3.zero;
            isSpinning = false;
        }

        void OnDisable()
        {
            ForceStop();
        }

        void OnDestroy()
        {
            ForceStop();
        }
    }
}
