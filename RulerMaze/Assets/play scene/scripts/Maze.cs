using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based with MazeGraph,fill the chunk
public class Maze : MonoBehaviour{
    public int mazeRows=50;
    public int mazeColumns=50;
    public float blockLength=0.1f;
    //以下blockLength作为单位“1”：
    public int wallSize=1;
    public int roomSize=5;//per cell
    public int wallHeight=6;

    public Material concreteMtrl;
    public GameObject parentGO;

    private MazeGraph mazeGraph;
    private MCTerrain mcterrain;

    private LineRenderer lineRenderer;

	void Start () 
    {
        //mazeGraph = new MazeGraph(mazeRows, mazeColumns);
        //mazeGraph = new MazeGraphWithUnionSet(mazeRows, mazeColumns);
        //mazeGraph=new MazeGraphByBacktracking(mazeRows,mazeColumns);
        if (GameData.instance.mazeDifficulty == MazeDifficulty.EASY)
        {
            mazeGraph = new MazeGraphWithUnionSet(mazeRows, mazeColumns);
        }
        else//hard
        {
            mazeGraph=new MazeGraphByBacktracking(mazeRows,mazeColumns);
        }
        mazeGraph.Generate();
        //一个mesh装不下
       // MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        //meshRenderer.material = mazeMtrl;
        //MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        //MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        //mcterrain = new MCTerrain(meshRenderer, meshCollider, meshFilter, blockLength, mazeColumns, wallHeight, mazeRows);
        int chunkSizeX=mazeColumns*(wallSize+roomSize)+wallSize;
        int chunkSizeZ=mazeRows*(wallSize+roomSize)+wallSize;
        mcterrain = new MCTerrain(parentGO,concreteMtrl,blockLength, chunkSizeX, wallHeight, chunkSizeZ);
        FillChunk();
        mcterrain.BuildMesh();

        lineRenderer = GetComponent<LineRenderer>();
        CalPathRenderer();
	}

    public void ReBuild()
    {
        mazeGraph.Generate();
        mcterrain.Reset();
        FillChunk();
        mcterrain.BuildMesh();
        CalPathRenderer();
    }
	
    //画线预览效果
    void MazeDraft()
    {
        //重新生成
        //按的时候前提需要focus on Game window，有点尴尬…
        if (Input.GetKeyDown(KeyCode.G))
            mazeGraph.Generate();

        float cellWidth = 0.2f;
        float cellDepth = 0.2f;
        float halfMazeWidth=mazeGraph.MazeColumns*cellWidth*0.5f;
        float halfMazeDepth=mazeGraph.MazeRows*cellDepth*0.5f;
        int[,] mazeTable = mazeGraph.MazeTable;
        for (int i = 0; i < mazeGraph.CellNum; i++)
        {
            int row = i / mazeGraph.MazeColumns;
            int column = i % mazeGraph.MazeColumns;
            //画右边的竖线
            if(column != mazeGraph.MazeColumns - 1 && mazeTable[i,3] < 0)
            {
                Vector3 start = new Vector3((column + 1) * cellWidth-halfMazeWidth, 0f, halfMazeDepth-row * cellDepth);
                Vector3 end=new Vector3((column + 1) * cellWidth-halfMazeWidth,0f,halfMazeDepth-(row+1) * cellDepth);
                Debug.DrawLine(start,end,Color.blue);
            }
            //画下面的横线
            if(row != mazeGraph.MazeRows - 1 && mazeTable[i,1] < 0)
            {
                Vector3 start = new Vector3(column * cellWidth-halfMazeWidth, 0f, halfMazeDepth-(row+1) * cellDepth);
                Vector3 end=new Vector3((column + 1) * cellWidth-halfMazeWidth,0f,halfMazeDepth-(row+1) * cellDepth);
                Debug.DrawLine(start,end,Color.blue);
            }
        }
        //画四条边
        Debug.DrawLine(new Vector3(-halfMazeWidth, 0f, halfMazeDepth),new Vector3(halfMazeWidth, 0f, halfMazeDepth),Color.blue);
        Debug.DrawLine(new Vector3(halfMazeWidth, 0f, halfMazeDepth),new Vector3(halfMazeWidth, 0f, cellDepth-halfMazeDepth),Color.blue);
        Debug.DrawLine(new Vector3(-halfMazeWidth, 0f, halfMazeDepth-cellDepth),new Vector3(-halfMazeWidth, 0f, -halfMazeDepth),Color.blue);
        Debug.DrawLine(new Vector3(-halfMazeWidth, 0f, -halfMazeDepth),new Vector3(halfMazeWidth, 0f, -halfMazeDepth),Color.blue);

        //path
        List<int> path=mazeGraph.CalPath();
        Vector3 pathStart=new Vector3(-halfMazeWidth+0.5f*cellWidth,0f,halfMazeDepth-0.5f*cellDepth),pathEnd;
        for (int i = path.Count - 2; i >= 0; i--)
        {
            int row = path[i] / mazeGraph.MazeColumns;
            int column = path[i] % mazeGraph.MazeColumns;
            pathEnd=new Vector3(column*cellWidth-halfMazeWidth+0.5f*cellWidth,0f,halfMazeDepth-row*cellDepth-0.5f*cellDepth);
            Debug.DrawLine(pathStart,pathEnd,Color.green);
            pathStart=pathEnd;
        }
        Debug.DrawLine(pathStart,new Vector3(halfMazeWidth-0.5f*cellWidth,0f,-halfMazeDepth+0.5f*cellDepth),Color.green);
    }

