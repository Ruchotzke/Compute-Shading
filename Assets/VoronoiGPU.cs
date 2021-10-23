using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiGPU : MonoBehaviour
{
    [Header("Image Settings")]
    public RawImage DisplayImage;
    public float ResolutionScale;

    [Header("GPU Settings")]
    public ComputeShader Shader;

    [Header("Voronoi Settings")]
    public int NumPoints = 10;

    [Header("Animation")]
    public float MoveSpeed = 10.0f;

    [Header("Graphical Settings")]
    public float PointSize = 5.0f;
    public float RegionDistance = 300.0f;
    public float DistanceExponent = 2.0f;

    Vector2Int TexSize;
    RenderTexture Tex;
    List<Vector2> points = new List<Vector2>();
    List<Vector2> directions = new List<Vector2>();
    List<Color> colors = new List<Color>();

    private void Start()
    {
        /* First generate a texture for the image */
        TexSize = new Vector2Int((int)(Screen.width * ResolutionScale), (int)(Screen.height * ResolutionScale));
        Tex = new RenderTexture(TexSize.x, TexSize.y, 32);
        Tex.filterMode = FilterMode.Point;
        Tex.wrapMode = TextureWrapMode.Clamp;
        Tex.enableRandomWrite = true;

        /* Set the iamge texture */
        DisplayImage.texture = Tex;

        /* Generate a list of points and generate a voronoi texture */
        for(int i = 0; i < NumPoints; i++)
        {
            points.Add(new Vector2(Random.Range(0, TexSize.x), Random.Range(0, TexSize.y)));
            directions.Add(new Vector2(Mathf.Cos(Random.value * Mathf.PI * 2), Mathf.Sin(Random.value * Mathf.PI * 2)));
            colors.Add(Color.HSVToRGB(Mathf.Lerp(0.0f, 1.0f, (float)i / NumPoints), 1.0f, 1.0f));
        }
        GenerateVoronoi(points, true);

    }

    private void Update()
    {
        /* Move the points */
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 newPos = points[i] + directions[i] * MoveSpeed * Time.deltaTime;

            /* If we are hitting the wall, move to the wall, stop, and reflect */
            int changeCount = 0;
            if (newPos.x < 0)
            {
                newPos.x = 0;
                changeCount += 1;
                directions[i] = Vector2.Reflect(directions[i], Vector2.right);
            }
            else if(newPos.x >= TexSize.x)
            {
                newPos.x = TexSize.x - 1;
                changeCount += 1;
                directions[i] = Vector2.Reflect(directions[i], Vector2.left);
            }

            if (newPos.y < 0)
            {
                newPos.y = 0;
                changeCount += 1;
                directions[i] = Vector2.Reflect(directions[i], Vector2.up);
            }
            else if(newPos.y >= TexSize.y)
            {
                newPos.y = TexSize.y - 1;
                changeCount += 1;
                directions[i] = Vector2.Reflect(directions[i], Vector2.down);
            }

            if(changeCount > 1) { directions[i] = (TexSize / 2 - points[i]).normalized; }

            /* move the point */
            if (changeCount == 0) points[i] = newPos;
        }

        /* Update the texture */
        GenerateVoronoi(points, true);
    }

    public void GenerateVoronoi(List<Vector2> points, bool drawPoints)
    {
        /* Get the kernel id */
        int kernelID = Shader.FindKernel("GenerateVoronoi");

        /* Generate buffers for agent data */
        ComputeBuffer PositionBuffer = new ComputeBuffer(NumPoints, 2 * sizeof(float));
        ComputeBuffer ColorBuffer = new ComputeBuffer(NumPoints, 4 * sizeof(float));

        PositionBuffer.SetData(points);
        ColorBuffer.SetData(colors);

        /* Bind Data to the GPU */
        Shader.SetInt("ImageX", TexSize.x);
        Shader.SetInt("ImageY", TexSize.y);
        Shader.SetInt("NumPoints", NumPoints);
        Shader.SetFloat("NormalizedDistance", RegionDistance);
        Shader.SetFloat("PointSize", PointSize);
        Shader.SetFloat("DistanceExp", DistanceExponent);
        Shader.SetBuffer(kernelID, "NodePositions", PositionBuffer);
        Shader.SetBuffer(kernelID, "NodeColors", ColorBuffer);
        Shader.SetTexture(kernelID, "OutputTexture", Tex);

        /* Perform the operation */
        Shader.GetKernelThreadGroupSizes(kernelID, out uint kx, out uint ky, out _);
        Shader.Dispatch(kernelID, Mathf.CeilToInt(TexSize.x / kx), Mathf.CeilToInt(TexSize.y / ky), 1);

        /* Dispose of the buffers - we are done */
        ColorBuffer.Dispose();
        PositionBuffer.Dispose();
    }
}
