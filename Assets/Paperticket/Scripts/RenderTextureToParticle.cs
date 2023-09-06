using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureToParticle : MonoBehaviour {

    public ParticleSystem _particleSystem;
    public MeshRenderer previewQuad;
    public RenderTexture renderTexture;
    public TextureFormat textureFormat;

    ParticleSystem.ShapeModule shapeModule;
    Texture2D texture;

    public bool supported = true;
    public bool assign = true;

    void OnEnable() {
        texture = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
        texture.name = "Procedural Video Screen";

        shapeModule = _particleSystem.shape;

        shapeModule.texture = texture;
        if (previewQuad != null) previewQuad.material.mainTexture = texture;

        StartCoroutine(CopyRTToTexture());
    }


    IEnumerator CopyRTToTexture() {

        while (true) {

            yield return new WaitForEndOfFrame();

            if (supported) {

                //Debug.Log($"CopyTexture");
                Graphics.CopyTexture(renderTexture, texture);

            } else {
                // Shouldn't ever need this else statement
                //Debug.Log($"ReadPixels");

                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0.0f, 0.0f, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                RenderTexture.active = null;
            }

            if (assign) {
                shapeModule.texture = texture;
            }

        }
    }

}