	void Update () 
    {
        //debug
        //MazeDraft();
	}

    void FillChunk()
    {
        int tile = wallSize + roomSize;//not cell size but T

        for (int x = 0; x < mcterrain.chunk.GetLength(2); x++)
            for (int y = 0; y < mcterrain.chunk.GetLength(1); y++)
                for (int z = 0; z < mcterrain.chunk.GetLength(0); z++)
                {
                    //debug
                    //mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;//CTM怎么就超出数组范围了？！
                    //——》第三个for复制过来时中间x没改成z…

                    //in which cell
                    int column=x/tile;
                    int row = z / tile;
                    //哈哈哈，上下颠倒了，chunk所使用的z方向同L，与maze graph的相反。show path时对照MazeDraft才发现
                    //row = mazeGraph.MazeRows - 1 - row;//不该放到这里，还没判断有没出界
                    //整个Maze的右墙or下墙——》上墙
                    if (column >= mazeColumns || row >= mazeRows)
                    {
                        mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;
                        continue;
                    }
                    row = mazeGraph.MazeRows - 1 - row;
                    int cellIndex = row * mazeGraph.MazeColumns + column;

                    //坉，也为炸弹超人开路
                    if (x % tile < wallSize && z % tile < wallSize)
                    {
                        mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;
                    }
                    else if(x % tile < wallSize )
                    {
                        //左边是否有墙
                        if (mazeGraph.MazeTable[cellIndex, 2] < 0)
                        {
                            mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;
                        }
                    }
                    else if(z % tile < wallSize)
                    {
                        /*
                        //上边是否有墙
                        if (mazeGraph.MazeTable[cellIndex, 0] < 0)
                        {
                            mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;
                        }
                        */
                        //哈哈哈，上下颠倒了
                        //下边是否有墙
                        if (mazeGraph.MazeTable[cellIndex, 1] < 0)
                        {
                            mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.Concrete;
                        }
                    }
                }

        //修补
        //entrance:
        for (int x = 0; x < wallSize; x++)
            for (int z =mcterrain.chunk.GetLength(0)-1-wallSize; z >mcterrain.chunk.GetLength(0)-1- tile; z--)
                for (int y = 0; y < wallHeight; y++)
                    mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.None;

        //debug——》原来是忘了将count减1才是index！
        /*for (int y = 0; y < wallHeight; y++)
            mcterrain.chunk[0, y, mcterrain.chunk.GetLength(0)].mtrlType = MCTerrain.Block.BlockType.None;*/
        
        //exit:
        for (int x = mcterrain.chunk.GetLength(2)-1; x >mcterrain.chunk.GetLength(2)-1- wallSize; x--)
            for (int z = wallSize; z < tile; z++)
                for (int y = 0; y < wallHeight; y++)
                    mcterrain.chunk[x, y, z].mtrlType = MCTerrain.Block.BlockType.None;
    }

    void CalPathRenderer()
    {
        List<int> path=mazeGraph.CalPath();
        lineRenderer.numPositions = path.Count;
        float tileLength = (wallSize + roomSize) * blockLength;
        //the offset of top left corner to center per cell
        float toCenter=(wallSize+roomSize/2.0f)*blockLength;
        Vector3 offset = new Vector3(toCenter - mcterrain.HalfChunkSize.x, 0f, mcterrain.HalfChunkSize.z - toCenter);
        for(int i=0;i<path.Count;i++)
        {
            int row = path[i] / mazeGraph.MazeColumns;
            int column = path[i] % mazeGraph.MazeColumns;

            Vector3 pos = new Vector3(column * tileLength, 0, -row * tileLength) + offset;
            lineRenderer.SetPosition(i, pos);
        }
    }

}
