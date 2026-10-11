using UnityEngine;
using UnityEngine.UI;

namespace SeaBoundary
{
    public sealed class ExitWarningPresenter : MonoBehaviour
    {
        [SerializeField] private LocationExitController controller;
        [SerializeField] private Text warningText;
        [SerializeField] private Image warningBackground;
        private void OnEnable()
        {
            if (controller == null || warningText == null || warningBackground == null)
            {
                Debug.LogError("ExitWarningPresenter: assign controller, warning text and background.", this);
                enabled = false;
                return;
            }
            controller.WarningChanged += Render;
            Render(controller.WarningVisible, controller.RemainingSeconds);
        }
        private void OnDisable()
        {
            if (controller != null) controller.WarningChanged -= Render;
            if (warningText != null) warningText.enabled = false;
            if (warningBackground != null) warningBackground.enabled = false;
        }
        private void Render(bool visible, int seconds)
        {
            warningText.enabled = visible;
            warningBackground.enabled = visible;
            if (visible) warningText.text = $"Вернитесь в зону плавания  {seconds} с";
        }
    }
}
