using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimations : MonoBehaviour
{
    private Button button;
    private RectTransform rect;
    private Image image;
    private Color originalColor;

    void Start()
    {
        button = GetComponent<Button>();
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (image) originalColor = image.color;
    }

    // Эти методы вызываются через Event Trigger (или можно через интерфейсы)
    public void OnPointerEnter()
    {
        rect.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        if (image) image.DOColor(new Color(1f, 0.5f, 0f, 1f), 0.2f);
    }

    public void OnPointerExit()
    {
        rect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        if (image) image.DOColor(originalColor, 0.2f);
    }

    public void OnPointerDown()
    {
        rect.DOScale(0.95f, 0.08f).SetEase(Ease.InOutQuad);
    }

    public void OnPointerUp()
    {
        rect.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
    }
}