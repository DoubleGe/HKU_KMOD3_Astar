using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Linq.Expressions;

public class Astar
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private Heap<Node> openList;
    private List<Cell> closedList;
    private Dictionary<Vector2Int, Node> nodePositions;

    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path from the startPos to the endPos
    /// Note that you will probably need to add some helper functions
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        Cell startCell = grid[startPos.x, startPos.y];

        Node startNode = new Node(startCell.gridPosition, null, startCell, 0, CalculateDistance(startPos, endPos));

        openList = new Heap<Node>(grid.GetLength(0) * grid.GetLength(1));
        openList.Add(startNode);

        closedList = new List<Cell>();
        nodePositions = new Dictionary<Vector2Int, Node>
        {
            { startPos, startNode }
        };

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            if(currentNode.position == endPos) return GetPath(currentNode);

            closedList.Add(currentNode.cell);

            foreach (Cell neighbour in currentNode.cell.GetNeighbours(grid))
            {
                if (closedList.Contains(neighbour)) continue;

                if (neighbour.gridPosition.y > currentNode.position.y && neighbour.HasWall(Wall.DOWN)) continue;
                if (neighbour.gridPosition.y < currentNode.position.y && neighbour.HasWall(Wall.UP)) continue;
                if (neighbour.gridPosition.x > currentNode.position.x && neighbour.HasWall(Wall.LEFT)) continue;
                if(neighbour.gridPosition.x < currentNode.position.x && neighbour.HasWall(Wall.RIGHT)) continue;   
                
                int newGCost = currentNode.GCost + CalculateDistance(currentNode.position, neighbour.gridPosition);

                if (nodePositions.ContainsKey(neighbour.gridPosition))
                {
                    Node neighbourNode = nodePositions[neighbour.gridPosition];
                    if(newGCost < neighbourNode.GCost)
                    {
                        neighbourNode.parent = currentNode;
                        neighbourNode.GCost = newGCost;
                        neighbourNode.HCost = CalculateDistance(neighbourNode.position, endPos);
                    }
                }
                else
                {
                    Node neighbourNode = new Node(neighbour.gridPosition, currentNode, neighbour, newGCost, CalculateDistance(currentNode.position, neighbour.gridPosition));
                    nodePositions.Add(neighbour.gridPosition, neighbourNode);
                    openList.Add(neighbourNode);
                }
            }
        }

        return null;
    }

    private List<Vector2Int> GetPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int> { endNode.position };
        Node currentNode = endNode;

        while(currentNode.parent != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private Node GetLowestFCostNode(Heap<Node> nodeList)
    {
        return nodeList.RemoveFirst();
    }

    private int CalculateDistance(Vector2Int start, Vector2Int end)
    {
        int xDistance = Mathf.Abs(start.x - end.x);
        int yDistance = Mathf.Abs(start.y - end.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node : IHeapItem<Node>
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node
        public Cell cell;

        public int FCost { //GScore + HScore
            get { return GCost + HCost; }
        }

        private int heapIndex;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        public int GCost; //Current Travelled Distance
        public int HCost; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, Cell cell, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.cell = cell;
            this.GCost = GScore;
            this.HCost = HScore;
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(nodeToCompare.HCost);
            }
            return -compare;
        }
    }
}
