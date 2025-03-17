using UnityEngine;

[System.Serializable]
public class BouncyImage
{
    public RectTransform image;       // UI 이미지의 RectTransform
    [Tooltip("바운스의 초기 위상 오프셋")]
    public float phaseOffset = 0f;      // 시작 위상 오프셋
}

public class BouncyUI : MonoBehaviour
{
    // 모든 이미지에 공통으로 적용할 amplitude와 frequency
    public float amplitude = 40f; // 바운스 높이
    public float frequency = 0.5f;  // 바운스 속도 (주기)

    // Inspector에서 설정할 이미지 목록
    public BouncyImage[] bouncyImages;

    // 각 이미지의 초기 위치를 저장할 배열
    private Vector3[] initialPositions;

    void Start()
    {
        initialPositions = new Vector3[bouncyImages.Length];
        for (int i = 0; i < bouncyImages.Length; i++)
        {
            if (bouncyImages[i].image != null)
                initialPositions[i] = bouncyImages[i].image.localPosition;
        }
    }

    void Update()
    {
        for (int i = 0; i < bouncyImages.Length; i++)
        {
            if (bouncyImages[i].image != null)
            {
                // 동일한 amplitude, frequency를 사용하지만, 각 이미지마다 phaseOffset이 적용됩니다.
                float newY = initialPositions[i].y +
                    Mathf.Sin(Time.time * frequency * 2 * Mathf.PI + bouncyImages[i].phaseOffset) * amplitude;
                bouncyImages[i].image.localPosition = new Vector3(initialPositions[i].x, newY, initialPositions[i].z);
            }
        }
    }
}