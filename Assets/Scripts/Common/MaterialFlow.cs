using UnityEngine;

public class MaterialFlow : MonoBehaviour
{
    public float flowSpeed = 0.5f;
    public string textureProperty = "_MainTex";

    private Material[] materials;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            materials = renderer.materials;
        }
    }

    void Update()
    {
        if (materials != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                Vector2 offset = materials[i].GetTextureOffset(textureProperty);
                offset.y += flowSpeed * Time.deltaTime;
                offset.y = Mathf.Repeat(offset.y, 1f); // offset 값을 0~1 범위로 반복
                materials[i].SetTextureOffset(textureProperty, offset);
            }
        }
    }
}
