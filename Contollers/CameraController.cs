using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Material effectMat;

    void OnRenderImage(RenderTexture _src, RenderTexture _dest)
    {
        if (effectMat == null)
            return;

        Graphics.Blit(_src, _dest, effectMat);
    }

    private void Start()
    {
    }

    void OnDestroy()
    {
        SetGrayScale(false);
    }

    public void SetGrayScale(bool isGrayscale)
    {
        effectMat.SetFloat("_GrayscaleAmount", isGrayscale ? 0.6f: 0);
        effectMat.SetFloat("_DarkAmount", isGrayscale ? 0.12f : 0);
    }


}
