using UnityEngine;

namespace SeaBoundary
{
    public sealed class LocationExitLogger : MonoBehaviour
    {
        [SerializeField] private LocationExitController controller;
        private void OnEnable()
        {
            if (controller == null)
            {
                Debug.LogError("LocationExitLogger: assign controller.", this);
                enabled = false;
                return;
            }
            controller.OnLocationExit += LogExit;
        }
        private void OnDisable()
        {
            if (controller != null) controller.OnLocationExit -= LogExit;
        }
        private void LogExit() => Debug.Log("Location exited", this);
    }
}
