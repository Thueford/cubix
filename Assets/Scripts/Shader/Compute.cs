using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compute : MonoBehaviour
{
    public ComputeShader bloom, chromaticAberration, lensFlare;

    public Material blurrMat;

    public RenderTexture sourceTex,
        brightTex,
        blurrBuff,
        caResult,
        lfResult;

    private RenderTexture lensDirt;
    public Texture2D lensDirtTex;

    public bool skipAll = false;

    public bool useBloom = true;
    
    [Range(1, 20)]
    public int bloomBlurrAmount = 1;

    [Range(0.01f, 1f)]
    public float bloomThreshold = .65f;

    public bool useCA = true;

    [Range(1, 10)]
    public int caAmount = 3;

    public bool useLensFlare = true;

    [Range(1, 10)]
    public int lfGhostCount = 3;

    [Range(1, 20)]
    public int lfBlurrCount = 1;

    [Range(0.01f, 1f)]
    public float lfThreshold = 0.7f;

    [Range(0.1f, 2f)]
    public float lfGhostSpacing = 1.4f;

    [Range(0.0f, 0.2f)]
    public float lfCAStrength = 0.1f;

    public bool useVignette = true;

    [Range(0.0f, 1f)]
    public float vignetteAmount = 0.8f;

    [Range(0.0f, 0.4f)]
    public float vignetteWidth = 0.1f;

    private const int numThreads = 24;

    private int xThreads = (int)Mathf.Ceil(Screen.width / numThreads);
    private int yThreads = (int)Mathf.Ceil(Screen.height / numThreads);

    private int lastWidth = Screen.width;
    private int lastHeight = Screen.height;

    // Start is called before the first frame update
    void Start()
    {
        createTextures();

        setTextures();

        setBloomUniforms();
        setCAUniforms();
        setLensFlareUniforms();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Screen.width != lastWidth ||Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            xThreads = (int)Mathf.Ceil(Screen.width / numThreads);
            yThreads = (int)Mathf.Ceil(Screen.height / numThreads);

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
            setCAUniforms();

            Graphics.Blit(sourceTex, caResult);

            chromaticAberration.Dispatch(0, xThreads, yThreads, 1);
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
        Graphics.Blit(lensDirtTex, lensDirt);
    }

    private void setTextures()
    {
        setBloomTextures();
        setCATextures();
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

    private void setCAUniforms()
    {
        chromaticAberration.SetInt("amount", caAmount);
    }

    private void setCATextures()
    {
        chromaticAberration.SetTexture(0, "Source", sourceTex);
        chromaticAberration.SetTexture(0, "Result", caResult);
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
