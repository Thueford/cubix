using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProc : MonoBehaviour
{
    public Material MatPostProc1;
    public Material MatPostProc2;
    public bool doTwoSteps = true;

    //public Shader shader;
    private RenderTexture tmp;

    private void Start()
    {
        tmp = new RenderTexture(Screen.width, Screen.height, 24);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (doTwoSteps)
        {
            Graphics.Blit(source, tmp, MatPostProc1);
            Graphics.Blit(tmp, destination, MatPostProc2);
        }
        else
        {
            Graphics.Blit(source, destination, MatPostProc1);
        }
    }
}
