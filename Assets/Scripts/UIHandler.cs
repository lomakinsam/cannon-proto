using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [Header("Shot Power Panel")]
    [SerializeField]
    private Cannon cannon;
    [SerializeField]
    private Slider powerSlider;
    [SerializeField]
    private TextMeshProUGUI powerValueText;

    private void Awake()
    {
        cannon.OnShotPowerChange += UpdateShotPowerPanel; 
        UpdateShotPowerPanel(cannon.ShotPower);
    }

    private void UpdateShotPowerPanel(float shotPower)
    {
        powerSlider.value = Remap(shotPower, 0f, Cannon.maxShotPower, 0f, 1f);
        powerValueText.text = ((int)Remap(shotPower, 0f, Cannon.maxShotPower, 0f, 100f)).ToString();
    }

    private float Remap(float value, float inMin, float inMax, float outMin, float outMax) => outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
}
