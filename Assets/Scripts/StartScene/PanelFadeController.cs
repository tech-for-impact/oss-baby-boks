using UnityEngine;
using UnityEngine.UI;

public class PanelFadeController : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup; // UI 패널에 부착된 CanvasGroup
    public float displayDuration = 3f;   // 표시될 시간(초)
    public float fadeDuration = 0.5f;    // 페이드 아웃에 걸리는 시간(초)

    private float timer = 0f;
    private bool isFadingOut = false;

    void Start()
    {
        // 씬이 시작되면 바로 표시되도록 alpha를 1로 고정
        panelCanvasGroup.alpha = 1f;
        timer = displayDuration;
    }

    void Update()
    {
        // 패널이 표시될 시간이 남아 있다면 시간 감소
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            // 시간이 모두 지난 후 페이드 아웃 시작
            if (timer <= 0f)
            {
                isFadingOut = true;
            }
        }
        else if (isFadingOut)
        {
            // 페이드 아웃 진행
            panelCanvasGroup.alpha -= Time.deltaTime / fadeDuration;

            // 알파 값이 0 이하면 오브젝트 비활성화 등 처리
            if (panelCanvasGroup.alpha <= 0f)
            {
                panelCanvasGroup.alpha = 0f;
                isFadingOut = false;
                gameObject.SetActive(false); // 필요에 따라 오브젝트 비활성화
            }
        }
    }
}
