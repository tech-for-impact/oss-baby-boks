using UnityEngine;

[System.Serializable]
public class BouncyImage
{
    public RectTransform image;       // UI �̹����� RectTransform
    [Tooltip("�ٿ�� �ʱ� ���� ������")]
    public float phaseOffset = 0f;      // ���� ���� ������
}

public class BouncyUI : MonoBehaviour
{
    // ��� �̹����� �������� ������ amplitude�� frequency
    public float amplitude = 40f; // �ٿ ����
    public float frequency = 0.5f;  // �ٿ �ӵ� (�ֱ�)

    // Inspector���� ������ �̹��� ���
    public BouncyImage[] bouncyImages;

    // �� �̹����� �ʱ� ��ġ�� ������ �迭
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
                // ������ amplitude, frequency�� ���������, �� �̹������� phaseOffset�� ����˴ϴ�.
                float newY = initialPositions[i].y +
                    Mathf.Sin(Time.time * frequency * 2 * Mathf.PI + bouncyImages[i].phaseOffset) * amplitude;
                bouncyImages[i].image.localPosition = new Vector3(initialPositions[i].x, newY, initialPositions[i].z);
            }
        }
    }
}