using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProc : MonoBehaviour
{
    public Material MatPostProc;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, MatPostProc);
    }
}
