using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    private bool isHit = false; // 이미 처리되었는지 확인하는 플래그
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

    // 노트가 활성화될 때 리스트에 추가
    void OnEnable()
    {
        activeNotes.Add(this);
    }

    // 노트가 비활성화될 때 리스트에서 제거
    void OnDisable()
    {
        activeNotes.Remove(this);
    }

    // 노트를 초기화할 때, isHit 플래그를 false로 설정하여 재사용 가능하도록 함
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
        // 이미 처리된 노트는 더 이상 업데이트하지 않음
        if (isHit)
            return;

        float timeToHit = hitTime - audioSource.time;

        if (timeToHit <= 0)
        {
            // 노트 이동 처리
            transform.position += Vector3.back * (noteSpeed * Time.deltaTime);

            // z축 위치가 -16f 미만이면 Miss 처리
            if (transform.position.z < -16f)
            {
                // 한 번만 처리되도록 isHit 플래그 설정
                isHit = true;
                Debug.Log("Miss");

                // 노트 타입 1인 경우와 그 외의 경우에 따라 처리
                if (noteType == 1)
                {
                    StageManager.instance.ResetCombo();
                    StageManager.instance.notesProcessed++;
                }
                else
                {
                    StageManager.instance.notesProcessed++;
                }

                // 노트를 즉시 비활성화하고 객체 풀에 반환
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
