using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


public class RDOnGPU : MonoBehaviour
{
    private RenderTexture rDCells;
    private RenderTexture rDCellsTemp;

    public ComputeShader computeRD;

    public int width = 800;
    public int height = 800;

    [Range(0.5f, 1.5f)]
    public float dA = 1f;
    [Range(0f, 1f)]
    public float dB = 0.5f;
    [Range(0.04f, 0.06f)]
    public float f = 0.055f;
    [Range(0.04f, 0.08f)]
    public float k = 0.062f;

    private RenderTexture renderTexture;

    private FilterMode filterMode = FilterMode.Bilinear;
    private GraphicsFormat graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

    private void Start()
    {
        CreateRenderTexture(ref renderTexture, width, height, filterMode, graphicsFormat);
        CreateRenderTexture(ref rDCells, width, height, filterMode, graphicsFormat);
        CreateRenderTexture(ref rDCellsTemp, width, height, filterMode, graphicsFormat);
        InitCells();
        computeRD.SetTexture(0, "Result", renderTexture);
        computeRD.SetTexture(0, "rDCells", rDCells);
        computeRD.SetTexture(0, "rDCellsTemp", rDCellsTemp);
    }

    private void InitCells()
    {
        Texture2D rdCellsTexture = new Texture2D(width, height);

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                rdCellsTexture.SetPixel(w, h, new Color(1f, 0f, 0f));
            }
        }

        InitStartState(ref rdCellsTexture);
        rdCellsTexture.Apply();
        Graphics.Blit(rdCellsTexture, rDCells);
    }

    private void InitStartState(ref Texture2D initTexture)
    {
        initTexture.SetPixel(100, 100, new Color(0f, 1f, 0f));
        initTexture.SetPixel(101, 100, new Color(0f, 1f, 0f));
        initTexture.SetPixel(100, 101, new Color(0f, 1f, 0f));
        initTexture.SetPixel(101, 101, new Color(0f, 1f, 0f));

        initTexture.SetPixel(110, 110, new Color(0f, 1f, 0f));
        initTexture.SetPixel(111, 110, new Color(0f, 1f, 0f));
        initTexture.SetPixel(110, 111, new Color(0f, 1f, 0f));
        initTexture.SetPixel(111, 111, new Color(0f, 1f, 0f));

        initTexture.SetPixel(10, 10, new Color(0f, 1f, 0f));
        initTexture.SetPixel(11, 10, new Color(0f, 1f, 0f));
        initTexture.SetPixel(10, 11, new Color(0f, 1f, 0f));
        initTexture.SetPixel(11, 11, new Color(0f, 1f, 0f));

        initTexture.SetPixel(60, 60, new Color(0f, 1f, 0f));
        initTexture.SetPixel(61, 60, new Color(0f, 1f, 0f));
        initTexture.SetPixel(60, 61, new Color(0f, 1f, 0f));
        initTexture.SetPixel(61, 61, new Color(0f, 1f, 0f));
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(rDCells, rDCellsTemp);
        computeRD.SetFloat("dA", dA);
        computeRD.SetFloat("dB", dB);
        computeRD.SetFloat("f", f);
        computeRD.SetFloat("k", k);

        Dispatch(computeRD, width, height);
        Graphics.Blit(renderTexture, dest);
    }

    public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode, GraphicsFormat format)
    {
        if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }
            texture = new RenderTexture(width, height, 0);
            texture.graphicsFormat = format;
            texture.enableRandomWrite = true;

            texture.autoGenerateMips = false;
            texture.Create();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;
    }

    private void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
    {
        Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    private Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
}
