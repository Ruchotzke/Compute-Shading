using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentsCPU : MonoBehaviour
{
    [Header("Image Settings")]
    public RawImage DisplayImage;
    public float ResolutionScale;

    [Header("Agent Settings")]
    public int NumAgents = 6;
    public float AgentSpeed;

    [Header("Graphics Settings")]
    public float EvaporationSpeed = 1.0f;

    private Vector2Int Size;

    Texture2D texture;

    private List<Agent> agents = new List<Agent>();

    private void Start()
    {
        Size = new Vector2Int((int)(Screen.width * ResolutionScale), (int)(Screen.height * ResolutionScale));
        texture = new Texture2D(Size.x, Size.y);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        /* Black the texture */
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();

        DisplayImage.texture = texture;

        for(int i = 0; i < NumAgents; i++)
        {
            agents.Add(new Agent(Size / 2, 0.18f + Mathf.Lerp(0f, Mathf.PI * 2, (float)i / NumAgents)));
        }
    }

    private void Update()
    {
        /* Update agent positions */
        foreach(var agent in agents)
        {
            var direction = new Vector2(Mathf.Cos(agent.angle), Mathf.Sin(agent.angle));
            Vector2 newPosition = agent.position + direction * AgentSpeed * Time.deltaTime;

            /* If the new position would be off screen, flip the direction back inside */
            if (newPosition.x <= 0 || newPosition.x >= Size.x - 0.1f || newPosition.y <= 0 || newPosition.y > Size.y - 0.1f)
            {
                newPosition = new Vector2(Mathf.Clamp(newPosition.x, 0, Size.x - 0.01f), Mathf.Clamp(newPosition.y, 0, Size.y - 0.01f));

                /* Update the angle */
                int numUpdates = 0;
                if(newPosition.x <= 0)
                {
                    direction = Vector2.Reflect(direction, Vector2.right);
                    agent.angle = Mathf.Atan2(direction.y, direction.x);
                    numUpdates++;
                }
                else if(newPosition.x >= Size.x - 0.1f)
                {
                    direction = Vector2.Reflect(direction, Vector2.left);
                    agent.angle = Mathf.Atan2(direction.y, direction.x);
                    numUpdates++;
                }

                if (newPosition.y <= 0)
                {
                    direction = Vector2.Reflect(direction, Vector2.up);
                    agent.angle = Mathf.Atan2(direction.y, direction.x);
                    numUpdates++;
                }
                else if (newPosition.y >= Size.y - 0.1f)
                {
                    direction = Vector2.Reflect(direction, Vector2.down);
                    agent.angle = Mathf.Atan2(direction.y, direction.x);
                    numUpdates++;
                }

                /* Special case - we hit a corner */
                if(numUpdates > 1)
                {
                    direction = ((Size / 2) - agent.position).normalized;
                    agent.angle = Mathf.Atan2(direction.y, direction.x);
                }

                if(numUpdates > 0)
                {
                    newPosition = agent.position + direction * AgentSpeed * Time.deltaTime;
                }
            }

            /* Apply the new movement */
            agent.position = newPosition;
        }

        /* Update texture */
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        /* Dim all pixels slightly */
        Color[] data = new Color[Size.x * Size.y];
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Color curr = texture.GetPixel(x, y);

                var gray = Mathf.Max(0f, curr.grayscale - EvaporationSpeed * Time.deltaTime);
                curr = new Color(gray, gray, gray);

                texture.SetPixel(x, y, curr);

                data[x + Size.x * y] = curr;
            }
        }

        /* Blur the image */
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                int totalPixels = 0;
                Vector3 total = Vector3.zero;
                for (int xx = -1; xx < 2; xx++)
                {
                    for (int yy = -1; yy < 2; yy++)
                    {
                        /* First do a bounds check to make sure we are in bounds */
                        if (x + xx < 0 || x + xx >= Size.x || y + yy < 0 || y + yy >= Size.y) continue;

                        total.x += data[x + xx + Size.x * (y + yy)].r;
                        total.y += data[x + xx + Size.x * (y + yy)].g;
                        total.z += data[x + xx + Size.x * (y + yy)].b;
                        totalPixels += 1;
                    }
                }

                /* Average complete. Store new pixel */
                total /= totalPixels;
                texture.SetPixel(x, y, new Color(total.x, total.y, total.z));
            }
        }

        /* Add to brightness for each agent */
        foreach (var agent in agents)
        {
            texture.SetPixel(Mathf.RoundToInt(agent.position.x), Mathf.RoundToInt(agent.position.y), Color.white);
        }


        /* Finally, apply the changes */
        texture.Apply();
    }

    class Agent
    {
        public Vector2 position;
        public float angle;

        public Agent(Vector2 Position, float Angle)
        {
            this.position = Position;
            this.angle = Angle;
        }
    }
}
