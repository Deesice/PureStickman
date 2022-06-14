#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureConverter : MonoBehaviour
{
    [SerializeField] Texture2D texture;
    [ContextMenu("Convert")]
    void Convert()
    {
        var newTexture = new Texture2D(texture.width, texture.height);
        for (int x = 0; x < newTexture.width; x++)
        {
            for (int y = 0; y < newTexture.height; y++)
            {
                var color = texture.GetPixel(x, y);
                color.a = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
                newTexture.SetPixel(x, y, color);
            }
        }
        newTexture.Apply();
        File.WriteAllBytes(AssetDatabase.GetAssetPath(texture).Replace(".png", "2.png"), newTexture.EncodeToPNG());
    }
}
#endif
