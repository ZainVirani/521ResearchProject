using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AStarBehavior : MonoBehaviour {
    
    public int rewindsUsed;
    public int rewindCost;
    int maxSnapshots;
    GameTile currentTile;
    GameTile goalTile;
    List<GameTile> path;

    public GameSystem gameSystem;
    public GameMapBehavior mapBehavior;
    public PlayerController player;

	void Start () {
        rewindsUsed = 0;
        gameSystem = GameObject.Find("GameSystem").GetComponent<GameSystem>();
        mapBehavior = GameObject.Find("GameMap").GetComponent<GameMapBehavior>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        goalTile = mapBehavior.GetTileAt(mapBehavior.goalPosition);
        currentTile = mapBehavior.GetTileAt(player.curMapPos);
        maxSnapshots = gameSystem.maxSnapshots;
        path = new List<GameTile>();
	}

    void Rewind(int snapshotsAgo)
    {
        gameSystem.Rewind(snapshotsAgo);
        rewindsUsed++;
    }

    int RewindCost()
    {
        return rewindCost * rewindsUsed;
    }

    List<GameTile> BuildPath(GameTile start, GameTile end)
    {
        path = new List<GameTile>();
        GameTile currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    public List<GameTile> FindPath()
    {
        /*
        OPEN: set of nodes to be evaluated
        CLOSED: set of nodes already evaluated
        G: dist of a node from current pos
        H: dist of a node from target pos
        F: G+H
        */
        currentTile = mapBehavior.GetTileAt(player.curMapPos);
        List<GameTile> OPEN = new List<GameTile>();
        List<GameTile> CLOSED = new List<GameTile>();
        currentTile.G = 0;
        //currentTile.H = currentTile.distance(goalTile);
        currentTile.F = currentTile.H + currentTile.G;
        OPEN.Add(currentTile);
        GameTile current = currentTile;

        /*
        loop
            current = node in OPEN with lowest F
            remove current from OPEN
            add current to CLOSED

            if current is target node
                return
        */

        //int counter = 0; //meant for bounding A*, which ended up not being necessary
        //int k = 0;
        while (true)
        {
            float lowestF = Mathf.Infinity;
            for (int i = 0; i < OPEN.Count; i++)
            {
                if (OPEN[i].F < lowestF)
                {
                    lowestF = OPEN[i].F;
                    current = OPEN[i];
                }
            }

            OPEN.Remove(current);
            CLOSED.Add(current);

            if (current.mapPosition.GetPosition() == goalTile.mapPosition.GetPosition()) //|| counter > aStarLimit)
            {
                return BuildPath(currentTile, current);
            }

            List<GameTile> neighbours = new List<GameTile>();
            //find all appropriate neighbours
            Vector2 up = current.mapPosition.GetPosition() + new Vector2(0, 1);
            Vector2 down = current.mapPosition.GetPosition() + new Vector2(0, -1);
            Vector2 left = current.mapPosition.GetPosition() + new Vector2(1, 0);
            Vector2 right = current.mapPosition.GetPosition() + new Vector2(-1, 0);
            Vector2 NW = current.mapPosition.GetPosition() + new Vector2(-1, 1);
            Vector2 NE = current.mapPosition.GetPosition() + new Vector2(1, 1);
            Vector2 SW = current.mapPosition.GetPosition() + new Vector2(-1, -1);
            Vector2 SE = current.mapPosition.GetPosition() + new Vector2(1, -1);
            Vector2 currentLoc = current.mapPosition.GetPosition();
            //add appropriate neighbours
            neighbours.Add(mapBehavior.GetTileAt(up));
            neighbours.Add(mapBehavior.GetTileAt(down));
            neighbours.Add(mapBehavior.GetTileAt(left));
            neighbours.Add(mapBehavior.GetTileAt(right));
            neighbours.Add(mapBehavior.GetTileAt(NW));
            neighbours.Add(mapBehavior.GetTileAt(NE));
            neighbours.Add(mapBehavior.GetTileAt(SW));
            neighbours.Add(mapBehavior.GetTileAt(SE));
            /*
            foreach neighbour of current
                if neighbour is not traversable or neighbour is in CLOSED
                    skip to next neighbour
                
                if new path to neighbour is shorter or neighbour is not in OPEN
                    set F of neighbour
                    set parent of neighbour to current
                    if neighbour is not in OPEN
                        add neighbour to OPEN
            */
            foreach (GameTile n in neighbours)
            {
                if (!n.isPassable() || CLOSED.Contains(n)) //instead we just check at 0
                    continue;

                //GameTile is clear for planning
                float newNeighbourCost = current.G + distance(current.mapPosition.GetPosition(), n.mapPosition.GetPosition());
                if (OPEN.Contains(n) == false || newNeighbourCost < distance(n.mapPosition.GetPosition(), currentLoc))
                {
                    n.G = newNeighbourCost;
                    n.H = distance(n.mapPosition.GetPosition(), goalTile.mapPosition.GetPosition());
                    n.F = n.G + n.H;
                    n.parent = current;

                    if (!OPEN.Contains(n))
                        OPEN.Add(n);
                }
            }
        }
    }

    public static float distance(Vector2 a, Vector2 b)
    {
        int xDif = Mathf.Abs((int)b.x - (int)a.x);
        int yDif = Mathf.Abs((int)b.y - (int)a.y);
        if (yDif > xDif)
        {
            return (14f * xDif) + (10f * (yDif - xDif));
        }
        else
        {
            return (14f * yDif) + (10f * (xDif - yDif));
        }
    }
}
