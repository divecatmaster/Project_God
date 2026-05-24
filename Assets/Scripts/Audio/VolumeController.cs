using UnityEngine;
using UnityEngine.UI;

namespace God.Audio
{
    /// <summary>
    /// Connects a UI Slider to the SoundManager for volume control.
    /// </summary>
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private SoundCategory category;
        [SerializeField] private Slider slider;

        private void Start()
        {
            if (slider == null) slider = GetComponent<Slider>();
            if (slider == null) return;

            // Initialize slider value from SoundManager
            slider.value = SoundManager.Instance.GetVolume(category);
            
            // Listen for changes
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            SoundManager.Instance.SetVolume(category, value);
        }

        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
