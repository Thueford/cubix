using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compute : MonoBehaviour
{
    public ComputeShader bloom, lensFlare;

    public Material blurrMat;

    public RenderTexture sourceTex,
        brightTex,
        blurrBuff,
        caResult,
        lfResult;

    public RenderTexture lensDirt;
    public Texture2D lensDirtTex, starburstTex;

    public bool skipAll = false;

    public bool useBloom = true;
    
    [Range(1, 20)]
    public int bloomBlurrAmount = 1;

    [Range(0.01f, 1f)]
    public float bloomThreshold = .65f;

    public bool useCA = true;

    [Range(0f, 0.01f)]
    public float caAmount = 0.001f;

    public bool useLensFlare = true;

    [Range(1, 10)]
    public int lfGhostCount = 3;

    [Range(1, 20)]
    public int lfBlurrCount = 1;

    [Range(0.01f, 1f)]
    public float lfThreshold = 0.7f;

    [Range(0.1f, 2f)]
    public float lfGhostSpacing = .69f;

    [Range(0.0f, 0.3f)]
    public float lfCAStrength = 0.15f;

    public bool useVignette = true;

    [Range(0.0f, 1f)]
    public float vignetteAmount = 0.8f;

    [Range(0.0f, 0.4f)]
    public float vignetteWidth = 0.1f;

    private const int numThreads = 24;

    private int xThreads = Mathf.CeilToInt(Screen.width / numThreads);
    private int yThreads = Mathf.CeilToInt(Screen.height / numThreads);

    private int lastWidth = Screen.width;
    private int lastHeight = Screen.height;

    // Start is called before the first frame update
    void Start()
    {
        blurrMat.SetTexture("_StarburstTex", starburstTex);
        createTextures();

        setTextures();

        setBloomUniforms();
        setLensFlareUniforms();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Screen.width != lastWidth ||Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            xThreads = Mathf.CeilToInt(Screen.width / numThreads);
            yThreads = Mathf.CeilToInt(Screen.height / numThreads);

            createTextures();
            setTextures();
        }

        if (skipAll)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, sourceTex);

        if (useBloom)
        {
            setBloomUniforms();

            bloom.Dispatch(0, xThreads, yThreads, 1);

            blurr(brightTex, bloomBlurrAmount);

            bloom.Dispatch(1, xThreads, yThreads, 1);
        }

        if (useCA)
        {

            blurrMat.SetFloat("_CAAmount", caAmount);
            Graphics.Blit(sourceTex, caResult, blurrMat, 3);

        } 
        else Graphics.Blit(sourceTex, caResult);

        if (useLensFlare)
        {
            setLensFlareUniforms();

            lensFlare.Dispatch(0, xThreads, yThreads, 1);

            blurr(lfResult, lfBlurrCount);

            lensFlare.Dispatch(1, xThreads, yThreads, 1);
        }
        if (useVignette)
        {
            blurrMat.SetFloat("_vignetteAmount", vignetteAmount);
            blurrMat.SetFloat("_vignetteWidth", vignetteWidth);
            Graphics.Blit(caResult, destination, blurrMat, 1);
        }
        else Graphics.Blit(caResult, destination);

    }

    private void blurr(RenderTexture tex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            blurrMat.SetInt("_horizontal", 1);
            Graphics.Blit(tex, blurrBuff, blurrMat, 0);
            blurrMat.SetInt("_horizontal", 0);
            Graphics.Blit(blurrBuff, tex, blurrMat, 0);
        }
    }

    private RenderTexture createTexture()
    {
        RenderTexture tex = new RenderTexture(Screen.width, Screen.height, 24);
        tex.enableRandomWrite = true;
        tex.Create();
        return tex;
    }

    private void createTextures()
    {
        sourceTex = createTexture();
        brightTex = createTexture();
        blurrBuff = createTexture();
        caResult = createTexture();
        lfResult = createTexture();
        lensDirt = createTexture();
        Graphics.Blit(lensDirtTex, lensDirt, blurrMat, 2);
    }

    private void setTextures()
    {
        setBloomTextures();
        setLensFlareTextures();
    }

    private void setBloomUniforms()
    {
        bloom.SetFloat("threshold", bloomThreshold);
    }

    private void setBloomTextures()
    {
        bloom.SetTexture(0, "Source", sourceTex);
        bloom.SetTexture(0, "BrightSpots", brightTex);

        bloom.SetTexture(1, "Source", sourceTex);
        bloom.SetTexture(1, "BrightSpots", brightTex);
    }

    private void setLensFlareUniforms()
    {
        lensFlare.SetInt("width", Screen.width);
        lensFlare.SetInt("height", Screen.height);
        lensFlare.SetInt("ghostCount", lfGhostCount);
        lensFlare.SetFloat("ghostSpacing", lfGhostSpacing);
        lensFlare.SetFloat("threshold", lfThreshold);
        lensFlare.SetFloat("caStrength", lfCAStrength);
    }

    private void setLensFlareTextures()
    {
        lensFlare.SetTexture(0, "Source", sourceTex);
        lensFlare.SetTexture(0, "Result", lfResult);
        lensFlare.SetTexture(1, "lensDirt", lensDirt);
        lensFlare.SetTexture(1, "Source", caResult);
        lensFlare.SetTexture(1, "Result", lfResult);
    }
}
