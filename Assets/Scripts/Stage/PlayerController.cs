using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    // ========================================================
    // [Public 설정 변수] - Inspector에서 설정할 변수들
    // ========================================================
    [Header("이동 및 노트 관련 설정")]
    public int moveStep = 1;                           // 한 번 입력당 이동 비율
    public float moveSpeed = 10f;                      // 이동 속도
    public float[] positionsX = new float[] { -1.5f, 0f, 1.5f };  // 플레이어가 위치할 수 있는 X 좌표
    public ObjectPool notePool;                        // 노트 풀 참조
    public StageManager stageManager;                  // ScoreManager 참조

    [Header("UI 설정")]
    public Image[] leftUI;                             // 왼쪽 UI 이미지 배열
    public Image[] rightUI;                            // 오른쪽 UI 이미지 배열
    public Color filledColor = Color.red;              // 채워진 색상
    public Color emptyColor = Color.white;             // 빈 색상

    [Header("효과 및 머티리얼")]
    public Material superMaterial;                     // 슈퍼 효과용 머티리얼 (노트 타입 4)
    public float scaleChangeDuration = 5f;             // 스케일 변경 지속 시간

    [Header("기타 효과")]
    public ParticleSystem[] positionParticles;         // 노트 맞았을 때 재생할 파티클들

    // ========================================================
    // [Private 상태 변수] - 내부적으로 관리할 변수들
    // ========================================================
    // 이동 및 입력 상태 변수
    private int currentPositionIndex = 1;              // 현재 레인 인덱스 (예: 1 → 중앙)
    private int previousPositionIndex = 1;             // 이동 시작 전 레인 인덱스 (복귀용)
    private int lastMoveDirection = 0;                 // 마지막 이동 방향 (-1: 좌, 1: 우, 0: 없음)
    private bool isMoving = false;                     // AnimateMove 진행 여부
    private Vector3 targetPosition;                    // 이동 목표 위치
    private int accumulatedMove = 0;                   // 누적 입력값
    private int prevAccumulatedMove = 0;               // 이전 누적 입력값
    private Coroutine animateMoveRoutine = null;       // AnimateMove 코루틴 참조

    // 효과 제어 플래그
    private bool isBouncing = false;                   // BounceBack 효과 진행 중 여부
    private bool isScaling = false;                    // ChangeScaleTemporarily 효과 진행 중 여부

    // 이동 복원 관련
    private Quaternion previousRotation;               // AnimateMove 시작 전 플레이어 회전

    // ========================================================
    // 휠 입력 처리를 위한 변수
    // ========================================================
    private double previousLeftDiff = 0;   // 이전 프레임의 leftDiff
    private double previousRightDiff = 0;  // 이전 프레임의 rightDiff

    // ========================================================
    // Unity 이벤트 함수
    // ========================================================
    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            bool qPressed, aPressed, oPressed, lPressed;
            (qPressed, aPressed, oPressed, lPressed) = CheckCombinedInput();

            HandleInput(qPressed, oPressed, aPressed, lPressed);
        }
        SmoothMove();
    }

    // ========================================================
    // 휠 + 키보드 입력 결합
    // ========================================================
    (bool qPressed, bool aPressed, bool oPressed, bool lPressed) CheckCombinedInput()
    {
        bool kb_qPressed = Input.GetKeyDown(KeyCode.Q);
        bool kb_aPressed = Input.GetKeyDown(KeyCode.A);
        bool kb_oPressed = Input.GetKeyDown(KeyCode.O);
        bool kb_lPressed = Input.GetKeyDown(KeyCode.L);

        bool qPressed = kb_qPressed;
        bool aPressed = kb_aPressed;
        bool oPressed = kb_oPressed;
        bool lPressed = kb_lPressed;

        return (qPressed, aPressed, oPressed, lPressed);
    }

    // ========================================================
    //  HandleInput: 외부에서 q/o/a/l을 인자로 받도록 변경
    // ========================================================
    void HandleInput(bool qPressed, bool oPressed, bool aPressed, bool lPressed)
    {
        if (aPressed)
        {
            Debug.Log("Move Left");
            Move(-moveStep);
            UpdateUI(1, leftUI);
        }
        if (qPressed)
        {
            Debug.Log("Move Left");
            Move(-moveStep);
            UpdateUI(1, leftUI);
        }
        if (oPressed)
        {
            Debug.Log("Move Right");
            Move(moveStep);
            UpdateUI(2, rightUI);
        }
        if (lPressed)
        {
            Debug.Log("Move Right");
            Move(moveStep);
            UpdateUI(2, rightUI);
        }
    }

    void Move(int step)
    {
        int currentMoveDirection = (step > 0) ? 1 : -1;

        if (currentMoveDirection != lastMoveDirection && lastMoveDirection != 0)
        {
            accumulatedMove = step;
            prevAccumulatedMove = 0;
            if (currentMoveDirection > 0)
            {
                UpdateUI(2, rightUI);
                for (int i = 0; i < leftUI.Length; i++)
                    leftUI[i].color = emptyColor;
            }
            else
            {
                UpdateUI(1, leftUI);
                for (int i = 0; i < rightUI.Length; i++)
                    rightUI[i].color = emptyColor;
            }
        }
        else
        {
            prevAccumulatedMove = accumulatedMove;
            accumulatedMove += step;
        }
        lastMoveDirection = currentMoveDirection;

        if (Mathf.Abs(accumulatedMove) >= 4) /////////////////////
        {
            AudioManager.Instance.PlaySound("Stage_Move");
            previousPositionIndex = currentPositionIndex;
            previousRotation = transform.rotation;

            currentPositionIndex = Mathf.Clamp(currentPositionIndex + currentMoveDirection, 0, positionsX.Length - 1);
            targetPosition = new Vector3(positionsX[currentPositionIndex], transform.position.y, transform.position.z);

            animateMoveRoutine = StartCoroutine(AnimateMove());

            accumulatedMove = 0;
            prevAccumulatedMove = 0;
            for (int i = 0; i < leftUI.Length; i++)
                leftUI[i].color = emptyColor;
            for (int i = 0; i < rightUI.Length; i++)
                rightUI[i].color = emptyColor;
        }
    }

    void SmoothMove()
    {
        if (isBouncing)
            return;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    private IEnumerator AnimateMove()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 peakPosition = startPosition + new Vector3(0, 0.25f, 0);
        Quaternion startRotation = transform.rotation;
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = 1 - Mathf.Pow(1 - t, 3); // Ease-out Cubic
            Vector3 finalMove = Vector3.Lerp(peakPosition, targetPosition, t);
            transform.position = finalMove;

            float rotationAngle = 1080 * Mathf.Sign(lastMoveDirection) * t;
            transform.rotation = startRotation * Quaternion.Euler(0, 0, rotationAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        transform.rotation = startRotation; // 회전 복원
        isMoving = false;
    }

    // ========================================================
    // UI 업데이트 함수
    // ========================================================
    void UpdateUI(int moveDirection, Image[] uiImages)
    {
        int currentUIIndex = Mathf.Clamp(Mathf.Abs(accumulatedMove), 0, 3);
        int prevUIIndex = Mathf.Clamp(Mathf.Abs(prevAccumulatedMove), 0, 3);

        if (accumulatedMove == 0 || Mathf.Abs(accumulatedMove) >= 4)
        {
            for (int i = 0; i < uiImages.Length; i++)
                uiImages[i].color = emptyColor;
        }
        else if (accumulatedMove != 0)
        {
            if (accumulatedMove > 0)
            {
                for (int i = 0; i <= currentUIIndex; i++)
                    uiImages[i].color = filledColor;

                if (currentUIIndex != prevUIIndex && prevAccumulatedMove < accumulatedMove && prevAccumulatedMove != 0)
                {
                    for (int i = currentUIIndex + 1; i < uiImages.Length; i++)
                        uiImages[i].color = emptyColor;
                }
            }
            else if (accumulatedMove < 0)
            {
                for (int i = 0; i <= currentUIIndex; i++)
                    uiImages[i].color = filledColor;

                if (currentUIIndex != prevUIIndex && prevAccumulatedMove > accumulatedMove && prevAccumulatedMove != 0)
                {
                    for (int i = currentUIIndex + 1; i < uiImages.Length; i++)
                        uiImages[i].color = emptyColor;
                }
            }
        }
    }

    // ========================================================
    // 파티클 재생 함수
    // ========================================================
    private void PlayParticleAtPosition(int positionIndex)
    {
        if (positionParticles == null || positionParticles.Length <= positionIndex)
            return;
        var particle = positionParticles[positionIndex];
        if (particle != null)
        {
            if (!particle.gameObject.activeInHierarchy)
                particle.gameObject.SetActive(true);
            particle.Play();
        }
    }

    // ========================================================
    // BounceBack: 노트 타입 5(장애물) 충돌 시, 이동 중지 & 이전 레인 복귀
    // ========================================================
    private IEnumerator BounceBack()
    {
        if (animateMoveRoutine != null)
        {
            StopCoroutine(animateMoveRoutine);
            animateMoveRoutine = null;
        }
        transform.DOKill();
        isBouncing = true;
        isMoving = false;

        Vector3 bounceTarget;
        if (currentPositionIndex == 0)
        {
            bounceTarget = new Vector3(positionsX[2], transform.position.y, transform.position.z);
            currentPositionIndex = 2;
        }
        else if (currentPositionIndex == 2)
        {
            bounceTarget = new Vector3(positionsX[0], transform.position.y, transform.position.z);
            currentPositionIndex = 0;
        }
        else if (currentPositionIndex == 1 && previousPositionIndex == 0)
        {
            bounceTarget = new Vector3(positionsX[0], transform.position.y, transform.position.z);
            currentPositionIndex = 0;
        }
        else if (currentPositionIndex == 1 && previousPositionIndex == 2)
        {
            bounceTarget = new Vector3(positionsX[2], transform.position.y, transform.position.z);
            currentPositionIndex = 2;
        }
        else
        {
            yield break;
        }

        Quaternion targetRot = previousRotation;
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveX(bounceTarget.x, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOMoveZ(bounceTarget.z, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(transform.DORotateQuaternion(targetRot * Quaternion.Euler(0, 0, 360), 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOShakePosition(0.3f, new Vector3(0.1f, 0, 0), 10, 90, false, true));

        seq.OnComplete(() =>
        {
            transform.position = bounceTarget;
            transform.rotation = targetRot;
            targetPosition = bounceTarget;
            lastMoveDirection = 0;
            isBouncing = false;
            isMoving = false;
        });

        yield return seq.WaitForCompletion();
    }

    // ========================================================
    // ChangeScaleTemporarily: 노트 타입 4 (슈퍼 효과) 처리
    // ========================================================
    private IEnumerator ScaleOverTimeWithBok(Transform bokTransform, Vector3 originalBokLocalScale, Vector3 parentOriginalScale, Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out 효과
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localScale = newScale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        float finalFactor = parentOriginalScale.x / transform.localScale.x;
        bokTransform.localScale = originalBokLocalScale * finalFactor;
    }

    private IEnumerator ChangeScaleTemporarily()
    {
        isScaling = true;

        // 이 오브젝트의 Renderer를 가져와서 슈퍼 효과용 머티리얼 적용
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("이 오브젝트에서 Renderer 컴포넌트를 찾을 수 없습니다.");
            yield break;
        }

        Material originalMaterial = renderer.material;
        renderer.material = superMaterial;

        Vector3 originalScale = transform.localScale;

        yield return new WaitForSeconds(0.2f);

        // 단계별 타겟 스케일 (이 오브젝트에 적용)
        Vector3 firstTargetScale = originalScale * 2f;
        Vector3 secondTargetScale = originalScale * 1.5f;
        Vector3 thirdTargetScale = originalScale * 2.4f;
        Vector3 fourthTargetScale = originalScale * 1.9f;
        Vector3 finalTargetScale = originalScale * 4.5f; // 최대 크기

        yield return ScaleOverTime(originalScale, firstTargetScale, 0.3f);
        yield return ScaleOverTime(firstTargetScale, secondTargetScale, 0.25f);
        yield return ScaleOverTime(secondTargetScale, thirdTargetScale, 0.35f);
        yield return ScaleOverTime(thirdTargetScale, fourthTargetScale, 0.3f);
        yield return ScaleOverTime(fourthTargetScale, finalTargetScale, 1.5f);

        yield return new WaitForSeconds(scaleChangeDuration);

        yield return DeflateEffect(finalTargetScale, originalScale, 1.5f);

        // 효과 종료 후 원래 머티리얼 복원
        renderer.material = originalMaterial;
        isScaling = false;
    }


    // ========================================================
    // DeflateEffect: 부풀었다가 원래 크기로 돌아가는 효과 (흔들림)
    // ========================================================
    private IEnumerator DeflateEffect(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.Pow(t, 4);
            float shake = Mathf.Sin(elapsedTime * 50f) * 0.05f;
            Vector3 newScale = Vector3.Lerp(startScale, endScale, t);
            newScale.y += shake;
            transform.localScale = newScale;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endScale;
    }

    // ========================================================
    // ScaleOverTime: 일정 시간 동안 선형 보간 + Ease-out으로 스케일 변화
    // ========================================================
    private IEnumerator ScaleOverTime(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endScale;
    }

    // ========================================================
    // 충돌 처리: Note 태그를 가진 오브젝트와 충돌 시 처리
    // ========================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Note"))
        {
            Debug.Log("Hit!");
            NoteController noteController = other.GetComponent<NoteController>();
            if (noteController == null)
                return;

            Debug.Log($"노트 타입: {noteController.noteType}");

            switch (noteController.noteType)
            {
                case 1:
                    StageManager.instance.IncreaseCombo();
                    stageManager?.AddScore();
                    PlayParticleAtPosition(currentPositionIndex);
                    break;
                case 2:
                    if (!isScaling)
                    {
                        StageManager.instance.ResetCombo();
                        StageManager.instance.TextByType("콰광");
                    }
                    break;
                case 3:
                    stageManager.IncreaseLife();
                    PlayParticleAtPosition(currentPositionIndex);
                    StageManager.instance.TextByType("회복");
                    break;
                case 4:
                    if (!isScaling) StartCoroutine(ChangeScaleTemporarily());
                    PlayParticleAtPosition(currentPositionIndex);
                    StageManager.instance.TextByType("슈퍼");
                    break;
                case 5:
                    if (!isScaling) StartCoroutine(BounceBack());
                    break;
                default:
                    Debug.LogWarning($"잘못된 노트 타입: {noteController.noteType}");
                    break;
            }

            other.gameObject.SetActive(false);
            notePool.ReturnObject(other.gameObject);
        } 
    }
}
