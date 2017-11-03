using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generate maze procedurally,in data structure aspect i.e. graph
//base，因为有多种迷宫生成算法，我都想试一下
public class MazeGraph
{
    //public int mazeRows;
    //public int mazeColumns;
    //又不是MonoBehaviour
    protected int mazeRows;
    public int MazeRows
    {
        get
        {
            return mazeRows;
        }
        set
        {
            if (value > 1)
            {
                mazeRows = value;
            }
        }
    }
    protected int mazeColumns;
    public int MazeColumns
    {
        get
        {
            return mazeColumns;
        }
        set
        {
            if (value > 1)
            {
                mazeColumns = value;
            }
        }
    }
    protected int cellNum;
    public int CellNum
    {
        get{ return cellNum;}
    }

    protected int[,] mazeGraph;
    public int[,] MazeTable
    {
        get
        {
            return mazeGraph;
        }
        //private set;
    }

    public MazeGraph()
    {
        mazeRows = mazeColumns = 50;

        cellNum = mazeRows * mazeColumns;
        //mazeGraph=new int[cellNum,cellNum] ;
        //哈哈，迷宫grid比较特殊，直接相连的即每个顶点的弧最多4个
        mazeGraph = new int[cellNum, 4];
        ResetGraph();
    }

    public MazeGraph(int rows,int columns)
    {
        mazeRows = rows;
        mazeColumns = columns;
        cellNum = mazeRows * mazeColumns;
        //mazeGraph=new int[cellNum,cellNum] ;
        //哈哈，迷宫grid比较特殊，直接相连的即每个顶点的弧最多4个
        mazeGraph = new int[cellNum, 4];
        ResetGraph();
    }

    protected void ResetGraph()
    {
        for (int i = 0; i < mazeGraph.GetLength(0); i++)
        {
            //0 1 2 3index分别约定为上下左右
            //-1表示不直接相连
            mazeGraph[i,0] = -1;
            mazeGraph[i,1] = -1;
            mazeGraph[i,2] = -1;
            mazeGraph[i,3] = -1;
        }
    }

    //abstract virtual public void Generate();//c++的=0
   virtual public void Generate()
    {
        
    }

    //因为无权，Dijkstra直接简化成BFS
    virtual public List<int> CalPath()
    {
        //遍历时的路径
        int[] prevCellIndex = new int[cellNum];
        bool[] ifVisited = new bool[cellNum];
        for(int i = 0; i < cellNum; i++)
        {
            prevCellIndex[i] =-1;
            ifVisited[i] = false;
        }   
        ifVisited[0] = true;
        //no matter tree or graph,as to BFS,just use queue!
        Queue<int> unSearchCells=new Queue<int>();
        unSearchCells.Enqueue(0);
        //上下左右查找index offset，省得switch case
        int[] offsetTable=new int[4]{-mazeColumns,mazeColumns,-1,1};
        while(!ifVisited[cellNum-1] && unSearchCells.Count>0)
        {
            int cell = unSearchCells.Dequeue();
            for(int i = 0; i < 4; i++)
            {
                if (mazeGraph[cell,i] > 0)
                {
                    int linkedCellIndex = cell + offsetTable[i];
                    if(ifVisited[linkedCellIndex]) 
                        continue;

                    ifVisited[linkedCellIndex] = true;
                    prevCellIndex[linkedCellIndex] = cell;
                    unSearchCells.Enqueue(linkedCellIndex);
                }
            }

        }

        int pathCell = cellNum - 1;
        List<int> path = new List<int>();
        path.Add(pathCell);
        while(pathCell != 0)
        {
            pathCell = prevCellIndex[pathCell];
            path.Add(pathCell);
        }
        return path;
    }

}





public class MazeGraphWithUnionSet:MazeGraph
{

    //连通集
    class UnionSet
    {
        private int[] unionArray;

        public UnionSet(int size)
        {
            unionArray=new int[size];
            Reset();
        }

