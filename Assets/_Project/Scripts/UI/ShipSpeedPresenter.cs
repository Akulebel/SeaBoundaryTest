using UnityEngine;
using UnityEngine.UI;

namespace SeaBoundary
{
    public sealed class ShipSpeedPresenter : MonoBehaviour
    {
        [SerializeField] private Transform ship;
        [SerializeField] private Text speedText;
        private Vector3 lastPosition;
        private string lastDisplay;
        public float CurrentSpeed { get; private set; }

        private void OnEnable()
        {
            if (ship == null || speedText == null)
            {
                Debug.LogError("ShipSpeedPresenter: assign ship and speed text.", this);
                enabled = false;
                return;
            }
            lastPosition = ship.position;
            CurrentSpeed = 0f;
            Render();
        }

        private void LateUpdate()
        {
            Vector3 position = ship.position;
            Vector2 movement = new Vector2(position.x - lastPosition.x, position.z - lastPosition.z);
            CurrentSpeed = Time.deltaTime > 0f ? movement.magnitude / Time.deltaTime : 0f;
            lastPosition = position;
            Render();
        }

        private void Render()
        {
            string display = $"Скорость: {CurrentSpeed:0.0} ед/с";
            if (display == lastDisplay) return;
            speedText.text = display;
            lastDisplay = display;
        }
    }
}
