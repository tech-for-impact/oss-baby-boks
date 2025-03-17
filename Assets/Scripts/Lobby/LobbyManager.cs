using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("UI 이미지/배치 관련")]
    public RectTransform[] images;    // 스테이지를 나타내는 RectTransform 배열
    public float[] sizes;             // 각 스테이지(이미지)의 크기(가로/세로)를 지정할 배열
    public Vector2 centerPosition;    // 중앙 기준이 될 위치 (이미지 배치를 위한 중심점)
    public float spacing = 500f;      // 이미지 간 간격
    public float moveSpeed = 5f;      // 크기 보간 시 사용될 속도
    public float smoothTime = 0.1f;   // 부드러운 이동(SmoothDamp)에 쓰이는 보간 시간

    [Header("씬 전환 관련")]
    public string[] sceneNames;       // 각 이미지(스테이지)별로 전환할 씬 이름 목록

    [Header("점수 UI")]
    public TMP_Text bestScoreText;    // 최고 점수를 표시할 TextMeshPro UI
    public TMP_Text totalScoreText;   // 누적 점수를 표시할 TextMeshPro UI

    [Header("스프라이트 (잠금/해제)")]
    public Sprite lockedSprite;       // 스테이지가 잠겨 있을 때 표시할 스프라이트
    public Sprite unlockedSprite;     // 스테이지가 해제되어 있을 때 표시할 스프라이트

    [Header("Stage Details UI")]
    public TMP_Text stageNumberTMP;   // 선택된 스테이지의 번호를 표시할 TextMeshPro
    public TMP_Text stageNameTMP;     // 선택된 스테이지의 이름을 표시할 TextMeshPro
    public Image stageLockImage;      // 선택된 스테이지의 잠금 상태를 표시할 Image

    [Header("사용자 지정 스테이지 이름")]
    public string[] customStageNames; // 각 스테이지의 이름을 직접 지정할 문자열 배열

    // ========================================================
    // [Private 상태 변수]
    // ========================================================
    private bool isSelectedStage = false;
    private int currentIndex = 0;         // 스테이지 선택을 위한 기준 인덱스
    private Vector2[] velocities;         // 이미지 이동(SmoothDamp) 시 사용
    private int initialOffset = 5;        // 초기 선택 위치 지정

    private double previousLeftDiff = 0;
    private double previousRightDiff = 0;

    void Start()
    {
        currentIndex = initialOffset % images.Length;
        velocities = new Vector2[images.Length];
        UpdateImagePositions();
        GameManager.Instance.InitializeStages(images.Length);
        GameManager.Instance.CheckAndUnlockStages();
        UpdateStageScores(currentIndex);
    }

    void Update()
    {
        if (!GameManager.InputEnabled)
            return;

        bool kb_qPressed = Input.GetKeyDown(KeyCode.Q);
        bool kb_oPressed = Input.GetKeyDown(KeyCode.O);
        bool kb_aPressed = Input.GetKeyDown(KeyCode.A);
        bool kb_lPressed = Input.GetKeyDown(KeyCode.L);

        bool qPressed = kb_qPressed;
        bool oPressed = kb_oPressed;
        bool aPressed = kb_aPressed;
        bool lPressed = kb_lPressed;

        if (qPressed) MoveLeft();
        else if (oPressed) MoveRight();

        if (aPressed) TryLoadSelectedScene();

        if (lPressed)
            GameManager.Instance.TransitionToScene("1_StartScene");

        UpdateImagePositions();
    }

    void MoveLeft()
    {
        AudioManager.Instance.PlaySound("UI_Move");
        currentIndex = (currentIndex - 1 + images.Length) % images.Length;
        UpdateStageScores(currentIndex);
    }

    void MoveRight()
    {
        AudioManager.Instance.PlaySound("UI_Move");
        currentIndex = (currentIndex + 1) % images.Length;
        UpdateStageScores(currentIndex);
    }

    void UpdateImagePositions()
    {
        int centerImageIndex = (currentIndex + images.Length / 2) % images.Length;
        for (int i = 0; i < images.Length; i++)
        {
            int relativeIndex = (i - currentIndex + images.Length * 2) % images.Length;
            float xOffset = (relativeIndex - images.Length / 2) * spacing;
            float targetSize = sizes[relativeIndex];
            Vector2 targetPosition = new Vector2(centerPosition.x + xOffset, centerPosition.y);

            images[i].anchoredPosition = Vector2.SmoothDamp(
                images[i].anchoredPosition,
                targetPosition,
                ref velocities[i],
                smoothTime
            );

            images[i].sizeDelta = Vector2.Lerp(
                images[i].sizeDelta,
                new Vector2(targetSize, targetSize),
                Time.deltaTime * moveSpeed
            );

            // 개별 옵션 UI 갱신: center에 해당하는 스테이지만 업데이트
            UpdateStageAppearance(i);

            if (i == centerImageIndex ||
                i == (centerImageIndex + 1) % images.Length ||
                i == (centerImageIndex - 1 + images.Length) % images.Length)
            {
                images[i].SetSiblingIndex(images.Length - 1);
            }
            else
            {
                images[i].SetSiblingIndex(0);
            }
        }
    }

    // center에 해당하는(선택된) 스테이지의 번호, 이름, 잠금 상태를 업데이트합니다.
    void UpdateStageAppearance(int stageIndex)
    {
        int centerImageIndex = (currentIndex + images.Length / 2) % images.Length;
        if (stageIndex == centerImageIndex)
        {
            bool isUnlocked = GameManager.Instance.IsStageUnlocked(centerImageIndex);

            // 스테이지 번호: 인덱스+1로 표시
            if (stageNumberTMP != null)
                stageNumberTMP.text = "Stage " + (centerImageIndex + 1);

            // 스테이지 이름: 직접 지정한 customStageNames 배열 사용
            if (stageNameTMP != null)
            {
                if (customStageNames != null && customStageNames.Length > centerImageIndex)
                    stageNameTMP.text = customStageNames[centerImageIndex];
                else
                    stageNameTMP.text = "Stage " + (centerImageIndex + 1);
            }

            // 잠금 상태 이미지: 잠금 여부에 따라 스프라이트 갱신
            if (stageLockImage != null)
                stageLockImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        }
    }

    void TryLoadSelectedScene()
    {
        int centerImageIndex = (currentIndex + images.Length / 2) % images.Length;
        Debug.Log(centerImageIndex);
        if (GameManager.Instance.IsStageUnlocked(centerImageIndex))
        {
            AudioManager.Instance.PlaySound("UI_StagePick");
            string sceneToLoad = sceneNames[centerImageIndex];
            Debug.Log($"Loading scene: {sceneToLoad}");
            GameManager.Instance.TransitionToScene(sceneToLoad);
        }
        else
        {
            AudioManager.Instance.PlaySound("Stage_BadNote");
            Debug.LogWarning($"Stage {centerImageIndex + 1} is locked!");
        }
    }

    void UpdateStageScores(int stageIndex)
    {
        int centerImageIndex = (currentIndex + images.Length / 2) % images.Length;

        int bestScore = PlayerPrefs.GetInt($"BestScore_{centerImageIndex}", 0);
        int totalScore = PlayerPrefs.GetInt($"TotalScore_{centerImageIndex}", 0);

        Debug.Log($"Best score: {bestScore} (Stage {centerImageIndex})");
        Debug.Log($"Total score: {totalScore} (Stage {centerImageIndex})");

        if (bestScoreText != null)
            bestScoreText.text = $"{bestScore}";
        if (totalScoreText != null)
            totalScoreText.text = $"{totalScore}";

        // center 스테이지의 상세 UI(번호, 이름, 잠금 상태) 갱신
        UpdateStageAppearance(centerImageIndex);
    }
}
