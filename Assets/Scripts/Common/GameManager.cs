using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool InputEnabled = true;
    public bool[] Item;  // 아이템 착용 상태 (true: 착용됨, false: 미착용)

    [SerializeField] private bool[] stageUnlockConditions; // 스테이지 잠금 상태 배열
    [SerializeField] private int[] stageMaxScores;           // 각 스테이지의 최고 점수 (달성 조건)

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Slide Panel")]
    public RectTransform panel1;    // 첫 번째 패널
    public RectTransform panel2;    // 두 번째 패널
    public float fastSpeed = 0.4f;  // panel1 애니메이션 시간
    public float slowSpeed = 0.6f;  // panel2 애니메이션 시간
    public AnimationCurve easeCurve;// 가속 & 감속 곡선

    [SerializeField]
    private int currentStageIndex = 0; // 현재 진행 중인 스테이지 인덱스

    [Header("인벤토리 연동")]
    public InventoryItem[] inventoryItems;  // 인벤토리 아이템 배열 (인벤토리 UI 각 칸)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadStageData();
        Item = new bool[10];

        // 저장된 아이템 착용 상태를 불러옴
        for (int i = 0; i < Item.Length; i++)
        {
            Item[i] = LoadItemEquipState(i);
            Debug.Log("Item[" + i + "] loaded state: " + (Item[i] ? "Active" : "Inactive"));
        }

        // 각 스테이지의 누적 점수를 확인하여 10000점 이상이면 해당 아이템 활성화
        // (조건에 맞을 경우 PlayerPrefs에도 저장)
        for (int i = 0; i < stageUnlockConditions.Length; i++)
        {
            int cumulativeScore = PlayerPrefs.GetInt($"StageScore_{i}", 0);
            if (cumulativeScore >= 10000)
            {
                Item[i] = true;
                SaveItemEquipState(i, true);
                Debug.Log($"Stage {i} 누적 점수({cumulativeScore})가 10000점 이상이므로 Item[{i}] 활성화");
            }
        }
    }

    public void InitializeStages(int stageCount)
    {
        if (stageUnlockConditions == null || stageUnlockConditions.Length != stageCount)
        {
            stageUnlockConditions = new bool[stageCount];
            stageUnlockConditions[0] = true; // 첫 번째 스테이지 기본 잠금 해제
        }
    }

    public bool IsStageUnlocked(int stageIndex)
    {
        if (stageUnlockConditions == null || stageIndex < 0 || stageIndex >= stageUnlockConditions.Length)
        {
            return false;
        }
        return stageUnlockConditions[stageIndex];
    }

    // 아이템 토글 및 상태 저장
    public void ItemEquipment(int num)
    {
        Item[num] = !Item[num];
        SaveItemEquipState(num, Item[num]);
        Debug.Log("Item[" + num + "] is now " + (Item[num] ? "Active" : "Inactive"));
    }

    // 스테이지 점수 저장 시, 누적 점수가 해당 스테이지의 최고점수(달성 조건)에 도달하면 인벤토리 아이템 잠금 해제
    public void SaveStageScore(int stageIndex, int score)
    {
        int previousScore = PlayerPrefs.GetInt($"StageScore_{stageIndex}", 0);
        int newScore = previousScore + score;

        PlayerPrefs.SetInt($"StageScore_{stageIndex}", newScore);
        PlayerPrefs.Save();

        Debug.Log($"Stage {stageIndex} Score Updated: {newScore}/{stageMaxScores[stageIndex]}");

        // 최고점수를 달성했으면 해당 인벤토리 아이템 잠금 해제 (0번 인덱스 → 1스테이지 등으로 대응)
        CheckAndUnlockInventoryItem(stageIndex, newScore);

        CheckAndUnlockStages();
    }

    // 인벤토리 아이템 잠금 해제 조건 체크
    private void CheckAndUnlockInventoryItem(int stageIndex, int newScore)
    {
        // stageIndex가 0부터 시작한다고 가정하고,
        // 해당 스테이지의 최고점수를 달성하면(inventoryItems 배열도 0부터 대응) 아이템 잠금 해제
        if (inventoryItems != null && stageIndex >= 0 && stageIndex < inventoryItems.Length)
        {
            if (newScore >= stageMaxScores[stageIndex])
            {
                inventoryItems[stageIndex].UnlockItem();
                Debug.Log($"Inventory item {stageIndex + 1} unlocked due to achieving max score in stage {stageIndex + 1}.");
            }
        }
    }

    private void LoadStageData()
    {
        for (int i = 0; i < stageUnlockConditions.Length; i++)
        {
            stageUnlockConditions[i] = PlayerPrefs.GetInt($"StageUnlocked_{i}", i == 0 ? 1 : 0) == 1;
        }
    }

    public void CheckAndUnlockStages()
    {
        for (int i = 1; i < stageUnlockConditions.Length; i++)
        {
            int previousStageTotalScore = PlayerPrefs.GetInt($"TotalScore_{i - 1}", 0);

            if (previousStageTotalScore >= stageMaxScores[i - 1] && !stageUnlockConditions[i])
            {
                UnlockStage(i);
            }
        }
    }

    public void UnlockStage(int stageIndex)
    {
        if (stageUnlockConditions == null || stageIndex < 0 || stageIndex >= stageUnlockConditions.Length)
        {
            Debug.LogError("Invalid stage index or unlock conditions not initialized.");
            return;
        }

        stageUnlockConditions[stageIndex] = true;
        PlayerPrefs.SetInt($"StageUnlocked_{stageIndex}", 1);
        PlayerPrefs.Save();

        Debug.Log($"Stage {stageIndex} unlocked!");
    }

    public void TransitionToScene(string sceneName)
    {
        // 전역 입력 비활성화
        InputEnabled = false;
        if (EventSystem.current != null)
            EventSystem.current.enabled = false;

        StartCoroutine(SlideAndSwitchScenes(sceneName));
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));

        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        sceneLoadOperation.allowSceneActivation = false;

        while (!sceneLoadOperation.isDone)
        {
            if (sceneLoadOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(fadeDuration);
                sceneLoadOperation.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return StartCoroutine(Fade(0f));

        InputEnabled = true;
        if (EventSystem.current != null)
            EventSystem.current.enabled = true;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FadeCanvasGroup is not assigned.");
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator SlideAndSwitchScenes(string sceneName)
    {
        // 씬 이동 시작 → 오브젝트 이동 활성화
        SceneMoveEff.SetIsMovingObj(true);

        AudioManager.Instance.PlaySound("UI_Move");
        float screenWidth = Screen.width;

        panel1.anchoredPosition = new Vector2(-screenWidth, panel1.anchoredPosition.y);
        panel2.anchoredPosition = new Vector2(screenWidth, panel2.anchoredPosition.y);

        Coroutine slide1 = StartCoroutine(SlidePanel(panel1, 0, fastSpeed));
        Coroutine slide2 = StartCoroutine(SlidePanel(panel2, 0, slowSpeed));
        yield return slide1;
        yield return slide2;

        yield return new WaitForSeconds(1.0f);

        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        sceneLoadOperation.allowSceneActivation = false;

        while (!sceneLoadOperation.isDone)
        {
            if (sceneLoadOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1.0f);
                sceneLoadOperation.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        slide1 = StartCoroutine(SlidePanel(panel1, -screenWidth, fastSpeed));
        slide2 = StartCoroutine(SlidePanel(panel2, screenWidth, slowSpeed));
        yield return slide1;
        yield return slide2;

        // 씬 이동 완료 → 오브젝트 이동 비활성화
        SceneMoveEff.SetIsMovingObj(false);

        InputEnabled = true;
        if (EventSystem.current != null)
            EventSystem.current.enabled = true;
    }

    private IEnumerator SlidePanel(RectTransform panel, float targetX, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPos = panel.anchoredPosition;
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsedTime / duration);
            panel.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panel.anchoredPosition = targetPos;
    }

    // ------------------------------------------------------------------
    // PlayerPrefs를 이용해 아이템 착용 상태를 저장/불러오는 메소드
    // ------------------------------------------------------------------
    public void SaveItemEquipState(int index, bool isEquipped)
    {
        PlayerPrefs.SetInt("ItemEquip_" + index, isEquipped ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool LoadItemEquipState(int index)
    {
        return PlayerPrefs.GetInt("ItemEquip_" + index, 0) == 1;
    }
}
