using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public RectTransform targetUI; // 움직일 UI 요소
    public float amplitude = 20f; // 움직임의 크기
    public float speed = 1f; // 움직임의 속도
    public bool usePerlinNoise = true; // Perlin Noise를 사용할지 여부

    private Vector2 originalPosition;
    private float randomOffset; // Perlin Noise의 랜덤 시작점

    void Start()
    {
        if (targetUI == null)
        {
            targetUI = GetComponent<RectTransform>();
        }

        if (targetUI == null)
        {
            Debug.LogError("No RectTransform found. Please assign a target UI element.");
            enabled = false;
            return;
        }

        originalPosition = targetUI.anchoredPosition;
        randomOffset = Random.Range(0f, 100f); // Perlin Noise 시작점 설정
    }

    void Update()
    {
        float offsetY;

        if (usePerlinNoise)
        {
            // Perlin Noise로 부드럽고 자연스러운 움직임 구현
            offsetY = Mathf.PerlinNoise(Time.time * speed, randomOffset) * 2f - 1f;
        }
        else
        {
            // Sine Wave로 주기적인 움직임 구현
            offsetY = Mathf.Sin(Time.time * speed);
        }

        // 불규칙한 움직임의 크기 반영
        offsetY *= amplitude;

        // UI의 위치 업데이트
        targetUI.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + offsetY);
    }
}