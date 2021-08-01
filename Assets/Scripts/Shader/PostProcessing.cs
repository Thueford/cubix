using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessing : MonoBehaviour
{
    public static PostProcessing self;

    [Header("Shader")]
    public ComputeShader bloom;
    public ComputeShader lensFlare;
    public Material postProcMat;

    [Header("Lens Textures")]
    public Texture2D lensDirtTex;
    public Texture2D starburstTex;

    [Header("General Settings")]
    public bool skipAll = false;

    [Header("Bloom")]
    public bool useBloom = true;
    [Range(1, 20)] public int bloomBlurrAmount = 4;
    [Range(0.01f, 1f)] public float bloomThreshold = .75f;

    [Header("Chromatic Aberration")]
    public bool useCA = true;
    [Range(0f, 0.01f)] public float caAmount = 0.0005f;

    [Header("Lens Flare")]
    public bool useLensFlare = true;
    [Range(1, 10)] public int lfGhostCount = 6;
    [Range(1, 20)] public int lfBlurrCount = 3;
    [Range(0.01f, 1f)] public float lfThreshold = 0.77f;
    [Range(0.1f, 2f)] public float lfGhostSpacing = .69f;
    [Range(0.0f, 0.3f)] public float lfCAStrength = 0.15f;

    [Header("CTR Monitor Effect")]
    public bool useCTREffect = true;
    [Range(0.0f, 1f)] public float vignetteAmount = 0.8f;
    [Range(0.0f, 0.4f)] public float vignetteWidth = 0.1f;

    [Header("Textures")]
    public RenderTexture sourceTex;
    public RenderTexture brightTex;
    public RenderTexture blurrBuff;
    public RenderTexture caResult;
    public RenderTexture lfResult;
    public RenderTexture lensDirt;

    private Light dirLight; 

    private const int threadsPerGroup = 24;

    private int xThreadGroups = Mathf.CeilToInt(Screen.width / threadsPerGroup);
    private int yThreadGroups = Mathf.CeilToInt(Screen.height / threadsPerGroup);

    private int lastWidth = Screen.width;
    private int lastHeight = Screen.height;

    private void Awake() => self = this;

    // Start is called before the first frame update
    void Start()
    {
        dirLight = GameObject.Find("Directional Light").GetComponent<Light>();
        if (!dirLight) Debug.LogError("No Directional Light found");

        postProcMat.SetTexture("_StarburstTex", starburstTex);

        createTextures();
        setTextures();

        setBloomUniforms();
        setLensFlareUniforms();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (skipAll)
        {
            Graphics.Blit(source, destination);
            return;
        }
            
        //Recreate Textures if Window was resized
        if (Screen.width != lastWidth ||Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            xThreadGroups = Mathf.CeilToInt(Screen.width / threadsPerGroup);
            yThreadGroups = Mathf.CeilToInt(Screen.height / threadsPerGroup);

            createTextures();
            setTextures();
        }

        Graphics.Blit(source, sourceTex);

        //Perform Bloom, Result: 'sourceTex'
        if (useBloom)
        {
            setBloomUniforms();

            bloom.Dispatch(0, xThreadGroups, yThreadGroups, 1);

            blurr(brightTex, bloomBlurrAmount);

            bloom.Dispatch(1, xThreadGroups, yThreadGroups, 1);
        }

        //Perform Chromatic Aberration, Result: 'caResult'
        if (useCA)
        {
            setCAUniforms();
            Graphics.Blit(sourceTex, caResult, postProcMat, 3);
        } 
        else Graphics.Blit(sourceTex, caResult);

        //Perform Lens Flare, Result: 'lfResult'
        if (useLensFlare)
        {
            setLensFlareUniforms();

            lensFlare.Dispatch(0, xThreadGroups, yThreadGroups, 1);
            blurr(lfResult, lfBlurrCount);
            lensFlare.Dispatch(1, xThreadGroups, yThreadGroups, 1);
        }

        //Perform CTR Effect, Result is 'destination'
        if (useCTREffect)
        {
            setCTRUniforms();
            Graphics.Blit(caResult, destination, postProcMat, 1);
        }

        else Graphics.Blit(caResult, destination);

    }

    private void blurr(RenderTexture tex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            postProcMat.SetInt("_horizontal", 1);
            Graphics.Blit(tex, blurrBuff, postProcMat, 0);
            postProcMat.SetInt("_horizontal", 0);
            Graphics.Blit(blurrBuff, tex, postProcMat, 0);
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
        Graphics.Blit(lensDirtTex, lensDirt, postProcMat, 2);
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

    private void setCAUniforms()
    {
        postProcMat.SetFloat("_CAAmount", caAmount);
    }

    private void setCTRUniforms()
    {
        postProcMat.SetFloat("_vignetteAmount", vignetteAmount);
        postProcMat.SetFloat("_vignetteWidth", vignetteWidth);
    }

    public void PlayerHitEffect(float duration)
    {
        if (GameState.save.config.computes)
        	StartCoroutine(EndPlayerHitEffect(duration));
    }

    private IEnumerator EndPlayerHitEffect(float duration)
    {
        const float changeInterval = 0.05f;
        float originalCAAmount = caAmount, 
            originalLightIntensity = dirLight.intensity;
        int count = (int)(duration / changeInterval);

        for (int i = 0; i < count; i++)
        {
            caAmount = Random.Range(0.001f, 0.01f);
            dirLight.intensity = Random.Range(0, 0.3f);
            yield return new WaitForSeconds(changeInterval);
        }
        caAmount = originalCAAmount;
        dirLight.intensity = originalLightIntensity;
        yield return true;
    }
}
