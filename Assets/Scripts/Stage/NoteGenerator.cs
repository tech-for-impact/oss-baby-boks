using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteGenerator : MonoBehaviour
{
    // 추가: Inspector에서 json 파일 이름을 지정할 수 있습니다.
    // Resources 폴더 내 파일 이름(확장자 제외)을 입력하세요.
    public string jsonFileName = "NoteData";

    [Header("노트 생성 설정")]
    public int bpm = 120;
    public Transform[] lanePositions; // 노트가 생성될 레인 위치 배열
    public float beatOffset = 0f;     // 비트 오프셋 (초 단위)
    public AudioSource audioSource;   // 음악 재생 AudioSource

    [Header("노트 풀 관리")]
    public List<ObjectPool> notePools;
    private Dictionary<int, ObjectPool> notePoolDictionary = new Dictionary<int, ObjectPool>();
    public ObjectPool tempoBarPool;

    [Header("기타")]
    public StageManager scoreManager;  // StageManager 참조

    private float bps;                // 초당 비트 수
    private float nextTempoBarTime;   // 다음 Tempo Bar 생성 시간

    [System.Serializable]
    public class NoteData
    {
        public float beat;
        public int lane;
        public int notetype;
        public int order;
    }

    public List<NoteData> noteDataList = new List<NoteData>();

    void Awake()
    {
        bps = bpm / 60f;
        nextTempoBarTime = 0f;

        LoadNoteData();

        if (scoreManager != null)
        {
            // 전체 노트 개수 전달
            scoreManager.totalNotes = noteDataList.Count;

            // 노트 타입 1의 개수 추출
            int type1Count = 0;
            foreach (NoteData note in noteDataList)
            {
                if (note.notetype == 1)
                    type1Count++;
            }
            // StageManager에 노트 타입 1의 개수 전달
            scoreManager.totalType1Notes = type1Count;
        }

        InitializeNotePools();
    }

    void Start()
    {
        StartCoroutine(StartSpawnNotesAfterDelay(3f));
    }

    IEnumerator StartSpawnNotesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
        StartCoroutine(SpawnNotesCoroutine());
    }

    void InitializeNotePools()
    {
        if (notePools == null || notePools.Count == 0)
        {
            Debug.LogError("notePools 확인");
            return;
        }

        foreach (ObjectPool pool in notePools)
        {
            if (pool == null)
            {
                Debug.LogError("notePools 리스트에 null");
                continue;
            }

            if (pool.prefab == null)
            {
                Debug.LogError($"ObjectPool '{pool.gameObject.name}'에서 prefab이 설정되지 않음");
                continue;
            }

            NoteController sampleNote = pool.prefab.GetComponent<NoteController>();
            if (sampleNote == null)
            {
                Debug.LogError($"NoteController가 {pool.prefab.name}에 없음");
                continue;
            }

            int type = sampleNote.noteType;
            Debug.Log($"🔍 ObjectPool '{pool.gameObject.name}' -> Prefab '{pool.prefab.name}'의 noteType: {type}");

            if (!notePoolDictionary.ContainsKey(type))
            {
                notePoolDictionary[type] = pool;
                Debug.Log($"노트 타입 {type}을(를) ObjectPool에 등록");
            }
            else
            {
                Debug.LogWarning($"노트 타입 {type}이 중복");
            }
        }

        Debug.Log($"총 등록된 노트 타입 개수: {notePoolDictionary.Count}");
    }

    void LoadNoteData()
    {
        // jsonFileName에 Inspector에서 지정한 파일 이름 사용 (Resources 폴더 내 파일, 확장자 제외)
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile != null)
        {
            Serialization<NoteData> loadedData = JsonUtility.FromJson<Serialization<NoteData>>(jsonFile.text);
            noteDataList = loadedData.target;
            for (int i = 0; i < noteDataList.Count; i++)
            {
                noteDataList[i].order = i;
            }
            Debug.Log("Note data loaded successfully. Number of notes: " + noteDataList.Count);
        }
        else
        {
            Debug.LogError($"지정한 json 파일 '{jsonFileName}'을(를) 찾을 수 없습니다.");
        }
    }

    [System.Serializable]
    private class Serialization<T>
    {
        public List<T> target;
        public Serialization(List<T> target)
        {
            this.target = target;
        }
    }

    IEnumerator SpawnNotesCoroutine()
    {
        foreach (NoteData noteData in noteDataList)
        {
            float scheduledTime = noteData.beat / bps + beatOffset;

            float delay = scheduledTime - audioSource.time;
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            int lane = noteData.lane;
            int notetype = noteData.notetype;
            SpawnNote(lane, scheduledTime, noteData.order, notetype);
        }
    }

    void SpawnNote(int lane, float time, int order, int notetype)
    {
        int laneIndex = lane - 1;
        if (!notePoolDictionary.ContainsKey(notetype))
        {
            Debug.LogWarning($"노트 타입 {notetype}에 해당하는 ObjectPool 확인");
            return;
        }

        GameObject note = notePoolDictionary[notetype].GetObject();
        if (laneIndex >= 0 && laneIndex < lanePositions.Length)
        {
            note.transform.position = lanePositions[laneIndex].position;
        }
        else
        {
            Debug.LogWarning("Invalid lane index: " + lane);
        }

        NoteController noteController = note.GetComponent<NoteController>();
        if (noteController != null)
        {
            noteController.Initialize(time, audioSource, laneIndex, notetype, notePoolDictionary[notetype]);
        }
    }

    void Update()
    {
        // 기타 업데이트 처리 (예: UI 업데이트 등)
    }
}
