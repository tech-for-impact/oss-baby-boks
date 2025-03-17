using System.Collections;
using UnityEngine;

public class NoteHighlighter : MonoBehaviour
{
    // ���� ������ �� ��ġ (��: �÷��̾��� ���� ����)
    public GameObject spotLight;
    public Transform judgementPoint;
    private NoteController lastHighlightedNote = null;

    void Update()
    {
        NoteController closestNote = null;
        float minDistance = float.MaxValue;

        // Ȱ��ȭ�� ��Ʈ�� �߿��� ��ƮŸ���� 2, 5�� �ƴ� ��Ʈ�� �� ���� ����� ��Ʈ ã��
        foreach (NoteController note in NoteController.activeNotes)
        {
            // ��ƮŸ�� 2 �Ǵ� 5�� ��� �ǳʶٱ�
            if (note.noteType == 2 || note.noteType == 5)
                continue;

            float distance = Vector3.Distance(note.transform.position, judgementPoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNote = note;
            }
        }

        // Ȱ�� ��Ʈ�� ������ ����Ʈ����Ʈ ��Ȱ��ȭ
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

        // ���ο� ��Ʈ�� �����Ǹ� ����Ʈ����Ʈ Ȱ��ȭ (���� ���°� ��Ȱ��ȭ������)
        if (!spotLight.activeSelf)
            spotLight.SetActive(true);

        // ������ ���̶���Ʈ�� ��Ʈ�� ����� ���, ���� ��Ʈ�� ���̶���Ʈ ����
        if (lastHighlightedNote != null && lastHighlightedNote != closestNote)
        {
            lastHighlightedNote.SetHighlight(false);
        }

        // ���� ����� ��Ʈ�� ���̶���Ʈ ���� �� ����Ʈ����Ʈ ��ġ ������Ʈ
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

        float duration = 0.5f; // spotAngle ���� �ð�
        float elapsed = 0f;
        float startAngle = 0.1f;
        float targetAngle = 30f;

        // ���� ������ �ʱ�ȭ
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
