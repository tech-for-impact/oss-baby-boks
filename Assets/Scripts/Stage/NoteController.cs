using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    private bool isHit = false; // �̹� ó���Ǿ����� Ȯ���ϴ� �÷���
    public int noteOrder;
    private float hitTime;
    public float noteSpeed;
    private AudioSource audioSource;
    private ObjectPool objectPool;
    public int noteType;
    private float spawnTime;
    private int lane;

    // Ȱ�� ��Ʈ�� �����ϱ� ���� ����Ʈ
    public static List<NoteController> activeNotes = new List<NoteController>();

    // ��Ʈ�� Ȱ��ȭ�� �� ����Ʈ�� �߰�
    void OnEnable()
    {
        activeNotes.Add(this);
    }

    // ��Ʈ�� ��Ȱ��ȭ�� �� ����Ʈ���� ����
    void OnDisable()
    {
        activeNotes.Remove(this);
    }

    // ��Ʈ�� �ʱ�ȭ�� ��, isHit �÷��׸� false�� �����Ͽ� ���� �����ϵ��� ��
    public void Initialize(float time, AudioSource source, int lane, int notetype, ObjectPool pool)
    {
        spawnTime = time;
        audioSource = source;
        this.lane = lane;
        noteType = notetype;
        objectPool = pool;
        isHit = false;
    }

    void Update()
    {
        // �̹� ó���� ��Ʈ�� �� �̻� ������Ʈ���� ����
        if (isHit)
            return;

        float timeToHit = hitTime - audioSource.time;

        if (timeToHit <= 0)
        {
            // ��Ʈ �̵� ó��
            transform.position += Vector3.back * (noteSpeed * Time.deltaTime);

            // z�� ��ġ�� -16f �̸��̸� Miss ó��
            if (transform.position.z < -16f)
            {
                // �� ���� ó���ǵ��� isHit �÷��� ����
                isHit = true;
                Debug.Log("Miss");

                // ��Ʈ Ÿ�� 1�� ���� �� ���� ��쿡 ���� ó��
                if (noteType == 1)
                {
                    StageManager.instance.ResetCombo();
                    StageManager.instance.notesProcessed++;
                }
                else
                {
                    StageManager.instance.notesProcessed++;
                }

                // ��Ʈ�� ��� ��Ȱ��ȭ�ϰ� ��ü Ǯ�� ��ȯ
                objectPool.ReturnObject(gameObject);
            }
        }
    }

    public void SetHighlight(bool enable)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (enable)
                renderer.material.SetFloat("_OutlineWidth", 1.0f);
            else
                renderer.material.SetFloat("_OutlineWidth", 0f);
        }
    }
}
