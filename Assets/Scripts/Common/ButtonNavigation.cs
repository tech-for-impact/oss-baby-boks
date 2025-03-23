using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum PopupType { Builders, Inventory }

public class ButtonNavigation : MonoBehaviour
{
    // ========================================================
    // [Public 설정 변수] - Inspector에 설정할 변수들
    // ========================================================
    [Header("메인 UI")]
    public GameObject mainUIPanel;         // 메인 UI 패널
    public Button[] mainButtons;           // 메인 UI의 버튼 배열

    [Header("빌더즈 팝업 UI")]
    public GameObject buildersPopupPanel;  // 빌더즈 팝업 패널
    public Button[] buildersPopupButtons;  // 빌더즈 팝업 버튼 배열

    [Header("인벤토리 팝업 UI")]
    public GameObject inventoryPopupPanel; // 인벤토리 팝업 패널
    public Button[] inventoryPopupButtons; // 인벤토리 팝업 버튼 배열

    // ========================================================
    // [Private 상태 변수]
    // ========================================================
    // 조작 관련
    private double previousLeftDiff = 0;
    private double previousRightDiff = 0;

    // 팝업 모드 관련
    private int currentIndex;
    private GameObject currentPopupPanel;   // 현재 활성화된 팝업 패널
    private Button[] currentPopupButtons;     // 현재 활성화된 팝업의 버튼 배열
    private int popupCurrentIndex = 0;        // 현재 선택된 팝업 버튼 인덱스
    private bool isPopupActive = false;       // 팝업 모드 활성 여부
    private PopupType currentPopupType;       // 현재 활성화된 팝업의 종류

    // ========================================================
    // Unity 이벤트 함수
    // ========================================================
    void Start()
    {
        currentIndex = 1;
        if (mainUIPanel != null)
            mainUIPanel.SetActive(true);
        HighlightMainButton(0);

        if (buildersPopupPanel != null)
            buildersPopupPanel.SetActive(false);
        if (inventoryPopupPanel != null)
            inventoryPopupPanel.SetActive(false);
    }

    void Update()
    { 
        if (!GameManager.InputEnabled)
            return;

        bool kb_qPressed = Input.GetKeyDown(KeyCode.Q);
        bool kb_aPressed = Input.GetKeyDown(KeyCode.A);
        bool kb_oPressed = Input.GetKeyDown(KeyCode.O);
        bool kb_lPressed = Input.GetKeyDown(KeyCode.L);

        bool qPressed = (kb_qPressed);
        bool aPressed = (kb_aPressed);
        bool oPressed = (kb_oPressed);
        bool lPressed = (kb_lPressed);

        if (isPopupActive)
        {
            HandlePopupInput(aPressed, lPressed, qPressed, oPressed);
        }
        else
        {
            HandleMainInput(aPressed, qPressed, oPressed);
        }
    }

    // ========================================================
    // 메인 UI 입력 처리 (Q: 좌, O: 우, A: 선택)
    // ========================================================
    void HandleMainInput(bool aPressed, bool qPressed, bool oPressed)
    {
        if (qPressed)
        {
            AudioManager.Instance.PlaySound("UI_Move");
            MoveMainSelection(-1);
        }
        else if (oPressed)
        {
            AudioManager.Instance.PlaySound("UI_Move");
            MoveMainSelection(1);
        }
        else if (aPressed)
        {
            AudioManager.Instance.PlaySound("UI_Select");
            PressMainButton();
        }
    }

    void MoveMainSelection(int direction)
    {
        // 현재 선택된 버튼의 인덱스를 업데이트
        for (int i = 0; i < mainButtons.Length; i++)
        {
            if (EventSystem.current.currentSelectedGameObject == mainButtons[i].gameObject)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex == -1)
            currentIndex = 0;

        // 새로운 인덱스를 계산하고 currentIndex에 반영
        int newIndex = currentIndex + direction;
        if (newIndex < 0) newIndex = mainButtons.Length - 1;
        if (newIndex >= mainButtons.Length) newIndex = 0;

        currentIndex = newIndex; // currentIndex 업데이트
        HighlightMainButton(currentIndex);
    }


