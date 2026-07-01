using UnityEngine;
using System.Collections;

public class CarAnimation : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float duration = 2f;

    private bool isMoving = false;

    // Событие, вызываемое при завершении движения (машина вернулась в начало)
    public event System.Action OnProductionComplete;

    public void PlayProduction()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("StartPoint или EndPoint не назначены!");
            return;
        }

        if (!isMoving)
            StartCoroutine(MoveCar());
    }

    private IEnumerator MoveCar()
    {
        isMoving = true;
        float elapsed = 0f;
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        yield return new WaitForSeconds(0.5f);
        transform.position = startPos;
        isMoving = false;

        // Уведомляем менеджер о завершении
        OnProductionComplete?.Invoke();
    }
}