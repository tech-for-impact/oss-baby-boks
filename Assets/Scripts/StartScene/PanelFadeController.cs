using UnityEngine;
using UnityEngine.UI;

public class PanelFadeController : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup; // UI �гο� ������ CanvasGroup
    public float displayDuration = 3f;   // ǥ�õ� �ð�(��)
    public float fadeDuration = 0.5f;    // ���̵� �ƿ��� �ɸ��� �ð�(��)

    private float timer = 0f;
    private bool isFadingOut = false;

    void Start()
    {
        // ���� ���۵Ǹ� �ٷ� ǥ�õǵ��� alpha�� 1�� ����
        panelCanvasGroup.alpha = 1f;
        timer = displayDuration;
    }

    void Update()
    {
        // �г��� ǥ�õ� �ð��� ���� �ִٸ� �ð� ����
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            // �ð��� ��� ���� �� ���̵� �ƿ� ����
            if (timer <= 0f)
            {
                isFadingOut = true;
            }
        }
        else if (isFadingOut)
        {
            // ���̵� �ƿ� ����
            panelCanvasGroup.alpha -= Time.deltaTime / fadeDuration;

            // ���� ���� 0 ���ϸ� ������Ʈ ��Ȱ��ȭ �� ó��
            if (panelCanvasGroup.alpha <= 0f)
            {
                panelCanvasGroup.alpha = 0f;
                isFadingOut = false;
                gameObject.SetActive(false); // �ʿ信 ���� ������Ʈ ��Ȱ��ȭ
            }
        }
    }
}