        void Union(int root1,int root2)
        {
            //unionArray[root1] = root2;
            //optimize，合并连通集即子树时尽量不增加树的高度
            if(unionArray[root1] < unionArray[root2])
            {
                unionArray[root2] = root1;
            }
            else
            {
                if(unionArray[root1] == unionArray[root2])
                {
                    unionArray[root2]--;
                }   
                unionArray[root1] = root2;
            }  
        }

        int FindSet(int x)
        {
            /*while(unionArray[x] >= 0)
        {
            x = unionArray[x];
        }
        return x;*/
            //optimize，改为共同指向连通集代表而不是链表
            if(unionArray[x] < 0) return x;
            return unionArray[x] = FindSet(unionArray[x]);
        }

        public bool SameSet(int x, int y)
        {
            return FindSet(x) == FindSet(y);
        }

        public void UnionElement(int x,int y)
        {
            Union(FindSet(x), FindSet(y));
        }

        public void Reset()
        {
            for(int i=0;i<unionArray.Length;i++)
            {
                unionArray[i]=-1;
            }
        }

    }


    private UnionSet unionSet;
  
    public MazeGraphWithUnionSet()
    {
        unionSet = new UnionSet(cellNum);
    }
   
    public MazeGraphWithUnionSet(int rows,int columns):base(rows,columns)
    {
        unionSet = new UnionSet(cellNum);
    }

    void Reset()
    {
        //reset maze graph
        ResetGraph();
        //reset union set
        unionSet.Reset();
    }

    override public void Generate()
    {
        Reset();
        while(!AllLinkedToFirstCell())
        {
            int cell1=0, cell2=0;
            PickRandomCellPairs(ref cell1,ref cell2);
            if(!unionSet.SameSet(cell1, cell2))
            {
                unionSet.UnionElement(cell1, cell2);
                UpdateGraph(cell1, cell2);
            }   
        }   
    }

    //i.e. linked to entrance
    bool AllLinkedToFirstCell()
    {
        for(int i = 1; i < cellNum; i++)
        {
            if(!unionSet.SameSet(0, i)) 
                return false;
        }   
        return true;
    }

    void PickRandomCellPairs(ref int cell,ref int neighbor)
    {
        cell =(int) (Random.value * cellNum);
        List<int> neiborCells =new List<int>(); 
        int row = cell / mazeColumns;
        int column = cell % mazeColumns;
        if(row != 0)
        { 
            neiborCells.Add(cell - mazeColumns);
        }   
        if(row != mazeRows - 1)
        { 
            neiborCells.Add(cell + mazeColumns);
        }   
        if(column != 0)
        { 
            neiborCells.Add(cell - 1); 
        }   
        if(column != mazeColumns - 1)
        { 
            neiborCells.Add(cell + 1); 
        }   
        neighbor =neiborCells[(int) (Random.value * neiborCells.Count)];
    }

    void UpdateGraph(int x, int y)
    {
        int diff = Mathf.Abs(x - y);
        if ( diff == 1)//左右相邻
        {
            if (x > y)
            {
                mazeGraph[x,2] = 1;
                mazeGraph[y,3] = 1;
            }
            else
            {
                mazeGraph[x,3] = 1;
                mazeGraph[y,2] = 1;
            }
        }

        if(diff == mazeColumns)
        {
            if (x > y)
            {
                mazeGraph[x,0] = 1;
                mazeGraph[y,1] = 1;
            }
            else
            {
                mazeGraph[x,1] = 1;
                mazeGraph[y,0] = 1;
            }
        }
    }

}




//严格来说just recursion，算不上回溯
public class MazeGraphByBacktracking:MazeGraph
{
    bool[,] bCellsVisited;
    //避免switch case,将就着用一下吧，待用int2代替
    int[,] directionsTable=new int[4,2]{{-1,0},{1,0},{0,-1},{0,1}};

    void ResetVisited()
    {
        for (int i = 0; i < bCellsVisited.GetLength(1); i++)
            for (int j = 0; j < bCellsVisited.GetLength(0); j++)
                bCellsVisited[i, j] = false;
    }

    void Reset()
    {
        ResetGraph();
        ResetVisited();
    }

    public MazeGraphByBacktracking()
    {
        bCellsVisited = new bool[mazeRows, mazeColumns];
    }

