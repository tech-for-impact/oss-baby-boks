using UnityEngine;

public class SwayEffect : MonoBehaviour
{
    // ����Ⱑ ���ġ�� �ӵ� (�ð��� ������ ��)
    private float swimSpeed = -10f;
    // �¿� �̵� ���� (������� ��ü�� �¿�� �󸶸�ŭ �̵�����)
    public float lateralAmplitude = 0.1f;
    // �¿� ȸ�� ���� (����Ⱑ �¿�� �������� �ִ� ����)
    public float rotationAmplitude = 15f;
    // �ʿ信 ���� z��(����) �̵� ���� �߰� (�̼��� ������ ȿ��)
    public float forwardAmplitude = 0.03f;
    // forwardAmplitude�� ���� �ӵ� (�ɼ�)
    public float forwardSpeedMultiplier = 0.5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // ���� �� ���� ��ġ�� ȸ���� ����
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // �ð��� swimSpeed�� ���ؼ� t�� ���
        float t = Time.time * swimSpeed;
        // �¿� �̵�: sine �Լ��� x�� ������ ���
        float xOffset = Mathf.Sin(t) * lateralAmplitude;
        // �¿� ȸ��: cosine �Լ��� ȸ�� ���� ��� (sine�� 90�� ������)
        float rotationY = Mathf.Cos(t) * rotationAmplitude;
        // ������: ���� �̵�(�̼��� ������)
        float zOffset = Mathf.Sin(t * forwardSpeedMultiplier) * forwardAmplitude;

        // �ʱ� ��ġ���� x�� z�� �������� �����Ͽ� S�� ����� �̵� ����
        transform.localPosition = initialPosition + new Vector3(xOffset, 0, zOffset);
        // �ʱ� ȸ������ y�� ȸ���� �߰� (����Ⱑ �¿�� ������)
        transform.localRotation = initialRotation * Quaternion.Euler(0, rotationY, 0);
    }
}