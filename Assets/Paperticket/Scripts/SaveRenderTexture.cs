using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveRenderTexture : MonoBehaviour {


    public RenderTexture renderTexture;
    [Space(5)]
    public string saveLocation = "D:/User Folders/Downloads/MeldingRealities/";
    [Space(10)]

    [InspectorButton("SaveTexture", ButtonWidth = 100)]
    [SerializeField] private bool saveTexture;

    Texture2D tex;

    // Use this for initialization
    public void SaveTexture() {
        Debug.Log("[SaveRenderTexture] Saving render texture to: " + saveLocation);
        byte[] bytes = toTexture2D(renderTexture).EncodeToPNG();
        System.IO.File.WriteAllBytes(saveLocation + "savedrendertex.png", bytes);

        DestroyImmediate(tex);//prevents memory leak
    }
    Texture2D toTexture2D(RenderTexture rTex) {
        tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

}
