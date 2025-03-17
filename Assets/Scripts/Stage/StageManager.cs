using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public int combo;
    public int maxCombo;
    public Text comboState;         // 예: "COMBO"를 표시하는 부모 텍스트
    public Text comboText;          // 콤보 수치 텍스트
    public TMP_Text scoreText;      // 현재 점수를 표시할 텍스트
    public ParticleSystem particle;

    public int totalNotes;          // 모든 노트의 총 개수 (게임 종료 판별용)
    public int totalType1Notes;     // 노트타입 1번의 총 개수 (점수 계산용)
    public int notesProcessed = 0;  // 처리된 노트 개수
    private float maxScore = 10000f; // 최대 점수 (고정값)
    private float scorePerNote;      // 노트당 점수 (노트타입1 기준)
    public float currentScore = 0f;   // 현재 점수 (인스펙터에서 조작 가능)

    private bool isAnimatingCombo = false;
    private bool isGameOver = false;

    public Image[] lifeImages;      // 생명력 이미지 배열
    public int currentLife;        // 현재 생명력

    public GameObject gameOverPanel;  // 게임 오버 패널
    public TMP_Text checkClear;
    public Image missOverlay;
    public TMP_Text currentScoreText; // 게임 오버 시 최종 점수 표시
    // bestScoreText와 totalScoreText는 더 이상 사용하지 않습니다.

    public Button restartButton;    // 다시 시작 버튼
    public Button lobbyButton;      // 로비 이동 버튼

    public int stageIndex;
    private Coroutine currentComboCoroutine = null;

    // ── 추가된 GameOver Panel UI ─────────────────────────────
    [Header("GameOver Panel UI")]
    public Image[] starImages;      // 좌측부터 3개의 별 이미지 (배열 길이 3)
    public Sprite starLitSprite;    // 별이 빛나는 이미지
    public Sprite starUnlitSprite;  // 별이 빛나지 않는 이미지
    public TMP_Text achievementText; // 별 아래 달성도 문구 (GOOD, EXCELLENT, PERFECT)

    [Header("Item Reward UI")]
    public Image itemImage;         // 아이템 이미지
    public TMP_Text itemNameText;   // 아이템 이름
    // 아이템 획득 여부는 예를 들어 PlayerPrefs("ItemObtained_{stageIndex}")로 관리

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 노트타입 1번의 총 개수를 기준으로 점수 당 값을 계산
        if (totalType1Notes != 0)
            scorePerNote = maxScore / totalType1Notes;
        else
            Debug.LogWarning("totalType1Notes가 0입니다.");

        currentLife = lifeImages.Length;
        UpdateLifeUI();

        gameOverPanel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
        lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void LateUpdate()
    {
        if (!isGameOver)
            return;

        bool qPressed = Input.GetKeyDown(KeyCode.Q);
        bool oPressed = Input.GetKeyDown(KeyCode.O);
        bool aPressed = Input.GetKeyDown(KeyCode.A);
        bool lPressed = Input.GetKeyDown(KeyCode.L);

        if (aPressed)
            RestartGame();
        if (lPressed)
            GoToLobby();
    }

    public void AddScore()
    {
        // 총 노트 처리(전체 노트 개수)와 점수 계산(노트타입 1 기준)은 별도로 처리
        notesProcessed++;
        currentScore += scorePerNote;
        currentScore = Mathf.Clamp(currentScore, 0f, maxScore);

        Debug.Log($"현재 점수: {Mathf.FloorToInt(currentScore)} / {maxScore}");

        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(currentScore).ToString();
        else
            Debug.LogWarning("scoreText가 Inspector에 할당되어 있지 않습니다.");

        if (particle != null)
            particle.Play();

        CheckAllNotesProcessed();
    }

    public void IncreaseCombo()
    {
        combo++;
        if (combo > maxCombo)
            maxCombo = combo;
        UpdateComboTextWithShake(combo);
    }

    public void ResetCombo()
    {
        notesProcessed++;
        combo = 0;
        DecreaseLife();
        UpdateComboTextWithShake(combo, isMiss: true);
        StartCoroutine(FlashMissOverlay(0.25f, 0.3f));
        CheckAllNotesProcessed();
    }

    public void TextByType(string notetype)
    {
        notesProcessed++;
        if (currentComboCoroutine != null)
        {
            StopCoroutine(currentComboCoroutine);
            currentComboCoroutine = null;
            isAnimatingCombo = false;
        }
        comboText.text = notetype + "!";
        StartCoroutine(AnimateTextByTypeEffect());
        CheckAllNotesProcessed();
    }

    private IEnumerator AnimateTextByTypeEffect()
    {
        RectTransform textTransform = comboText.GetComponent<RectTransform>();
        Vector3 originalPosition = textTransform.localPosition;
        Color originalColor = comboText.color;
        originalColor.a = 1f;
        Color parentOriginalColor = comboState.color;
        parentOriginalColor.a = 1f;

        Color flashTargetColor = comboText.text.Contains("MISS") ? Color.red : Color.yellow;
        float flashDuration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            float shakeX = Random.Range(-20f, 20f);
            float shakeY = Random.Range(-20f, 20f);
            textTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
            comboText.color = Color.Lerp(originalColor, flashTargetColor, t);
            if (comboState != null)
                comboState.color = Color.Lerp(parentOriginalColor, flashTargetColor, t);
            yield return null;
        }
        comboText.color = flashTargetColor;
        if (comboState != null)
            comboState.color = flashTargetColor;

        textTransform.localPosition = originalPosition;
        float shakeDuration = 0.3f;
        elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float shakeX = Random.Range(-20f, 20f);
            float shakeY = Random.Range(-20f, 20f);
            textTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
            yield return null;
        }
        textTransform.localPosition = originalPosition;

        float fadeDuration = 1.0f;
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color tempColor = originalColor;
            tempColor.a = newAlpha;
            comboText.color = tempColor;
            yield return null;
        }
        Color finalColor = originalColor;
        finalColor.a = 0f;
        comboText.color = finalColor;
    }

    private void CheckAllNotesProcessed()
    {
        // 게임 종료는 전체 노트(totalNotes)를 처리했는지로 판단
        if (notesProcessed >= totalNotes)
        {
            Debug.Log("모든 노트 처리 완료");
            StartCoroutine(ShowGameOverPanelAfterDelay(3f));
            checkClear.text = "Clear";
        }
    }

    private IEnumerator ShowGameOverPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameOver();
    }

    private void UpdateComboTextWithShake(int targetCombo, bool isMiss = false)
    {
        if (comboText != null)
        {
            if (currentComboCoroutine != null)
                StopCoroutine(currentComboCoroutine);
            Color c = comboText.color;
            c.a = 1f;
            comboText.color = c;
            currentComboCoroutine = StartCoroutine(AnimateComboTextWithShake(targetCombo, isMiss));
        }
    }

    private IEnumerator AnimateComboTextWithShake(int targetCombo, bool isMiss)
    {
        RectTransform textTransform = comboText.GetComponent<RectTransform>();
        Vector3 originalPosition = textTransform.localPosition;
        Text parentText = comboState;
        Color parentOriginalColor = (parentText != null) ? parentText.color : Color.white;
        parentOriginalColor.a = 1f;
        Color originalColor = comboText.color;
        originalColor.a = 1f;
        comboText.color = originalColor;

        if (isMiss)
        {
            comboState.text = " ";
            comboText.text = "MISS!";
        }
        else
        {
            comboState.text = "COMBO";
            int currentDisplayedCombo = (combo > 0) ? combo - 1 : 0;
            comboText.text = currentDisplayedCombo.ToString();
            while (currentDisplayedCombo != targetCombo)
            {
                currentDisplayedCombo += (currentDisplayedCombo < targetCombo) ? 1 : -1;
                comboText.text = currentDisplayedCombo.ToString();
                yield return new WaitForSeconds(0.01f);
            }
        }

        Color flashTargetColor = isMiss ? Color.red : Color.yellow;
        float flashDuration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            comboText.color = Color.Lerp(originalColor, flashTargetColor, t);
            if (parentText != null)
                parentText.color = Color.Lerp(parentOriginalColor, flashTargetColor, t);
            yield return null;
        }
        comboText.color = flashTargetColor;
        if (parentText != null)
            parentText.color = flashTargetColor;

        elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            comboText.color = Color.Lerp(flashTargetColor, originalColor, t);
            if (parentText != null)
                parentText.color = Color.Lerp(flashTargetColor, parentOriginalColor, t);
            yield return null;
        }
        textTransform.localPosition = originalPosition;
        comboText.color = originalColor;
        if (parentText != null)
            parentText.color = parentOriginalColor;

        float shakeDurationFinal = 0.3f;
        elapsedTime = 0f;
        while (elapsedTime < shakeDurationFinal)
        {
            elapsedTime += Time.deltaTime;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            yield return null;
        }
        textTransform.localPosition = originalPosition;

        float fadeDuration = 1.0f;
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color tempColor = originalColor;
            tempColor.a = newAlpha;
            comboText.color = tempColor;
            if (parentText != null)
            {
                Color tempParent = parentOriginalColor;
                tempParent.a = newAlpha;
                parentText.color = tempParent;
            }
            yield return null;
        }
        comboText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        if (parentText != null)
            parentText.color = new Color(parentOriginalColor.r, parentOriginalColor.g, parentOriginalColor.b, 0f);
        currentComboCoroutine = null;
    }

    private IEnumerator FlashMissOverlay(float duration, float maxAlpha)
    {
        Color originalColor = missOverlay.color;
        originalColor.a = 0f;
        missOverlay.color = originalColor;
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(0f, maxAlpha, t);
            missOverlay.color = newColor;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(maxAlpha, 0f, t);
            missOverlay.color = newColor;
            yield return null;
        }
        originalColor.a = 0f;
        missOverlay.color = originalColor;
    }

    public void IncreaseLife()
    {
        if (currentLife < 7)
        {
            currentLife++;
            UpdateLifeUI();
        }
    }

    public void DecreaseLife()
    {
        if (currentLife > 0)
        {
            currentLife--;
            UpdateLifeUI();
            if (currentLife == 0)
            {
                Debug.Log("Game Over!");
                GameOver();
                checkClear.text = "Failure!";
            }
        }
    }

    private void UpdateLifeUI()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < currentLife)
                lifeImages[i].color = Color.white;
            else
                lifeImages[i].color = new Color(96 / 255f, 96 / 255f, 96 / 255f, 1f);
        }
    }

    private void SaveScore()
    {
        int finalScore = Mathf.FloorToInt(currentScore);
        int bestScore = PlayerPrefs.GetInt($"BestScore_{stageIndex}", 0);
        if (finalScore > bestScore)
        {
            PlayerPrefs.SetInt($"BestScore_{stageIndex}", finalScore);
        }
        int totalScore = PlayerPrefs.GetInt($"TotalScore_{stageIndex}", 0) + finalScore;
        PlayerPrefs.SetInt($"TotalScore_{stageIndex}", totalScore);
        PlayerPrefs.Save();
        Debug.Log($"Stage {stageIndex} - Score saved. Best: {bestScore}, Total: {totalScore}");
    }

    private void GameOver()
    {
        // 플레이어 컨트롤러 비활성화
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        if(GameObject.Find("SpotLight") != null)
            GameObject.Find("SpotLight").SetActive(false);
    
        GameObject.Find("NotePool").SetActive(false);


        int finalScore = Mathf.FloorToInt(currentScore);
        currentScoreText.text = $"점수: {finalScore}";

        // 게임오버 패널의 별 및 달성도 업데이트
        float ratio = (float)finalScore / maxScore;
        if (starImages != null && starImages.Length >= 3 && achievementText != null)
        {
            if (ratio >= 1f)
            {
                starImages[0].sprite = starLitSprite;
                starImages[1].sprite = starLitSprite;
                starImages[2].sprite = starLitSprite;
                achievementText.text = "PERFECT";
            }
            else if (ratio >= 2f / 3f)
            {
                starImages[0].sprite = starLitSprite;
                starImages[1].sprite = starLitSprite;
                starImages[2].sprite = starUnlitSprite;
                achievementText.text = "EXCELLENT";
            }
            else if (ratio >= 1f / 3f)
            {
                starImages[0].sprite = starLitSprite;
                starImages[1].sprite = starUnlitSprite;
                starImages[2].sprite = starUnlitSprite;
                achievementText.text = "GOOD";
            }
            else
            {
                starImages[0].sprite = starUnlitSprite;
                starImages[1].sprite = starUnlitSprite;
                starImages[2].sprite = starUnlitSprite;
                achievementText.text = "";
            }
        }

        // 아이템 획득 여부에 따른 아이템 UI 업데이트
        bool itemObtained = PlayerPrefs.GetInt($"ItemObtained_{stageIndex}", 0) == 1;
        if (itemImage != null && itemNameText != null)
        {
            if (itemObtained)
            {
                itemImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                itemNameText.text = $"<s>{itemNameText.text}</s>";
            }
            else
            {
                itemImage.color = Color.white;
            }
        }

        if (finalScore == 10000)
        {
            PlayerPrefs.SetInt($"StageAchieved10000_{stageIndex}", 1);
            PlayerPrefs.Save();
            Debug.Log($"Stage {stageIndex}에서 10000점을 달성했습니다.");
        }

        int bestScore = PlayerPrefs.GetInt($"BestScore_{stageIndex}", 0);
        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt($"BestScore_{stageIndex}", bestScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"최고 점수: {bestScore}");

        int totalScore = PlayerPrefs.GetInt($"TotalScore_{stageIndex}", 0) + finalScore;
        PlayerPrefs.SetInt($"TotalScore_{stageIndex}", totalScore);
        PlayerPrefs.Save();
        Debug.Log($"누적 점수: {totalScore}");

        if (GameManager.Instance != null)
            GameManager.Instance.SaveStageScore(stageIndex, finalScore);
        else
            Debug.LogWarning("GameManager.Instance가 null입니다.");

        gameOverPanel.SetActive(true);
        StartCoroutine(GameOverUILockCoroutine());
    }

    private IEnumerator GameOverUILockCoroutine()
    {
        yield return new WaitForSeconds(1f);
        isGameOver = true;
    }

    public void RestartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TransitionToScene("3_Stage1");
        isGameOver = false;
    }

    public void GoToLobby()
    {
        gameOverPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.TransitionToScene("2_Lobby");
        isGameOver = false;
    }
}