    public MazeGraphByBacktracking(int rows,int columns):base(rows,columns)
    {
        bCellsVisited = new bool[mazeRows, mazeColumns];
    }

    override public void Generate()//沿着随机路径拆墙
    {
        Reset();
        //pick random start cell
        int startRow=(int)(Random.value*mazeRows);
        int startColumn = (int)(Random.value * mazeColumns);
        March(startRow, startColumn);
    }

    void March(int row,int column)
    {
        bCellsVisited[row,column] = true;
        int cellIndex = row * mazeColumns + column;
        int direction = GetDirection(row, column);
        while(direction>=0 && direction<=3)//倒回来时还有路可走
        {
            int nextRow = row + directionsTable[direction,0];
            int nextColumn=column+directionsTable[direction,1];

            //边探边拆
            int nextCellIndex=nextRow*mazeColumns+nextColumn;
            mazeGraph[cellIndex,direction] = 1;
            //int oppositeDir = directionIndex + (directionIndex & 0x00000001) == 0 ? 1 : -1;
            int oppositeDir = direction + ((direction & 0x00000001) == 0 ? 1 : -1);
            mazeGraph[nextCellIndex, oppositeDir] = 1;

            March(nextRow, nextColumn);
            direction = GetDirection(row, column);
        }

    }

    //边界和visited不可通，得到的可通directions中任选一条
    int GetDirection(int i,int j)
    {
        List<int> directions = new List<int>();
        //上
        if (i - 1 >= 0 && !bCellsVisited[i - 1, j])
            directions.Add(0);//0 1 2 3分别表上下左右
        //下
        if (i + 1 < mazeRows && !bCellsVisited[i + 1, j])
            directions.Add(1);
        //左
        if (j - 1 >= 0 && !bCellsVisited[i, j-1])
            directions.Add(2);
        //右
        if (j+1<mazeColumns && !bCellsVisited[i, j+1])
            directions.Add(3);

        if (directions.Count == 0)
            return -1;

        int randomIndex = (int)(Random.value * directions.Count);
        return directions[randomIndex];
    }

    //原，没回溯这种算法行不通
    /*
    void March(int row,int column)
    {
        bCellsVisited[row,column] = true;
        int cellIndex = row * mazeColumns + column;
        List<int> directions = GetDirections(row, column);
        foreach (int directionIndex in directions)//错了，调用完march返回来时，一些原来可通的可能已经不通了,又没有回溯
        {
            int nextRow = row + directionsTable[directionIndex,0];
            int nextColumn=column+directionsTable[directionIndex,1];

            //边探边拆
            int nextCellIndex=nextRow*mazeColumns+nextColumn;
            mazeGraph[cellIndex,directionIndex] = 1;
            //int oppositeDir = directionIndex + (directionIndex & 0x00000001) == 0 ? 1 : -1;
            int oppositeDir = directionIndex + ((directionIndex & 0x00000001) == 0 ? 1 : -1);
            mazeGraph[nextCellIndex, oppositeDir] = 1;

            March(nextRow, nextColumn);
          
        }

    }

    //边界和visited不可通，得到的可通directions需要打乱顺序
    List<int> GetDirections(int i,int j)
    {
        List<int> directions = new List<int>();
        //上
        if (i - 1 >= 0 && !bCellsVisited[i - 1, j])
            directions.Add(0);//0 1 2 3分别表上下左右
        //下
        if (i + 1 < mazeRows && !bCellsVisited[i + 1, j])
            directions.Add(1);
        //左
        if (j - 1 >= 0 && !bCellsVisited[i, j-1])
            directions.Add(2);
        //右
        if (j+1<mazeColumns && !bCellsVisited[i, j+1])
            directions.Add(3);

        //输出之前打乱顺序
        for (int m = 0; m < directions.Count; m++)
        {
            int changeIndex = (int)(Random.value * directions.Count);
            if (changeIndex != m)
            {
                int temp = directions[m];
                directions[m] = directions[changeIndex];
                directions[changeIndex] = temp;
            }
        }
        return directions;
    }
    */
}