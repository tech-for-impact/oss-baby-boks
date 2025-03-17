using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    public Material materialToScroll; // ��ũ�� ȿ���� �� ���׸���
    public float scrollSpeed = -0.5f;  // ��ũ�� �ӵ�

    void Update()
    {
        if (materialToScroll != null)
        {
            Vector2 offset = materialToScroll.mainTextureOffset;
            offset.y += scrollSpeed * Time.deltaTime;
            if (offset.y <= -5000f)
            {
                offset.y = 0f;
            }
            materialToScroll.mainTextureOffset = offset;
        }
    }
}
