using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentsGPU : MonoBehaviour
{
    [Header("Image Settings")]
    public RawImage DisplayImage;
    public float ResolutionScale;

    [Header("Shader Settings")]
    public ComputeShader Shader;

    [Header("Agent Settings")]
    public int NumAgents = 6;
    public float AgentSpeed;

    [Header("Graphics Settings")]
    public float EvaporationSpeed = 1.0f;

    private Vector2Int Size;

    RenderTexture texture_1;
    RenderTexture texture_2;

    ComputeBuffer AgentPositions;
    ComputeBuffer AgentAngles;

    Vector2[] agentPositions;
    float[] agentAngles;

    private void Start()
    {
        Size = new Vector2Int((int)(Screen.width * ResolutionScale), (int)(Screen.height * ResolutionScale));

        /* Generate two textures */
        texture_1 = new RenderTexture(Size.x, Size.y, 16);
        texture_1.filterMode = FilterMode.Point;
        texture_1.wrapMode = TextureWrapMode.Clamp;
        texture_1.enableRandomWrite = true;

        texture_2 = new RenderTexture(Size.x, Size.y, 16);
        texture_2.filterMode = FilterMode.Point;
        texture_2.wrapMode = TextureWrapMode.Clamp;
        texture_2.enableRandomWrite = true;

        DisplayImage.texture = texture_1;


        /* Generate agents */
        AgentPositions = new ComputeBuffer(NumAgents, 2 * sizeof(float));
        AgentAngles = new ComputeBuffer(NumAgents, sizeof(float));

        agentPositions = new Vector2[NumAgents];
        agentAngles = new float[NumAgents];

        for (int i = 0; i < NumAgents; i++)
        {
            agentPositions[i] = Size / 2;
            //agentPositions[i] = new Vector2(Random.Range(0.2f, 0.8f) * Size.x, Random.Range(0.2f, 0.8f) * Size.y);
            agentAngles[i] = 0.17f + Mathf.Lerp(0f, Mathf.PI * 2, (float)i / NumAgents); //add offset for no boring paths
            //agentAngles[i] = Random.Range(0f, Mathf.PI * 2);
        }

        AgentPositions.SetData(agentPositions);
        AgentAngles.SetData(agentAngles);

        /* Buffer initial data which will not change from frame to frame */
        BufferConstantData();
    }

    private void Update()
    {
        /* Update agent positions */
        UpdateAgents();

        /* Update texture */
        UpdateTexture();
    }

    private void BufferConstantData()
    {
        /* General Data */
        Shader.SetInt("ImageX", Size.x);
        Shader.SetInt("ImageY", Size.y);
        Shader.SetInt("NumAgents", NumAgents);
        Shader.SetFloat("AgentSpeed", AgentSpeed);
        Shader.SetFloat("EvaporationSpeed", EvaporationSpeed);

        /* Update Agents Kernel */
        int handle = Shader.FindKernel("UpdateAgents");
        Shader.SetBuffer(handle, "AgentPositions", AgentPositions);
        Shader.SetBuffer(handle, "AgentDirections", AgentAngles);

        /* Update Texture Kernel */
        handle = Shader.FindKernel("UpdateTexture");
        Shader.SetBuffer(handle, "AgentPositions", AgentPositions);
    }

    private void UpdateAgents()
    {
        int handle = Shader.FindKernel("UpdateAgents");

        Shader.SetFloat("DeltaTime", Time.deltaTime);
        Shader.SetTexture(handle, "PrevCycle", texture_1);

        Shader.Dispatch(handle, NumAgents, 1, 1);

        AgentAngles.GetData(agentAngles);
    }

    private void UpdateTexture()
    {
        /* First regenerate the image */
        int handle = Shader.FindKernel("UpdateTexture");

        Shader.SetTexture(handle, "PrevCycle", texture_1);
        Shader.SetTexture(handle, "OutputTex", texture_2);

        Shader.Dispatch(handle, Mathf.CeilToInt(Size.x / 8.0f), Mathf.CeilToInt(Size.y / 8.0f), 1);

        /* Next blur the image */
        handle = Shader.FindKernel("Blur");
        Shader.SetTexture(handle, "ToBlur", texture_2);
        Shader.SetTexture(handle, "BlurResult", texture_1);

        Shader.Dispatch(handle, Mathf.CeilToInt(Size.x / 8.0f), Mathf.CeilToInt(Size.y / 8.0f), 1);
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
