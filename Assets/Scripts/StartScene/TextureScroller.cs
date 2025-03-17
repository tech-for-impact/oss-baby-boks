using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    public Material materialToScroll; // 스크롤 효과를 줄 마테리얼
    public float scrollSpeed = -0.5f;  // 스크롤 속도

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
