using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MapGen;

public class Node
{
    public Node cameFrom = null; //parent node
    public double priority = 0; // F value
    public double costSoFar = 0; // G Value
    public Tile tile;

    public Node(Tile _tile, double _priority, Node _cameFrom, double _costSoFar)
    {
        cameFrom = _cameFrom;
        priority = _priority; 
        costSoFar = _costSoFar;
        tile = _tile;
    }
}

public class PathFinder
{
    List<Node> TODOList = new List<Node>();
    List<Node> DoneList = new List<Node>();
    Tile goalTile;

    public PathFinder()
    {
        goalTile = null;
    }

    // TODO: Find the path based on A-Star Algorithm
    public Queue<Tile> FindPathAStar(Tile start, Tile goal)
    {
        //return new Queue<Tile>(); // Returns an empty Path
        Queue<Tile> path = new Queue<Tile>();
        Node startingNode = new Node(start, HeuristicsDistance(start, goal), null, 0); // Creates a start node
        
        TODOList.Add(startingNode);

        while(TODOList.Count > 0) // Checks nodes if there are any in the TODOList
        {
            Node node = TODOList[0];

            for(int i = 0; i < TODOList.Count; i++)
            {
                if(TODOList[i].priority < node.priority) // Compares the F value of the nodes
                {
                    if (HeuristicsDistance(TODOList[i].tile, goal) <
                        HeuristicsDistance(node.tile, goal)) // Compares the H value of the nodes
                    {
                        node = TODOList[i];
                    }
                }
            }

            TODOList.Remove(node); // Removes the node from the TODOList
            DoneList.Add(node); // Adds the node to the Completed List

            // If The finished node is the goal of the map, retrace the path.
            if (node.tile == goal)
            {
                path = RetracePath(node);
                break;
            }

            // Creates a list for Adjacent Nodes
            List<Node> adjacentNodes = new List<Node>();
            for (int i = 0; i<node.tile.Adjacents.Count; i++)
            {
                adjacentNodes.Add(new Node(node.tile.Adjacents[i], (node.costSoFar + 10) + (HeuristicsDistance(node.tile.Adjacents[i], goal)), node, node.costSoFar + 10));
            }

            // Adds the adjacentNodes list to the TODOList
            for(int i = 0; i<adjacentNodes.Count; i++)
            {
                if(adjacentNodes[i].tile.isPassable && !TODOList.Contains(adjacentNodes[i]) && !DoneList.Contains(adjacentNodes[i]))
                {
                    TODOList.Add(adjacentNodes[i]);
                }

            }
            
        }

        return path;  //Returns the A* Algorithm calculated path.
    }

    // TODO: Find the path based on A-Star Algorithm
    // In this case avoid a path passing near an enemy tile
    // BONUS TASK (Required for Honors Contract Students)
    public Queue<Tile> FindPathAStarEvadeEnemy(Tile start, Tile goal)
    {
       //return new Queue<Tile>(); // Returns an empty Path
        Queue<Tile> path = new Queue<Tile>();
        Node startingNode = new Node(start, HeuristicsDistance(start, goal), null, 0); // Creates a start node
        
        TODOList.Add(startingNode);

        while(TODOList.Count > 0) // Checks nodes if there are any in the TODOList
        {
            Node node = TODOList[0];

            for(int i = 0; i < TODOList.Count; i++)
            {
                if(TODOList[i].priority < node.priority) // Compares the F value of the nodes
                {
                    if (HeuristicsDistance(TODOList[i].tile, goal) <
                        HeuristicsDistance(node.tile, goal)) // Compares the H value of the nodes
                    {
                        node = TODOList[i];
                    }
                }
            }

            TODOList.Remove(node); // Removes the node from the TODOList
            DoneList.Add(node); // Adds the node to the Completed List

            // If The finished node is the goal of the map, retrace the path.
            if (node.tile == goal)
            {
                path = RetracePath(node);
                break;
            }

            // Creates a list for Adjacent Nodes
            List<Node> adjacentNodes = new List<Node>();
            for (int i = 0; i<node.tile.Adjacents.Count; i++)
            {
                adjacentNodes.Add(new Node(node.tile.Adjacents[i], (node.costSoFar + 10) + (HeuristicsDistance(node.tile.Adjacents[i], goal)), node, node.costSoFar + 10));
            }

            // Adds the adjacentNodes list to the TODOList
            for(int i = 0; i<adjacentNodes.Count; i++)
            {
                if(adjacentNodes[i].tile.isPassable && !TODOList.Contains(adjacentNodes[i]) && !DoneList.Contains(adjacentNodes[i]))
                {
                    TODOList.Add(adjacentNodes[i]);
                }

            }
            
        }

        return path;  //Returns the A* Algorithm calculated path.
    }

    // Manhattan Distance with horizontal/vertical cost of 10
    double HeuristicsDistance(Tile currentTile, Tile goalTile)
    {
        int xdist = Math.Abs(goalTile.indexX - currentTile.indexX);
        int ydist = Math.Abs(goalTile.indexY - currentTile.indexY);
        // Assuming cost to move horizontally and vertically is 10
        //return manhattan distance
        return (xdist * 10 + ydist * 10);
    }

    // Retrace path from a given Node back to the start Node
    Queue<Tile> RetracePath(Node node)
    {
        List<Tile> tileList = new List<Tile>();
        Node nodeIterator = node;
        while (nodeIterator.cameFrom != null)
        {
            tileList.Insert(0, nodeIterator.tile);
            nodeIterator = nodeIterator.cameFrom;
        }
        return new Queue<Tile>(tileList);
    }

    // Generate a Random Path. Used for enemies
    public Queue<Tile> RandomPath(Tile start, int stepNumber)
    {
        List<Tile> tileList = new List<Tile>();
        Tile currentTile = start;
        for (int i = 0; i < stepNumber; i++)
        {
            Tile nextTile;
            //find random adjacent tile different from last one if there's more than one choice
            if (currentTile.Adjacents.Count < 0)
            {
                break;
            }
            else if (currentTile.Adjacents.Count == 1)
            {
                nextTile = currentTile.Adjacents[0];
            }
            else
            {
                nextTile = null;
                List<Tile> adjacentList = new List<Tile>(currentTile.Adjacents);
                ShuffleTiles<Tile>(adjacentList);
                if (tileList.Count <= 0) nextTile = adjacentList[0];
                else
                {
                    foreach (Tile tile in adjacentList)
                    {
                        if (tile != tileList[tileList.Count - 1])
                        {
                            nextTile = tile;
                            break;
                        }
                    }
                }
            }
            tileList.Add(currentTile);
            currentTile = nextTile;
        }
        return new Queue<Tile>(tileList);
    }

    private void ShuffleTiles<T>(List<T> list)
    {
        // Knuth shuffle algorithm :: 
        // courtesy of Wikipedia :) -> https://forum.unity.com/threads/randomize-array-in-c.86871/
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }
}
