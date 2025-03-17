using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siccity.GLTFUtility;
using UnityEngine;

public class GLTFImporter : MonoBehaviour
{
    private GameObject bokModel; // 로드된 복어 모델
    private float idleSpeed = 12f; // 흔들리는 속도
    private float idleAmount = 0.0125f; // 흔들리는 크기
    private float rotationAmount = 10f; // 회전 크기 (기울어지는 정도)
    private Vector3 initialPosition; // 초기 위치 저장
    private Quaternion initialRotation; // 초기 회전 저장

    void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "bok_model.glb");
        bokModel = Importer.LoadFromFile(filePath);

        if (bokModel != null)
        {
            bokModel.transform.position = transform.position;
            bokModel.transform.rotation = transform.rotation;
            bokModel.transform.SetParent(transform);
            
            // 초기 위치 & 회전값 저장
            initialPosition = bokModel.transform.localPosition;
            initialRotation = bokModel.transform.localRotation;

            // 모든 Material의 Shader를 Unlit/Texture로 변경
            Renderer[] renderers = bokModel.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = Shader.Find("Unlit/Texture");
                }
            }
        }
    }

    void Update()
    {
        if (bokModel != null)
        {
            float wave = Mathf.Sin(Time.time * idleSpeed) * idleAmount; // 좌우 흔들림 계산
            float rotationWave = Mathf.Sin(Time.time * idleSpeed) * rotationAmount; // 기울어짐 계산

            // 위치 변화
            bokModel.transform.localPosition = initialPosition + new Vector3(wave, 0, 0);

            // 회전 변화 (Z축 기준으로 살짝 기울어짐)
            bokModel.transform.localRotation = initialRotation * Quaternion.Euler(0, rotationWave, 0);
        }
    }
}