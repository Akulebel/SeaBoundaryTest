using System;
using UnityEngine;

namespace SeaBoundary
{
    public sealed class LocationExitController : MonoBehaviour
    {
        [SerializeField] private Transform ship;
        [SerializeField] private LocationBoundary boundary;
        private readonly ExitCountdown countdown = new ExitCountdown();
        private bool lastVisible;
        private int lastSeconds;
        public event Action OnLocationExit;
        public event Action<bool, int> WarningChanged;
        public bool WarningVisible => countdown.WarningVisible;
        public int RemainingSeconds => countdown.RemainingSeconds;
        public ExitPhase Phase => countdown.Phase;

        private void OnEnable()
        {
            countdown.Reset();
            if (ship == null || boundary == null)
            {
                Debug.LogError("LocationExitController: assign ship and boundary.", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            countdown.Reset();
            lastVisible = false;
            lastSeconds = 0;
            WarningChanged?.Invoke(false, 0);
        }

        private void LateUpdate()
        {
            bool fired = countdown.Tick(!boundary.Contains(ship.position), Time.deltaTime);
            if (lastVisible != WarningVisible || lastSeconds != RemainingSeconds)
            {
                lastVisible = WarningVisible;
                lastSeconds = RemainingSeconds;
                WarningChanged?.Invoke(lastVisible, lastSeconds);
            }
            if (fired) OnLocationExit?.Invoke();
        }
    }
}
