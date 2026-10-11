using UnityEngine;
using UnityEngine.UI;

namespace SeaBoundary
{
    public sealed class WeatherPresenter : MonoBehaviour
    {
        [SerializeField] private WeatherPresetResolver resolver;
        [SerializeField] private Text weatherText;
        [SerializeField] private Text detailText;
        [SerializeField] private Image accent;

        private void OnEnable()
        {
            if (resolver == null || weatherText == null || detailText == null || accent == null)
            {
                Debug.LogError("WeatherPresenter: assign resolver, labels and accent.", this);
                enabled = false;
                return;
            }
            resolver.PresetApplied += Render;
            Render(resolver.State, resolver.ActivePreset);
        }

        private void OnDisable()
        {
            if (resolver != null) resolver.PresetApplied -= Render;
        }

        private void Render(WeatherState state, WeatherPreset preset)
        {
            if (preset == null)
            {
                weatherText.text = "Погода: —";
                detailText.text = "Нет данных о погоде";
                accent.color = new Color32(166, 204, 213, 255);
                return;
            }

            switch (state)
            {
                case WeatherState.Rain:
                    weatherText.text = "Погода: Дождь (Rain)";
                    detailText.text = "Держите курс\nВетер сильнее · Видимость ниже";
                    accent.color = new Color32(100, 181, 221, 255);
                    break;
                case WeatherState.Storm:
                    weatherText.text = "Погода: Шторм (Storm)";
                    detailText.text = "Осторожнее на воде\nСильный ветер · Видимость ниже";
                    accent.color = new Color32(238, 133, 108, 255);
                    break;
                case WeatherState.Fog:
                    weatherText.text = "Погода: Туман (Fog)";
                    detailText.text = "Следите за курсом\nВетер слабее · Видимость плохая";
                    accent.color = new Color32(174, 170, 217, 255);
                    break;
                default:
                    weatherText.text = "Погода: Ясно (Clear)";
                    detailText.text = "Спокойное море\nВетер обычный · Видимость хорошая";
                    accent.color = new Color32(255, 207, 121, 255);
                    break;
            }
        }
    }
}
