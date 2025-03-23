using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    [HideInInspector]
    public bool isHit = false; // 한 번 처리되었는지 확인하는 플래그

    public int noteOrder;
    private float hitTime;
    public float noteSpeed;
    private AudioSource audioSource;
    private ObjectPool objectPool;
    public int noteType;
    private float spawnTime;
    private int lane;

    // 활성 노트를 관리하기 위한 리스트
    public static List<NoteController> activeNotes = new List<NoteController>();

    void OnEnable()
    {
        activeNotes.Add(this);
        isHit = false; // 재사용 시 플래그 초기화
    }

    void OnDisable()
    {
        activeNotes.Remove(this);
    }

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
        float timeToHit = hitTime - audioSource.time;

        if (timeToHit <= 0)
        {
            transform.position += Vector3.back * (noteSpeed * Time.deltaTime);

            if (transform.position.z < -16f)
            {
                // 한 번 처리된 노트는 더 이상 처리하지 않음
                if (isHit)
                    return;

                isHit = true;
                Debug.Log("Miss");

                if (noteType == 1)
                {
                    StageManager.instance.ResetCombo();
                    //StageManager.instance.notesProcessed++;

                }
                else
                {
                    StageManager.instance.notesProcessed++;
                }

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
