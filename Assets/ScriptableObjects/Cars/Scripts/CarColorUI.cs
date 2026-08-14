using UnityEngine;
using UnityEngine.UI;

public class CarColorUI : MonoBehaviour
{
    [Header("Target Car Data")]
    [SerializeField] private CarBlueprint currentCarBlueprint; // редактируемый blueprint

    [Header("UI Elements")]
    [SerializeField] private Button[] colorButtons;    // 5 кнопок для цветов
    [SerializeField] private Slider tintSlider;

    [Header("Scene Car Reference")]
    [SerializeField] private GameObject playerCarObject; // можно назначить вручную или искать по тегу

    private CarCustomizer carCustomizer;

    private void Start()
    {
        // Настройка кнопок цветов
        for (int i = 0; i < colorButtons.Length && i < CarCustomizer.PresetColors.Length; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => OnColorButtonClicked(index));

            // Окрашиваем кнопку в соответствующий цвет
            ColorBlock cb = colorButtons[i].colors;
            cb.normalColor = CarCustomizer.PresetColors[i];
            cb.highlightedColor = CarCustomizer.PresetColors[i] * 1.2f;
            cb.pressedColor = CarCustomizer.PresetColors[i] * 0.8f;
            colorButtons[i].colors = cb;
        }

        // Слайдер тонировки
        if (tintSlider != null)
        {
            tintSlider.onValueChanged.AddListener(OnTintSliderChanged);
            tintSlider.value = currentCarBlueprint?.tintLevel ?? 0f;
        }

        // Находим машину, если не назначена
        if (playerCarObject == null)
            playerCarObject = GameObject.FindGameObjectWithTag("PlayerCar");

        // Применяем начальные цвета
        ApplyToCar();
    }

    public void OnColorButtonClicked(int colorIndex)
    {
        if (currentCarBlueprint != null)
        {
            currentCarBlueprint.bodyColorIndex = colorIndex;
            ApplyToCar();
        }
    }

    public void OnTintSliderChanged(float value)
    {
        if (currentCarBlueprint != null)
        {
            currentCarBlueprint.tintLevel = value;
            ApplyToCar();
        }
    }

    private void ApplyToCar()
    {
        if (playerCarObject == null)
            playerCarObject = GameObject.FindGameObjectWithTag("PlayerCar");

        if (playerCarObject == null)
        {
            Debug.LogWarning("Player car not found in scene.");
            return;
        }

        if (carCustomizer == null)
            carCustomizer = playerCarObject.GetComponent<CarCustomizer>();

        if (carCustomizer != null && currentCarBlueprint != null)
            carCustomizer.ApplyFromBlueprint(currentCarBlueprint);
        else
            Debug.LogWarning("CarCustomizer not found on player car.");
    }

    /// <summary>Вызвать при смене машины (спавне новой)</summary>
    public void SetTargetCar(CarBlueprint blueprint, GameObject carObject)
    {
        currentCarBlueprint = blueprint;
        playerCarObject = carObject;
        carCustomizer = carObject?.GetComponent<CarCustomizer>();
        ApplyToCar();
    }
}