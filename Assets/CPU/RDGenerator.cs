using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RDGenerator : MonoBehaviour
{
    struct RDCell
    {
        public float a;
        public float b;
    }

    public int width = 512;
    public int height = 512;

    [Range(0.5f, 1.5f)]
    public float dA = 1f;
    [Range(0f, 1f)]
    public float dB = 0.5f;
    [Range(0.04f, 0.06f)]
    public float f = 0.055f;
    [Range(0.04f, 0.08f)]
    public float k = 0.062f;

    private Texture2D texture;

    private RDCell[,] rDCells;
    private RDCell[,] tempCells;

    private void Start()
    {
        texture = new Texture2D(width, height);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        InitCells();
        InitTexture();
        InitStartState();
    }

    private void Update()
    {
        UpdateState();
        UpdateTexture();
    }

    private void UpdateState()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                tempCells[w, h].a = rDCells[w, h].a;
                tempCells[w, h].b = rDCells[w, h].b;
            }
        }

        for (int w = 1; w < width - 1; w++)
        {
            for (int h = 1; h < height - 1; h++)
            {
                float a = tempCells[w, h].a;
                float b = tempCells[w, h].b;

                rDCells[w, h].a = a + (dA * LaplaceA(w, h) - (a * b * b) + (f * (1f - a)));
                rDCells[w, h].b = b + (dB * LaplaceB(w, h) + (a * b * b) - ((k + f) * b));
            }
        }
    }

    private float LaplaceA(int w, int h)
    {
        float sum = 0f;

        sum += tempCells[w, h].a * -1f;

        sum += tempCells[w + 1, h].a * 0.2f;
        sum += tempCells[w, h + 1].a * 0.2f;
        sum += tempCells[w - 1, h].a * 0.2f;
        sum += tempCells[w, h - 1].a * 0.2f;

        sum += tempCells[w + 1, h + 1].a * 0.05f;
        sum += tempCells[w - 1, h + 1].a * 0.05f;
        sum += tempCells[w + 1, h - 1].a * 0.05f;
        sum += tempCells[w - 1, h - 1].a * 0.05f;

        return sum;
    }

    private float LaplaceB(int w, int h)
    {
        float sum = 0f;

        sum += tempCells[w, h].b * -1f;

        sum += tempCells[w + 1, h].b * 0.2f;
        sum += tempCells[w, h + 1].b * 0.2f;
        sum += tempCells[w - 1, h].b * 0.2f;
        sum += tempCells[w, h - 1].b * 0.2f;

        sum += tempCells[w + 1, h + 1].b * 0.05f;
        sum += tempCells[w - 1, h + 1].b * 0.05f;
        sum += tempCells[w + 1, h - 1].b * 0.05f;
        sum += tempCells[w - 1, h - 1].b * 0.05f;

        return sum;
    }

    private void InitStartState()
    {
        rDCells[100, 100].b = 1f;
        rDCells[101, 100].b = 1f;
        rDCells[100, 101].b = 1f;
        rDCells[101, 101].b = 1f;

        rDCells[110, 110].b = 1f;
        rDCells[111, 110].b = 1f;
        rDCells[110, 111].b = 1f;
        rDCells[111, 111].b = 1f;

        rDCells[10, 10].b = 1f;
        rDCells[11, 10].b = 1f;
        rDCells[10, 11].b = 1f;
        rDCells[11, 11].b = 1f;

        rDCells[60, 60].b = 1f;
        rDCells[61, 60].b = 1f;
        rDCells[60, 61].b = 1f;
        rDCells[61, 61].b = 1f;
    }

    private void UpdateTexture()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                texture.SetPixel(w, h, Color.Lerp(Color.white, Color.black, rDCells[w, h].b * 3f));
            }
        }

        texture.Apply();
    }

    private void InitCells()
    {
        rDCells = new RDCell[width, height];
        tempCells = new RDCell[width, height];

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                rDCells[w, h].a = 1f;
                rDCells[w, h].b = 0f;
                tempCells[w, h].a = 1f;
                tempCells[w, h].b = 0f;
            }
        }
    }

    private void InitTexture()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                texture.SetPixel(w, h, Color.white);
            }
        }

        texture.Apply();
    }
}
