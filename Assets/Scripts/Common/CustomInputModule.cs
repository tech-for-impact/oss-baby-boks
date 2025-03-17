using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomInputModule : StandaloneInputModule
{
    private bool ShouldIgnoreEventsOnNoFocusCustom()
    {
#if UNITY_EDITOR
        return !EditorApplication.isRemoteConnected;
#else
        return true;
#endif
    }
    private bool ProcessTouchEventsCustom()
    {
        for (int i = 0; i < input.touchCount; ++i)
        {
            Touch touch = input.GetTouch(i);
            if (touch.type == TouchType.Indirect)
                continue;

            bool pressed, released;
            var pointer = GetTouchPointerEventData(touch, out pressed, out released);
            ProcessTouchPress(pointer, pressed, released);
            if (!released)
            {
                ProcessMove(pointer);
                ProcessDrag(pointer);
            }
            else
            {
                RemovePointerData(pointer);
            }
        }
        return input.touchCount > 0;
    }

    public override bool ShouldActivateModule()
    {
        if (!base.ShouldActivateModule())
            return false;

        bool shouldActivate = forceModuleActive;

        if (!string.IsNullOrEmpty(horizontalAxis))
            shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(horizontalAxis), 0.0f);
        if (!string.IsNullOrEmpty(verticalAxis))
            shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(verticalAxis), 0.0f);
        if (!string.IsNullOrEmpty(submitButton))
            shouldActivate |= input.GetButtonDown(submitButton);
        if (!string.IsNullOrEmpty(cancelButton))
            shouldActivate |= input.GetButtonDown(cancelButton);

        shouldActivate |= input.GetMouseButtonDown(0);
        if (input.touchCount > 0)
            shouldActivate = true;

        return shouldActivate;
    }

    public override void Process()
    {
        if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocusCustom())
            return;

        bool usedEvent = SendUpdateEventToSelectedObject();

        if (!ProcessTouchEventsCustom() && input.mousePresent)
            ProcessMouseEvent();

        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent && (!string.IsNullOrEmpty(horizontalAxis) || !string.IsNullOrEmpty(verticalAxis)))
            {
                usedEvent |= SendMoveEventToSelectedObject();
            }
            if (!usedEvent && (!string.IsNullOrEmpty(submitButton) || !string.IsNullOrEmpty(cancelButton)))
            {
                SendSubmitEventToSelectedObjectCustom();
            }
        }
    }

    protected void SendSubmitEventToSelectedObjectCustom()
    {
        if (eventSystem.currentSelectedGameObject == null)
            return;

        BaseEventData data = GetBaseEventData();
        if (!string.IsNullOrEmpty(submitButton) && input.GetButtonDown(submitButton))
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
        if (!string.IsNullOrEmpty(cancelButton) && input.GetButtonDown(cancelButton))
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
    }
}