/*
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public int combo;
    public int maxCombo;
    public Text comboState;   // 부모 텍스트 역할 (예: "COMBO")
    public Text comboText;    // 콤보 수치 텍스트
    public TMP_Text scoreText;
    public ParticleSystem particle;

    public int totalNotes;         // 총 노트 개수 (게임 종료 판단용)
    public int totalType1Notes;    // 노트타입 1번의 총 개수 (점수 계산용)
    public int notesProcessed = 0; // 처리된 노트 개수
    private float maxScore = 10000f; // 최대 점수 (고정값)
    private float scorePerNote;      // 노트당 점수 (노트타입1 기준)
    public float currentScore = 0f;   // 현재 점수 (인스펙터에서 조작 가능)

    private bool isAnimatingCombo = false; // 애니메이션 중인지 확인
    private bool isGameOver = false;

    public Image[] lifeImages; // 생명력 이미지 배열
    private int currentLife;   // 현재 생명력

    public GameObject gameOverPanel; // 게임 오버 패널
    public Image missOverlay;
    public TMP_Text currentScoreText; // 게임 오버 시 최종 점수 표시
    // bestScoreText와 totalScoreText는 더 이상 사용하지 않습니다.

    public Button restartButton;      // 다시 시작 버튼
    public Button lobbyButton;        // 로비 이동 버튼

    public int stageIndex;
    private Coroutine currentComboCoroutine = null; // 진행 중인 콤보 애니메이션 코루틴

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 노트타입 1번의 총 개수를 기준으로 점수 당 값을 계산합니다.
        if (totalType1Notes != 0)
        {
            scorePerNote = maxScore / totalType1Notes;
        }
        else
        {
            Debug.LogWarning("totalType1Notes가 0입니다.");
        }

        currentLife = lifeImages.Length;
        UpdateLifeUI();

        gameOverPanel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
        lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void LateUpdate()
    {
        if (!isGameOver) return;

        bool qPressed = Input.GetKeyDown(KeyCode.Q);
        bool oPressed = Input.GetKeyDown(KeyCode.O);
        bool aPressed = Input.GetKeyDown(KeyCode.A);
        bool lPressed = Input.GetKeyDown(KeyCode.L);

        if (aPressed) RestartGame();
        if (lPressed) GoToLobby();
    }

    public void AddScore()
    {
        notesProcessed++;  // 총 노트 개수 처리에는 totalNotes를 기준으로 합니다.
        currentScore += scorePerNote; // 점수는 노트타입 1번의 총 개수를 기준으로 계산
        currentScore = Mathf.Clamp(currentScore, 0f, maxScore);

        Debug.Log($"현재 점수: {Mathf.FloorToInt(currentScore)} / {maxScore}");

        if (scoreText != null)
        {
            scoreText.text = Mathf.FloorToInt(currentScore).ToString();
        }
        else
        {
            Debug.LogWarning("scoreText가 Inspector에 할당되어 있지 않습니다.");
        }

        if (particle != null) particle.Play();

        CheckAllNotesProcessed();
    }

    public void IncreaseCombo()
    {
        combo++;
        if (combo > maxCombo) maxCombo = combo;
        UpdateComboTextWithShake(combo);
    }

    public void ResetCombo()
    {
        notesProcessed++;
        combo = 0;
        DecreaseLife();
        UpdateComboTextWithShake(combo, isMiss: true);
        StartCoroutine(FlashMissOverlay(0.25f, 0.3f));
        CheckAllNotesProcessed();
    }

    public void TextByType(string notetype)
    {
        notesProcessed++;
        if (currentComboCoroutine != null)
        {
            StopCoroutine(currentComboCoroutine);
            currentComboCoroutine = null;
            isAnimatingCombo = false;
        }
        comboText.text = notetype + "!";
        StartCoroutine(AnimateTextByTypeEffect());
        CheckAllNotesProcessed();
    }

    private IEnumerator AnimateTextByTypeEffect()
    {
        RectTransform textTransform = comboText.GetComponent<RectTransform>();
        Vector3 originalPosition = textTransform.localPosition;
        Color originalColor = comboText.color;
        originalColor.a = 1f;
        Color parentOriginalColor = comboState.color;
        parentOriginalColor.a = 1f;

        Color flashTargetColor = comboText.text.Contains("MISS") ? Color.red : Color.yellow;

        float flashDuration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            float shakeX = Random.Range(-20f, 20f);
            float shakeY = Random.Range(-20f, 20f);
            textTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
            comboText.color = Color.Lerp(originalColor, flashTargetColor, t);
            if (comboState != null)
                comboState.color = Color.Lerp(parentOriginalColor, flashTargetColor, t);
            yield return null;
        }
        comboText.color = flashTargetColor;
        if (comboState != null)
            comboState.color = flashTargetColor;

        textTransform.localPosition = originalPosition;

        float shakeDuration = 0.3f;
        elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float shakeX = Random.Range(-20f, 20f);
            float shakeY = Random.Range(-20f, 20f);
            textTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
            yield return null;
        }
        textTransform.localPosition = originalPosition;

        float fadeDuration = 1.0f;
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color tempColor = originalColor;
            tempColor.a = newAlpha;
            comboText.color = tempColor;
            yield return null;
        }

        Color finalColor = originalColor;
        finalColor.a = 0f;
        comboText.color = finalColor;
    }

    private void CheckAllNotesProcessed()
    {
        // 게임 종료는 모든 노트(totalNotes)를 처리했는지로 판단합니다.
        if (notesProcessed >= totalNotes)
        {
            Debug.Log("모든 노트 처리 완료");
            StartCoroutine(ShowGameOverPanelAfterDelay(3f));
        }
    }

    private IEnumerator ShowGameOverPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameOver();
    }

    private void UpdateComboTextWithShake(int targetCombo, bool isMiss = false)
    {
        if (comboText != null)
        {
            if (currentComboCoroutine != null)
            {
                StopCoroutine(currentComboCoroutine);
            }
            Color c = comboText.color;
            c.a = 1f;
            comboText.color = c;
            currentComboCoroutine = StartCoroutine(AnimateComboTextWithShake(targetCombo, isMiss));
        }
    }

    private IEnumerator AnimateComboTextWithShake(int targetCombo, bool isMiss)
    {
        RectTransform textTransform = comboText.GetComponent<RectTransform>();
        Vector3 originalPosition = textTransform.localPosition;
        Text parentText = comboState;
        Color parentOriginalColor = (parentText != null) ? parentText.color : Color.white;
        parentOriginalColor.a = 1f;
        Color originalColor = comboText.color;
        originalColor.a = 1f;
        comboText.color = originalColor;

        if (isMiss)
        {
            comboState.text = " ";
            comboText.text = "MISS!";
        }
        else
        {
            comboState.text = "COMBO";
            int currentDisplayedCombo = (combo > 0) ? combo - 1 : 0;
            comboText.text = currentDisplayedCombo.ToString();
            while (currentDisplayedCombo != targetCombo)
            {
                currentDisplayedCombo += (currentDisplayedCombo < targetCombo) ? 1 : -1;
                comboText.text = currentDisplayedCombo.ToString();
                yield return new WaitForSeconds(0.01f);
            }
        }

        Color flashTargetColor = isMiss ? Color.red : Color.yellow;
        float flashDuration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            comboText.color = Color.Lerp(originalColor, flashTargetColor, t);
            if (parentText != null)
                parentText.color = Color.Lerp(parentOriginalColor, flashTargetColor, t);
            yield return null;
        }
        comboText.color = flashTargetColor;
        if (parentText != null)
            parentText.color = flashTargetColor;

        elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            comboText.color = Color.Lerp(flashTargetColor, originalColor, t);
            if (parentText != null)
                parentText.color = Color.Lerp(flashTargetColor, parentOriginalColor, t);
            yield return null;
        }

        textTransform.localPosition = originalPosition;
        comboText.color = originalColor;
        if (parentText != null)
            parentText.color = parentOriginalColor;

        float shakeDurationFinal = 0.3f;
        elapsedTime = 0f;
        while (elapsedTime < shakeDurationFinal)
        {
            elapsedTime += Time.deltaTime;
            textTransform.localPosition = originalPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            yield return null;
        }
        textTransform.localPosition = originalPosition;

        float fadeDuration = 1.0f;
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color tempColor = originalColor;
            tempColor.a = newAlpha;
            comboText.color = tempColor;

            if (parentText != null)
            {
                Color tempParent = parentOriginalColor;
                tempParent.a = newAlpha;
                parentText.color = tempParent;
            }
            yield return null;
        }

        comboText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        if (parentText != null)
            parentText.color = new Color(parentOriginalColor.r, parentOriginalColor.g, parentOriginalColor.b, 0f);

        currentComboCoroutine = null;
    }

    private IEnumerator FlashMissOverlay(float duration, float maxAlpha)
    {
        Color originalColor = missOverlay.color;
        originalColor.a = 0f;
        missOverlay.color = originalColor;

        float halfDuration = duration / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(0f, maxAlpha, t);
            missOverlay.color = newColor;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(maxAlpha, 0f, t);
            missOverlay.color = newColor;
            yield return null;
        }

        originalColor.a = 0f;
        missOverlay.color = originalColor;
    }

    public void IncreaseLife()
    {
        if (currentLife < 7)
        {
            currentLife++;
            UpdateLifeUI();
        }
    }

    public void DecreaseLife()
    {
        if (currentLife > 0)
        {
            currentLife--;
            UpdateLifeUI();

            if (currentLife == 0)
            {
                Debug.Log("Game Over!");
                GameOver();
            }
        }
    }

    private void UpdateLifeUI()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < currentLife)
                lifeImages[i].color = Color.white;
            else
                lifeImages[i].color = new Color(96 / 255f, 96 / 255f, 96 / 255f, 1f);
        }
    }

    private void SaveScore()
    {
        int finalScore = Mathf.FloorToInt(currentScore);

        int bestScore = PlayerPrefs.GetInt($"BestScore_{stageIndex}", 0);
        if (finalScore > bestScore)
        {
            PlayerPrefs.SetInt($"BestScore_{stageIndex}", finalScore);
        }

        int totalScore = PlayerPrefs.GetInt($"TotalScore_{stageIndex}", 0) + finalScore;
        PlayerPrefs.SetInt($"TotalScore_{stageIndex}", totalScore);

        PlayerPrefs.Save();
        Debug.Log($"Stage {stageIndex} - Score saved. Best: {bestScore}, Total: {totalScore}");
    }

    private void GameOver()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        int finalScore = Mathf.FloorToInt(currentScore);
        currentScoreText.text = $"점수: {finalScore}";

        if (finalScore == 10000)
        {
            PlayerPrefs.SetInt($"StageAchieved10000_{stageIndex}", 1);
            PlayerPrefs.Save();
            Debug.Log($"Stage {stageIndex}에서 10000점을 달성했습니다.");
        }

        int bestScore = PlayerPrefs.GetInt($"BestScore_{stageIndex}", 0);
        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt($"BestScore_{stageIndex}", bestScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"최고 점수: {bestScore}");

        int totalScore = PlayerPrefs.GetInt($"TotalScore_{stageIndex}", 0) + finalScore;
        PlayerPrefs.SetInt($"TotalScore_{stageIndex}", totalScore);
        PlayerPrefs.Save();
        Debug.Log($"누적 점수: {totalScore}");

        GameManager.Instance.SaveStageScore(stageIndex, finalScore);

        gameOverPanel.SetActive(true);
        StartCoroutine(GameOverUILockCoroutine());
    }

    private IEnumerator GameOverUILockCoroutine()
    {
        yield return new WaitForSeconds(1f);
        isGameOver = true;
    }

    public void RestartGame()
    {
        GameManager.Instance.TransitionToScene("3_Stage1");
        isGameOver = false;
    }

    public void GoToLobby()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance.TransitionToScene("2_Lobby");
        isGameOver = false;
    }
}
*/