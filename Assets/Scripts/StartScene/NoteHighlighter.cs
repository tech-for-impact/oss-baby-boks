using System.Collections;
using UnityEngine;

public class NoteHighlighter : MonoBehaviour
{
    // 판정 기준이 될 위치 (예: 플레이어의 판정 영역)
    public GameObject spotLight;
    public Transform judgementPoint;
    private NoteController lastHighlightedNote = null;

    void Update()
    {
        NoteController closestNote = null;
        float minDistance = float.MaxValue;

        // 활성화된 노트들 중에서 노트타입이 2, 5가 아닌 노트들 중 가장 가까운 노트 찾기
        foreach (NoteController note in NoteController.activeNotes)
        {
            // 노트타입 2 또는 5인 경우 건너뛰기
            if (note.noteType == 2 || note.noteType == 5)
                continue;

            float distance = Vector3.Distance(note.transform.position, judgementPoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNote = note;
            }
        }

        // 활성 노트가 없으면 스포트라이트 비활성화
        if (closestNote == null)
        {
            if (spotLight.activeSelf)
                spotLight.SetActive(false);
            if (lastHighlightedNote != null)
            {
                lastHighlightedNote.SetHighlight(false);
                lastHighlightedNote = null;
            }
            return;
        }

        // 새로운 노트가 감지되면 스포트라이트 활성화 (이전 상태가 비활성화였으면)
        if (!spotLight.activeSelf)
            spotLight.SetActive(true);

        // 이전에 하이라이트된 노트와 변경된 경우, 이전 노트의 하이라이트 해제
        if (lastHighlightedNote != null && lastHighlightedNote != closestNote)
        {
            lastHighlightedNote.SetHighlight(false);
        }

        // 가장 가까운 노트에 하이라이트 적용 및 스포트라이트 위치 업데이트
        if (closestNote != null)
        {
            if (lastHighlightedNote != closestNote)
            {
                StartCoroutine(AnimateSpotAngle());
            }
            spotLight.transform.position = new Vector3(
                closestNote.transform.position.x,
                spotLight.transform.position.y,
                closestNote.transform.position.z
            );
            lastHighlightedNote = closestNote;
        }
    }

    private IEnumerator AnimateSpotAngle()
    {
        Light lightComponent = spotLight.GetComponent<Light>();
        if (lightComponent == null)
            yield break;

        float duration = 0.5f; // spotAngle 변경 시간
        float elapsed = 0f;
        float startAngle = 0.1f;
        float targetAngle = 30f;

        // 시작 각도로 초기화
        lightComponent.spotAngle = startAngle;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            lightComponent.spotAngle = Mathf.Lerp(startAngle, targetAngle, elapsed / duration);
            yield return null;
        }

        lightComponent.spotAngle = targetAngle;
    }
}
