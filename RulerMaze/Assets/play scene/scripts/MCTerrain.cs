using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//chunk,i.e. 3D map of blocks with any material
//then build mesh based with chunk filled,by building surface cube faces iteratively
//as for how to fill th chunk,just expose the chunk to specific game play,so the instance of this class should not be bound to any GO
//in other word,it should never be the component of GOs
public class MCTerrain 
{
    private Material concreteMtrl;
    private GameObject parentGO;

    //MC cube
    public class Block
    {
        public enum BlockType
        {
            //空
            None = 0,
            //水泥
            Concrete=1,
            //待加其他material
        };
        public BlockType mtrlType=BlockType.None;
        //待加其他属性,特定game play相关
    }
    //提出到Block外面不作为property，因为是统一的
    private float blockLength;
    //这里的size意为complexity，而不是length in world space
    private int chunkSizeX,chunkSizeY,chunkSizeZ;
    private float halfChunkWidth, halfChunkDepth, halfChunkHeight;
    //read only
    public Vector3 HalfChunkSize
    {
        get
        {
            return new Vector3(halfChunkWidth, halfChunkHeight, halfChunkDepth);
        }
    }
    public Block[,,] chunk;//exposed to get filled

    public MCTerrain(GameObject parentGO,Material concreteMtrl,float blockLength,int chunkSizeX,int chunkSizeY,int chunkSizeZ)
    {
        //c++的初始化参数列表
        this.parentGO=parentGO;
        this.concreteMtrl = concreteMtrl;
        this.blockLength = blockLength;
        this.chunkSizeX = chunkSizeX;
        this.chunkSizeY = chunkSizeY;
        this.chunkSizeZ = chunkSizeZ;

        halfChunkWidth = chunkSizeX * blockLength * 0.5f;
        halfChunkHeight = chunkSizeY * blockLength * 0.5f;
        halfChunkDepth = chunkSizeZ * blockLength * 0.5f;

        chunk = new Block[chunkSizeX, chunkSizeY, chunkSizeZ];
        for (int i = 0; i < chunk.GetLength(0); i++)
            for (int j = 0; j < chunk.GetLength(1); j++)
                for (int k = 0; k < chunk.GetLength(2); k++)
                {
                    chunk[i, j, k] = new Block();
                }
        
    }
        
