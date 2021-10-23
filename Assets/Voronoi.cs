using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Voronoi : MonoBehaviour
{
    [Header("Image Settings")]
    public RawImage DisplayImage;
    public float ResolutionScale;

    [Header("Voronoi Settings")]
    public int NumPoints = 10;

    [Header("Animation")]
    public float MoveSpeed = 10.0f;

    Vector2Int TexSize;
    Texture2D Tex;
    List<Vector2> points = new List<Vector2>();
    List<Vector2> directions = new List<Vector2>();

    private void Start()
    {
        /* First generate a texture for the image */
        TexSize = new Vector2Int((int)(Screen.width * ResolutionScale), (int)(Screen.height * ResolutionScale));
        Tex = new Texture2D(TexSize.x, TexSize.y);
        Tex.filterMode = FilterMode.Point;
        Tex.wrapMode = TextureWrapMode.Clamp;

        /* Black the texture */
        for (int y = 0; y < TexSize.y; y++)
        {
            for (int x = 0; x < TexSize.x; x++)
            {
                Tex.SetPixel(x, y, Color.black);
            }
        }
        Tex.Apply();

        /* Set the iamge texture */
        DisplayImage.texture = Tex;

        /* Generate a list of points and generate a voronoi texture */
        for(int i = 0; i < NumPoints; i++)
        {
            points.Add(new Vector2(Random.Range(0, TexSize.x), Random.Range(0, TexSize.y)));
            directions.Add(new Vector2(Mathf.Cos(Random.value * Mathf.PI * 2), Mathf.Sin(Random.value * Mathf.PI * 2)));
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
        /* Step 1: Generate a dictionary for points to colors */
        Dictionary<Vector2, Color> regions = new Dictionary<Vector2, Color>();
        for(int i = 0; i < points.Count; i++)
        {
            regions.Add(points[i], Color.HSVToRGB(Mathf.Lerp(0.0f, 1.0f, (float)i / points.Count), 1.0f, 1.0f));
        }

        /* Step 2: Color each pixel in the image according to its voronoi region */
        for (int y = 0; y < TexSize.y; y++)
        {
            for (int x = 0; x < TexSize.x; x++)
            {
                /* Find the closest point */
                Vector2Int currPoint = new Vector2Int(x, y);
                float minDistance = Vector2.Distance(points[0], currPoint);
                Vector2 minPoint = points[0];
                for(int i = 1; i < points.Count; i++)
                {
                    float distance = Vector2.Distance(points[i], currPoint);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        minPoint = points[i];
                    }
                }

                /* Apply a color to this pixel */
                Tex.SetPixel(x, y, regions[minPoint]);
            }
        }

        /* Step 3: If we draw points, color the corresponding pixels black */
        if (drawPoints)
        {
            foreach(var point in points)
            {
                for (int x = -2; x < 3; x++)
                {
                    for (int y = -2; y < 3; y++)
                    {
                        Vector2Int pos = Vector2Int.FloorToInt(point) + new Vector2Int(x, y);
                        if (pos.x < 0 || pos.y < 0 || pos.x >= TexSize.x || pos.y >= TexSize.y) continue;
                        Tex.SetPixel(pos.x, pos.y, Color.black);
                    }
                }
                
            }
        }

        /* Apply the voronoi texture */
        Tex.Apply();
    }
}
