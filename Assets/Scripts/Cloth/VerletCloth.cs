using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WireSpring
{
    public float enableMinDis;
    public float enableMaxDis;
    public float k;

    public WireSpring(float min, float max, float k)
    {
        this.enableMinDis = min;
        this.enableMaxDis = max;
        this.k = k;
    }

    //dir:pos1->pos2
    public Vector3 CalHookeanForce(Vector3 pos1, Vector3 pos2)
    {
        Vector3 result = Vector3.zero;
        float dis = Vector3.Distance(pos1, pos2);
        if (dis >= enableMinDis && dis <= enableMaxDis)
        {
            result = k * dis * (pos2 - pos1).normalized;
        }
        return result;
    }
}

[System.Serializable]
public class ClothNode
{
    public bool isStatic = false;
    public float mass = 1f;
    //Rt-1
    public Vector3 r_prev = Vector3.zero;
    //Rt
    public Vector3 r_now = Vector3.zero;
    //force t-1
    public Vector3 f_prev = Vector3.zero;
    //force t
    public Vector3 f_now = Vector3.zero;

    //x(t+1) = 2x(t) − x(t−1)+ (f(t))/m ∆t^2
    public void UpdateState(Vector3 force, float dt)
    {
        if (!isStatic)
        {
            Vector3 r_next = 2 * r_now - r_prev + f_now / mass * dt * dt;

            f_prev = f_now;
            f_now = force;

            r_prev = r_now;
            r_now = r_next;
        }
        
    }
}

public class VerletCloth : MonoBehaviour
{
    public Material mat = null;

    Mesh mesh = null;
    int row = 50;
    int col = 50;
    void Start()
    {
        nodes = new ClothNode[row * col];
        vert = new Vector3[row * col];
        uv = new Vector2[row * col];
        tris = new int[(row - 1) * (col - 1) * 2 * 3];
        for (int i = 0; i < row; ++i)
        {
            for (int j = 0; j < col; ++j)
            {
                Vector3 initPos = new Vector3(j, -i, 0) / 10;
                vert[i * col + j] = initPos;
                var node = new ClothNode();
                node.r_prev = initPos;
                node.r_now = initPos;
                nodes[i * col + j] = node;
                node.isStatic = (i == 0);

                uv[i * col + j] = new Vector2(map(j, 0, (col - 1), 0, 1), map(-i, 0, -(row - 1), 1, 0));
            }
        }
        gameObject.AddComponent<MeshRenderer>().material = mat;
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        UpdateMesh();
    }

    ClothNode[] nodes = null;
    Vector3[] vert = null;
    Vector2[] uv = null;
    int[] tris = null;
    void UpdateMesh()
    {
        int trisIndex = 0;
        for (int i = 0; i < row; ++i)
        {
            for (int j = 0; j < col; ++j)
            {
                if (isStarted) CalculateNodePosition(i, j);
                if (i < row - 1 && j < col - 1)
                {
                    int baseJ = j;
                    tris[trisIndex++] = i * col + baseJ;
                    tris[trisIndex++] = (i + 1) * col + baseJ;
                    tris[trisIndex++] = (i + 1) * col + baseJ + 1;

                    baseJ++;
                    tris[trisIndex++] = i * col + baseJ;
                    tris[trisIndex++] = i * col + baseJ - 1;
                    tris[trisIndex++] = (i + 1) * col + baseJ;

                }
            }
        }

        mesh.vertices = vert;
        mesh.uv = uv;
        mesh.triangles = tris;
    }

    public bool isStarted = false;
    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
        UpdateWindDir();
    }

    int GetNodePosition(int i, int j, out Vector3 outPos)
    {
        outPos = Vector3.zero;
        if (i < 0 || j < 0 || i >= row || j >= col) return -1;

        int idx = i * col + j;
        if (idx >= 0 && idx < vert.Length)
        {
            outPos = vert[i * col + j];
            return 0;
        }

        return -1;
    }

    public float MAX_LEN = 0.5f;
    public float MIN_LEN = 0.05f;
    public WireSpring wireParam = new WireSpring(0.1f, 0.5f, 10);

    public Vector3 windDir = Vector3.zero;
    public Vector3 gravity = new Vector3(0, -2f, 0);

    Vector2Int[] nearNodeIndex = new Vector2Int[] {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0),};
    void CalculateNodePosition(int rowIndex, int colIndex)
    {
        int index = rowIndex * col + colIndex;
        var node = nodes[index];
        if (node.isStatic) return;

        Vector3 pos;
        if (GetNodePosition(rowIndex, colIndex, out pos) == 0)
        {
            Vector3 f = windDir + gravity;
            for (int i = 0; i < nearNodeIndex.Length; ++i)
            {
                var tmpIndex = nearNodeIndex[i];
                int xIndex = rowIndex + tmpIndex.x;
                int yIndex = colIndex + tmpIndex.y;
                Vector3 otherNode;
                if (GetNodePosition(xIndex, yIndex, out otherNode) == 0)
                {
                    f += wireParam.CalHookeanForce(pos, otherNode);
                }
            }

            //update pos
            node.UpdateState(f, Time.deltaTime);
            //constrain
            for (int i = 0; i < nearNodeIndex.Length; ++i)
            {
                var tmpIndex = nearNodeIndex[i];
                int xIndex = rowIndex + tmpIndex.x;
                int yIndex = colIndex + tmpIndex.y;
                Vector3 otherNode;

                if (GetNodePosition(xIndex, yIndex, out otherNode) == 0)
                {
                    float dis = Vector3.Distance(node.r_now, otherNode);
                    if (dis > MAX_LEN)
                    {
                        node.r_now = (node.r_now - otherNode).normalized * MAX_LEN + otherNode;
                    }
                    else if (dis < MIN_LEN)
                    {
                        node.r_now = (node.r_now - otherNode).normalized * MIN_LEN + otherNode;
                    }
                }
            }
            vert[index] = node.r_now;
        }
    }

    float windElapse = 0;
    void UpdateWindDir()
    {
        windElapse -= Time.deltaTime;
        if (windElapse <= 0)
        {
            windElapse = 5;
            if (windDir == Vector3.zero)
            {
                windDir = new Vector3(0, 0, Random.Range(0, 2f));
            }
            else
            {
                windDir = Vector3.zero;
            }
        }
    }


    void OnDrawGizmos()
    {
        if (vert != null)
        {
            for (int i = 0; i < vert.Length; ++i)
            {
                Gizmos.color = nodes[i].isStatic ? Color.red : Color.white;
                Gizmos.DrawSphere(vert[i], 0.01f);
            }
        }
       
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