    public void Reset()//不是MonoBehaviour,可以用这个名字
    {
        //destroy meshes
        foreach (Transform child in parentGO.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //reset chunk
        for (int i = 0; i < chunk.GetLength(0); i++)
            for (int j = 0; j < chunk.GetLength(1); j++)
                for (int k = 0; k < chunk.GetLength(2); k++)
                {
                    chunk[i, j, k].mtrlType = Block.BlockType.None;
                }
    }

    //外界填好chunk后，生成mesh
    public void BuildMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //遍历chunk, 生成其中的每一个Block
        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int y = 0; y < chunkSizeY; y++)
            {
                for (int z = 0; z <chunkSizeZ; z++)
                {
                    BuildBlock(x, y, z, verts, uvs, tris);
                }
            }
        }

        int vertexStart = 0;//16 bit indices (2^16 == 65536) 4~6~4
        int triIndexStart=0;
        int subGoIndex=0;
        const int maxVertices = 40000;//per mesh
        const int maxTriIndices=maxVertices*3/2;

        //C#循环中的局部变量，在运行完循环后TM还不能重新声明拿来用，按道理来说应该销毁了不存在冲突啊
        GameObject go;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        MeshFilter meshFilter;
        Mesh chunkMesh;
        while (verts.Count  - vertexStart > maxVertices)
        {
            //create subGO
            go = new GameObject("sub" + subGoIndex.ToString());
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.parent = parentGO.transform;
            meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = concreteMtrl;
            meshCollider = go.AddComponent<MeshCollider>();
            meshFilter = go.AddComponent<MeshFilter>();
             
            chunkMesh = new Mesh();
            chunkMesh.vertices = verts.GetRange(vertexStart,maxVertices).ToArray();
            chunkMesh.uv = uvs.GetRange(vertexStart,maxVertices).ToArray();
            /*chunkMesh.triangles = tris.GetRange(triIndexStart,maxTriIndices).ToArray();
            //Failed setting triangles. Some indices are referencing out of bounds vertices.
            //我知道错在哪里了，截取的数组，vertex index又从0开始，tris中相应段要减：
            foreach (int index in chunkMesh.triangles)
            {
                index -= triIndexStart;
            }*/
            //——》哈哈！傻逼！我tris一直用第一段不就行了吗！
            chunkMesh.triangles = tris.GetRange(0,maxTriIndices).ToArray();
            chunkMesh.RecalculateBounds();
            chunkMesh.RecalculateNormals();
            meshFilter.mesh = chunkMesh;
            meshCollider.sharedMesh = chunkMesh;

            vertexStart += maxVertices;
            triIndexStart += maxTriIndices;
            subGoIndex++;
        }
        //rest
        go = new GameObject("sub" + subGoIndex.ToString());
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.parent = parentGO.transform;
        meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = concreteMtrl;
        meshCollider = go.AddComponent<MeshCollider>();
        meshFilter = go.AddComponent<MeshFilter>();

        chunkMesh = new Mesh();
        chunkMesh.vertices = verts.GetRange(vertexStart,verts.Count-vertexStart).ToArray();
        chunkMesh.uv = uvs.GetRange(vertexStart,verts.Count-vertexStart).ToArray();
        //chunkMesh.triangles = tris.GetRange(triIndexStart,tris.Count-triIndexStart).ToArray();
        int restTris=(verts.Count-vertexStart)*3/2;
        chunkMesh.triangles = tris.GetRange(0,restTris).ToArray();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateNormals();
        meshFilter.mesh = chunkMesh;
        meshCollider.sharedMesh = chunkMesh;

    }

    void BuildBlock(int x, int y, int z, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        //空气
        if (chunk[x,y,z].mtrlType == Block.BlockType.None)
            return;

        //Left
        if (CheckNeedBuildFace(x - 1, y, z))
            BuildFace(new Vector3(x*blockLength-halfChunkWidth, y*blockLength-halfChunkHeight, (z+1)*blockLength-halfChunkDepth),
                Vector3.up, -Vector3.forward, verts, uvs, tris);
        
        //Right
        if (CheckNeedBuildFace(x + 1, y, z))
            BuildFace(new Vector3((x+1)*blockLength-halfChunkWidth, y*blockLength-halfChunkHeight, z*blockLength-halfChunkDepth),
                Vector3.up, Vector3.forward, verts, uvs, tris);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z))
            BuildFace(new Vector3(x*blockLength-halfChunkWidth, y*blockLength-halfChunkHeight, (z+1)*blockLength-halfChunkDepth),
                -Vector3.forward, Vector3.right, verts, uvs, tris);
        
        //Top
        if (CheckNeedBuildFace(x, y + 1, z))
            BuildFace(new Vector3(x*blockLength-halfChunkWidth, (y+1)*blockLength-halfChunkHeight, z*blockLength-halfChunkDepth),
                Vector3.forward, Vector3.right, verts, uvs, tris);

        //Back——》注意！对我们（观察者）来说它才是正面！一开始搞反了造成前后内壁可见…
        if (CheckNeedBuildFace(x, y, z - 1))
            BuildFace(new Vector3(x*blockLength-halfChunkWidth, y*blockLength-halfChunkHeight, z*blockLength-halfChunkDepth),
                Vector3.up, Vector3.right, verts, uvs, tris);
        
        //Front
        if (CheckNeedBuildFace(x, y, z + 1))
            BuildFace(new Vector3((x+1)*blockLength-halfChunkWidth, y*blockLength-halfChunkHeight, (z+1)*blockLength-halfChunkDepth),
            Vector3.up, -Vector3.right, verts, uvs, tris);
    }

    bool CheckNeedBuildFace(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= chunkSizeX || y >= chunkSizeY || z >= chunkSizeZ)
            return true;
        
        switch (chunk[x,y,z].mtrlType)
        {
            case Block.BlockType.None:
                return true;
            default:
                return false;
        }
    }

    void BuildFace(Vector3 corner, Vector3 up, Vector3 right,List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        int index = verts.Count;

        verts.Add (corner);
        verts.Add (corner + up*blockLength);
        verts.Add (corner + up*blockLength + right*blockLength);
        verts.Add (corner + right*blockLength);

        //暂且这么简单吧，待拓为根据block type以及是否wrap
        uvs.Add(new Vector2(0f,1f));
        uvs.Add(new Vector2(0f,0f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(1f,1f));

        tris.Add(index + 0);
        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 2);
        tris.Add(index + 3);
        tris.Add(index + 0);
    }

}
