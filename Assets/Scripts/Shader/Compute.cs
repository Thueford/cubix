using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compute : MonoBehaviour
{
    public ComputeShader bloom, chromaticAberration;

    public RenderTexture sourceTex, brightTex, blurrBuff, caTex;

    public bool useBloom = true;
    public bool useCA = true;

    [Range(1, 20)]
    public uint bloomBlurrAmount = 1;

    [Range(0.01f, 1f)]
    public float bloomThreshold = .5f;

    [Range(1, 10)]
    public int caAmount = 3;

    private const int numThreads = 24;

    private int xThreads = Screen.width / numThreads;
    private int yThreads = Screen.width / numThreads;

    // Start is called before the first frame update
    void Start()
    {
        sourceTex = createTexture();
        brightTex = createTexture();
        blurrBuff = createTexture();
        caTex = createTexture();

        setBloomUniforms();
        setCAUniforms();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, sourceTex);

        if (useBloom)
        {
            bloom.SetFloat("threshold", bloomThreshold);
            bloom.Dispatch(0, xThreads, yThreads, 1);

            for (int i = 0; i < bloomBlurrAmount; i++)
            {
                bloom.Dispatch(1, xThreads, yThreads, 1);
                bloom.Dispatch(2, xThreads, yThreads, 1);
            }

            bloom.Dispatch(3, xThreads, yThreads, 1);
        }

        if (useCA)
        {
            chromaticAberration.SetInt("amount", caAmount);
            chromaticAberration.Dispatch(0, xThreads, yThreads, 1);
            Graphics.Blit(caTex, destination);
        }
        else
            Graphics.Blit(sourceTex, destination);

    }

    private RenderTexture createTexture()
    {
        RenderTexture tex = new RenderTexture(Screen.width, Screen.height, 24);
        tex.enableRandomWrite = true;
        tex.Create();
        return tex;
    }

    private void setBloomUniforms()
    {
        bloom.SetFloat("threshold", bloomThreshold);

        bloom.SetTexture(0, "source", sourceTex);
        bloom.SetTexture(0, "BrightSpots", brightTex);

        bloom.SetTexture(1, "BrightSpots", brightTex);
        bloom.SetTexture(1, "BlurrBuffer", blurrBuff);

        bloom.SetTexture(2, "BrightSpots", brightTex);
        bloom.SetTexture(2, "BlurrBuffer", blurrBuff);

        bloom.SetTexture(3, "source", sourceTex);
        bloom.SetTexture(3, "BrightSpots", brightTex);
    }

    private void setCAUniforms()
    {
        chromaticAberration.SetInt("amount", caAmount);

        chromaticAberration.SetTexture(0, "Source", sourceTex);
        chromaticAberration.SetTexture(0, "Result", caTex);
    }
}