    void HighlightMainButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(mainButtons[index].gameObject);
        mainButtons[index].OnSelect(null);
    }

    void PressMainButton()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
    }

    public void GoLobby()
    {
        GameManager.Instance.TransitionToScene("2_Lobby");
    }
    
    // ========================================================
    // 팝업 UI 입력 처리 (팝업 모드: Q: 좌, O: 우, A: 취소/착용 토글, L: 나가기)
    // ========================================================
    void HandlePopupInput(bool aPressed, bool lPressed, bool qPressed, bool oPressed)
    {
        if (currentPopupType == PopupType.Builders)
        {
            if (qPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                MovePopupSelection(-1);
            }
            else if (oPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                MovePopupSelection(1);
            }
            else if (lPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                ClosePopup();
            }
        }
        else if (currentPopupType == PopupType.Inventory)
        {
            if (qPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                MovePopupSelection(-1);
            }
            else if (oPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                MovePopupSelection(1);
            }
            else if (aPressed)
            {
                AudioManager.Instance.PlaySound("UI_Select");
                PressPopupButton();
            }
            else if (lPressed)
            {
                AudioManager.Instance.PlaySound("UI_Move");
                ClosePopup();
            }
        }
    }

    public void PopupSelectionPrev()
    {
        MovePopupSelection(-1);
    }

    public void PopupSelectionNext()
    {
        MovePopupSelection(1);
    }

    void MovePopupSelection(int direction)
    {
        popupCurrentIndex += direction;
        if (popupCurrentIndex < 0)
            popupCurrentIndex = currentPopupButtons.Length - 1;
        if (popupCurrentIndex >= currentPopupButtons.Length)
            popupCurrentIndex = 0;
        HighlightPopupButton(popupCurrentIndex);

        // 빌더즈 팝업의 경우 현재 선택된 패널만 활성화
        if (currentPopupType == PopupType.Builders)
        {
            UpdateBuildersPanel();
        }
    }

    void HighlightPopupButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(currentPopupButtons[index].gameObject);
    }

    void PressPopupButton()
    {
        currentPopupButtons[popupCurrentIndex].onClick.Invoke();
    }

    public void ToggleEquipInventoryItem()
    {
        if (gameObject.name == "Item00")
        {
            GameManager.Instance.ItemEquipment(0);
        }
        else if (gameObject.name == "Item01")
        {
            GameManager.Instance.ItemEquipment(1);
        }
        else if (gameObject.name == "Item02")
        {
            GameManager.Instance.ItemEquipment(2);
        }
        else if (gameObject.name == "Item03")
        {
            GameManager.Instance.ItemEquipment(3);
        }
    }

    // ========================================================
    // 팝업 활성화 / 종료 함수
    // ========================================================
    public void ActivateBuildersPopup()
    {
        ActivatePopup(PopupType.Builders);
    }

    public void ActivateInventoryPopup()
    {
        ActivatePopup(PopupType.Inventory);
    }
    
    public void ActivatePopup(PopupType type)
    {
        isPopupActive = true;
        if (mainUIPanel != null)
            mainUIPanel.SetActive(false);

        currentPopupType = type;
        // 팝업 종류에 따라 활성화할 팝업 패널과 버튼 배열 지정
        switch (type)
        {
            case PopupType.Builders:
                currentPopupPanel = buildersPopupPanel;
                currentPopupButtons = buildersPopupButtons;
                break;
            case PopupType.Inventory:
                currentPopupPanel = inventoryPopupPanel;
                currentPopupButtons = inventoryPopupButtons;
                break;
        }
        if (currentPopupPanel != null)
            currentPopupPanel.SetActive(true);
        popupCurrentIndex = 0;
        HighlightPopupButton(popupCurrentIndex);

        if (currentPopupType == PopupType.Builders)
        {
            UpdateBuildersPanel();
        }
    }

    public void ClosePopup()
    {
        isPopupActive = false;
        if (currentPopupPanel != null)
            currentPopupPanel.SetActive(false);
        if (mainUIPanel != null)
            mainUIPanel.SetActive(true);
        // 이전에 선택한 메인 버튼 인덱스(currentIndex)를 사용하여 하이라이트 처리
        HighlightMainButton(currentIndex);
    }

    void UpdateBuildersPanel()
    {
        for (int i = 0; i < buildersPopupButtons.Length; i++)
        {
            buildersPopupButtons[i].gameObject.SetActive(i == popupCurrentIndex);
        }
    }
}