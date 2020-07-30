using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class test_combine : MonoBehaviour {
    public RenderTexture _combinedLutTexture;
    RenderTexture _combinedLutTexture_unfilter;
    RenderTexture save_texture;

    ComputeShader CS_CombinedLut;

    public Color ColorSaturation = Color.white;
    public Color ColorContrast = Color.white;
    public Color ColorGamma = Color.white;
    public Color ColorGain = Color.white;
    public Color ColorOffset = new Color(0, 0 ,0 ,0);


    public Color ColorSaturationShadows = Color.white;
    public Color ColorContrastShadows = Color.white;
    public Color ColorGammaShadows = Color.white;
    public Color ColorGainShadows = Color.white;
    public Color ColorOffsetShadows = new Color(0, 0, 0, 0);


    public Color ColorSaturationMidtones = Color.white;
    public Color ColorContrastMidtones = Color.white;
    public Color ColorGammaMidtones = Color.white;
    public Color ColorGainMidtones = Color.white;
    public Color ColorOffsetMidtones = new Color(0, 0, 0, 0);


    public Color ColorSaturationHighlights = Color.white;
    public Color ColorContrastHighlights = Color.white;
    public Color ColorGammaHighlights = Color.white;
    public Color ColorGainHighlights = Color.white;
    public Color ColorOffsetHighlights = new Color(0, 0, 0, 0);

    public float ColorCorrectionShadowsMax = 0.09f;
    public float ColorCorrectionHighlightsMin = 0.5f;
    public float BlueCorrection = 0.6f;
    public float ExpandGamut = 1.0f;


   public float TonemapperSlope = 0.88f;
   public float TonemapperToe = 0.55f;
   public float TonemapperShoulder = 0.26f;
   public float TonemapperBlackClip = 0.0f;
   public float TonemapperWhiteClip = 0.04f;

   public float WhiteBalanceTemp = 6500.0f;
   public float WhiteBalanceTint = 0.0f;

    private int tex1Res = 32;

    private Material _debug_mtl;
    private Material _debug_tex;

    private void SetColorForCS(string name, Color col)
    {
        CS_CombinedLut.SetVector(name, col);
    }

    private void OnEnable()
    {
        CS_CombinedLut = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scenes/CombineLut.compute");
        Debug.Log(string.Format("get CS progress {0}", CS_CombinedLut));

        //Create 3D Render Texture 1
        
        _combinedLutTexture = new RenderTexture(tex1Res, tex1Res, 0);
        _combinedLutTexture.format = RenderTextureFormat.ARGB2101010;
        _combinedLutTexture.enableRandomWrite = true;
        _combinedLutTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        _combinedLutTexture.volumeDepth = tex1Res;
        _combinedLutTexture.Create();


        //_combinedLutTexture = create_3d_tex(tex1Res, "Assets/Scenes/output_3d.assets", TextureFormat.RGBAFloat, Color.black);
        _debug_mtl = GameObject.Find("Plane").GetComponent<MeshRenderer>().sharedMaterial;
        _debug_tex = GameObject.Find("show_plane").GetComponent<MeshRenderer>().sharedMaterial;
    }

    void test_3d_tex()
    {
        //copy one use point filter\
        _combinedLutTexture_unfilter = new RenderTexture(_combinedLutTexture);
        _combinedLutTexture_unfilter.filterMode = FilterMode.Point;
        Graphics.CopyTexture(_combinedLutTexture, _combinedLutTexture_unfilter);
        

        _debug_mtl.SetTexture("_CombinedLutTex", _combinedLutTexture);
        _debug_tex.SetTexture("_Volume", _combinedLutTexture_unfilter);
    }
    private void ComputeLutTexture()
    {
        int noise1Gen = CS_CombinedLut.FindKernel("cs_main");

        SetColorForCS("ColorSaturation", ColorSaturation);
        SetColorForCS("ColorContrast", ColorContrast);
        SetColorForCS("ColorGamma", ColorGamma);
        SetColorForCS("ColorGain", ColorGain);
        SetColorForCS("ColorOffset", ColorOffset);

        SetColorForCS("ColorSaturationShadows", ColorSaturationShadows);
        SetColorForCS("ColorContrastShadows", ColorContrastShadows);
        SetColorForCS("ColorGammaShadows", ColorGammaShadows);
        SetColorForCS("ColorGainShadows", ColorGainShadows);
        SetColorForCS("ColorOffsetShadows", ColorOffsetShadows);

        SetColorForCS("ColorSaturationMidtones", ColorSaturationMidtones);
        SetColorForCS("ColorContrastMidtones", ColorContrastMidtones);
        SetColorForCS("ColorGammaMidtones", ColorGammaMidtones);
        SetColorForCS("ColorGainMidtones", ColorGainMidtones);
        SetColorForCS("ColorOffsetMidtones", ColorOffsetMidtones);

        SetColorForCS("ColorSaturationHighlights", ColorSaturationHighlights);
        SetColorForCS("ColorContrastHighlights", ColorContrastHighlights);
        SetColorForCS("ColorGammaHighlights", ColorGammaHighlights);
        SetColorForCS("ColorGainHighlights", ColorGainHighlights);
        SetColorForCS("ColorOffsetHighlights", ColorOffsetHighlights);

        CS_CombinedLut.SetFloat("ColorCorrectionShadowsMax", ColorCorrectionShadowsMax);
        CS_CombinedLut.SetFloat("ColorCorrectionHighlightsMin", ColorCorrectionHighlightsMin);
        CS_CombinedLut.SetFloat("BlueCorrection", BlueCorrection);
        CS_CombinedLut.SetFloat("ExpandGamut", ExpandGamut);

        Vector3 ColorTransform = new Vector3(0.0f, 0.5f, 1);
        {
            // x is the input value, y the output value
            // RGB = a, b, c where y = a * x*x + b * x + c

            float c = ColorTransform.x;
            float b = 4 * ColorTransform.y - 3 * ColorTransform.x - ColorTransform.z;
            float a = ColorTransform.z - ColorTransform.x - b;

            float[] v4MappingPolynomial = new float[4];
            v4MappingPolynomial[0] = a;
            v4MappingPolynomial[1] = b;
            v4MappingPolynomial[2] = c;
            v4MappingPolynomial[3] = 1;

            CS_CombinedLut.SetFloats("MappingPolynomial", v4MappingPolynomial);
            //_mat.SetVector("MappingPolynomial", new Vector4(a, b, c, 1)); 
        }

        //_mat.SetVector ("ColorScale", new Vector3(1.0f,1.0f,1.0f));
        //_mat.SetColor ("OverlayColor", new Color(0, 0, 0, 0));

        float[] v3ColorScale = new float[3];
        v3ColorScale[0] = 1.0f;
        v3ColorScale[1] = 1.0f;
        v3ColorScale[2] = 1.0f;

        CS_CombinedLut.SetFloats("ColorScale", v3ColorScale);

        SetColorForCS("OverlayColor", new Color(0, 0, 0, 0));

        CS_CombinedLut.SetFloats("FilmSlope", TonemapperSlope);
        CS_CombinedLut.SetFloats("FilmToe", TonemapperToe);
        CS_CombinedLut.SetFloats("FilmShoulder", TonemapperShoulder);
        CS_CombinedLut.SetFloats("FilmBlackClip", TonemapperBlackClip);
        CS_CombinedLut.SetFloats("FilmWhiteClip", TonemapperWhiteClip);

        CS_CombinedLut.SetFloats("WhiteTemp", WhiteBalanceTemp);
        CS_CombinedLut.SetFloats("WhiteTint", WhiteBalanceTint);

        ///
        /*
        static TConsoleVariableData<int32>* CVarOutputDevice = IConsoleManager::Get().FindTConsoleVariableDataInt(TEXT("r.HDR.Display.OutputDevice"));
        static TConsoleVariableData<float>* CVarOutputGamma = IConsoleManager::Get().FindTConsoleVariableDataFloat(TEXT("r.TonemapperGamma"));
        static TConsoleVariableData<int32>* CVarOutputGamut = IConsoleManager::Get().FindTConsoleVariableDataInt(TEXT("r.HDR.Display.ColorGamut"));

        int32 OutputDeviceValue = CVarOutputDevice->GetValueOnRenderThread();
        float Gamma = CVarOutputGamma->GetValueOnRenderThread();
        */
        float DisplayGamma = 2.2f;
        float Gamma = 0.0f;
        {
            float[] v3InverseGamma = new float[3];
            v3InverseGamma[0] = 1.0f / DisplayGamma;
            v3InverseGamma[1] = 2.2f / DisplayGamma;
            v3InverseGamma[2] = 1.0f / Mathf.Max(Gamma, 1.0f);

            CS_CombinedLut.SetFloats("InverseGamma", v3InverseGamma);
        }

        int OutputDeviceValue = 0;
        int OutputGamutValue = 0;
        CS_CombinedLut.SetInt("OutputDevice", OutputDeviceValue);
        CS_CombinedLut.SetInt("OutputGamut", OutputGamutValue);

        float[] ColorShadowTint2 = new float[4];
        ColorShadowTint2[0] = 0.0f;
        ColorShadowTint2[1] = 0.0f;
        ColorShadowTint2[2] = 0.0f;
        ColorShadowTint2[3] = 1.0f;
        CS_CombinedLut.SetFloats("ColorShadow_Tint2", ColorShadowTint2);


        //////////////////////////////////////////////////////////////////////////////////////
        CS_CombinedLut.SetTexture(noise1Gen, "RWOutComputeTex", _combinedLutTexture);
        CS_CombinedLut.Dispatch(noise1Gen, tex1Res / 8, tex1Res / 8, tex1Res / 8);

        //Debug.Log("CS Compute Process OK!");

        test_3d_tex();
    }

    // Use this for initialization
    void Start () {
		
	}

    // ---------------------------
    Vector3 saturate(Vector3 a)
    {
        Vector3 output;
        output.x = Mathf.Clamp(a.x, 0, 1);
        output.y = Mathf.Clamp(a.y, 0, 1);
        output.z = Mathf.Clamp(a.z, 0, 1);
        return output;
    }

    Vector3 num2v3(float a)
    {
        return new Vector3(a, a, a);
    }

    float LogToLin1Tmp(float x)
    {
        const float LinearRange = 14;
        const float LinearGrey = 0.18f;
        const float ExposureGrey = 444;

        float LinearColor = Mathf.Pow(2, (x - ExposureGrey / 1023.0f) * LinearRange) * LinearGrey;
        return LinearColor;
    }

    Vector3 LogToLin1(Vector3 LogColor)
    {
        return new Vector3(LogToLin1Tmp(LogColor.x), LogToLin1Tmp(LogColor.y), LogToLin1Tmp(LogColor.z));
    }

    Texture3D create_3d_tex(int tex_size, string save_path, TextureFormat fmt, Color default_color)
    {
        Texture3D tex = new Texture3D(tex_size, tex_size, tex_size, fmt, false);
        
        Debug.Log(string.Format("Get tex create!!!! {0}", tex));
        Color[] initState = new Color[tex_size * tex_size * tex_size];
        for (int i = 0; i < tex_size; i++)
        {
            for (int j = 0; j < tex_size; j++)
            {
                for (int k = 0; k < tex_size; k++)
                {
                    var index = k + j * tex_size + i * tex_size * tex_size;
                    //initState[index] = new Color(i * 1.0f / tex_size, j * 1.0f / tex_size, k * 1.0f / tex_size, 1.0f);
                    //initState[index] = default_color;
                    Vector3 output = LogToLin1(new Vector3(i, j, k));
                    initState[index] = new Color(output.x, output.y, output.z, 1.0f);
                }
            }
        }
        //public void SetPixelData(T[] data, int mipLevel, int sourceDataStartIndex); This is useful if you want to load compressed or other non-color texture format data into a texture.
        tex.SetPixels(initState);
        tex.Apply();

        AssetDatabase.CreateAsset(tex, save_path);
        AssetDatabase.Refresh();

        return tex;
    }

    void create_tex_test()
    {
        save_texture = new RenderTexture(_combinedLutTexture);
        Graphics.CopyTexture(_combinedLutTexture, save_texture);
        AssetDatabase.CreateAsset(save_texture, "Assets/Scenes/_combinedLutTexture.asset");
    }

    // Update is called once per frame
    void Update () {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ComputeLutTexture();
            create_tex_test();
        }
        */

        ComputeLutTexture();
    }
}
