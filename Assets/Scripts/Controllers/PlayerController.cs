using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerSnapshot
{
    public MapPosition position;
    public float costSoFar;

    public PlayerSnapshot(MapPosition position, float costSoFar)
    {
        this.position = position;
        this.costSoFar = costSoFar;
    }
}

public static class AppHelper
{
public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}

public class PlayerController : MonoBehaviour {

    public GameMapBehavior map;
    public MapPosition curMapPos;
    public AStarBehavior pathFinder;
    public GameSystem gameSystem;
    List<GameTile> path;
    public List<float> snapShotCosts;
    public List<float> costsSoFar;
    public int rewindsUsed;
    public float rewindCost;
    public int comparisonWindow;
    public float costSoFar;
    int step;
    bool ready = false;
    public bool rewindsAllowed;

    int run;
    float accumulatedCosts;
    float accumulatedRewinds;
    public int totalRuns;

    //init
    void Start ()
    {
        run = 0;
        accumulatedCosts = 0;
        step = 0;
        rewindsUsed = 0;
        costSoFar = 0;
        MoveToMapPosition(map.startPosition);
        path = new List<GameTile>();
        snapShotCosts = new List<float>();
        costsSoFar = new List<float>();
        ready = true;
	}

    //every frame
    private void Update()
    {
        //handleInput();
    }

    //get player pos
    public Vector2 GetPosition()
    {
        return curMapPos.GetPosition();
    }

    //called every step
    public void TravelTowardGoal()
    {
        if (!ready || gameSystem.state != GameState.Running)
            return;
        if (curMapPos == map.goalPosition) //if goal is reached
        {
            Debug.Log("Final path cost: " + costSoFar + ". Total rewinds used: " + rewindsUsed);
            accumulatedCosts += costSoFar;
            accumulatedRewinds += rewindsUsed;
            if (run == totalRuns - 1)
            {
                Debug.Log("TOTAL AVERAGES OVER " + totalRuns + " RUNS: ");
                Debug.Log("AVERAGE COST: " + (accumulatedCosts / totalRuns));
                Debug.Log("AVERAGE REWINDS: " + (accumulatedRewinds / totalRuns));
                AppHelper.Quit();
            }
            else
            {
                step = 0;
                rewindsUsed = 0;
                costSoFar = 0;
                MoveToMapPosition(map.startPosition);
                path = new List<GameTile>();
                snapShotCosts = new List<float>();
                costsSoFar = new List<float>();
                ready = true;
            }
            run++;
            return;
        }

        path = pathFinder.FindPath(); //find a path and traverse the first step
        MoveToTile(path[0]);

        if (snapShotCosts.Count == gameSystem.maxSnapshots)
        {
            snapShotCosts.RemoveAt(0);
        }
        if (costsSoFar.Count == gameSystem.maxSnapshots)
        {
            costsSoFar.RemoveAt(0);
        }
        snapShotCosts.Add(CalculatePathCost()); //update the list of path costs
        costsSoFar.Add(costSoFar);

        if(step == comparisonWindow) //if it's time to consider a rewind
        {
            int rewindTo;
            if (snapShotCosts.Count > 1)
                rewindTo = ComparePreviousStates(); //consider a rewind if there is anywhere to rewind to
            else
                rewindTo = -1;

            if (rewindTo != -1)
            {
                //Debug.Log(CostsToString());
                //Debug.Log("current cost " + snapShotCosts[snapShotCosts.Count - 1]);
                //Debug.Log("rewind by " + (snapShotCosts.Count - rewindTo - 1));
                //Debug.Log("rewind to " + snapShotCosts[rewindTo]);
                
                if (rewindsAllowed && snapShotCosts[rewindTo] != snapShotCosts[snapShotCosts.Count - 1])
                    Rewind(snapShotCosts.Count - rewindTo - 1, snapShotCosts[rewindTo]);
            }
            step = 0;
        }
        else
        {
            step++;
        }
    }

    //to string
    public string CostsToString()
    {
        string toReturn = "";
        for (int i = 0; i < snapShotCosts.Count; i++)
        {
            toReturn += snapShotCosts[i] + ", ";
        }
        return toReturn;
    }

    //cost of a path
    public float CalculatePathCost()
    {
        float cost = 0;
        for(int i = 0; i < path.Count - 1; i++)
        {
            cost += pathFinder.distance(path[i].mapPosition.GetPosition(), path[i + 1].mapPosition.GetPosition()); //distance between each node
            //Debug.Log(pathFinder.distance(path[i].mapPosition.GetPosition(), path[i + 1].mapPosition.GetPosition()));
        }
        //Debug.Log(cost);
        //Debug.Break();
        return cost;
    }

    //compares current cost to previous costs + cost of rewind
    public int ComparePreviousStates()
    {
        if (snapShotCosts.Count == 0)
            return -1;
        float toCompare = snapShotCosts[snapShotCosts.Count - 1];
        float difference = costsSoFar[costsSoFar.Count - 1];
        int index = 0;
        for (int i = 0; i < snapShotCosts.Count; i++)
        {
            float cost = snapShotCosts[i] + RewindCost();
            if (cost < toCompare + (difference - costsSoFar[i]))
            {
                toCompare = cost + RewindCost();
                difference = costsSoFar[i];
                index = i;
            }
        }

        if (toCompare == snapShotCosts[snapShotCosts.Count - 1])
            return -1;
        else
        {
            return index;
        }
    }
    
    //tell system to rewind to specific gamestate, adjust cost accordingly
    void Rewind(int snapshotsAgo, float cost)
    {
        path.Clear();
        snapShotCosts.Clear();
        costsSoFar.Clear();
        snapShotCosts.Add(cost);
        costsSoFar.Add(costSoFar);
        //Debug.Break();
        gameSystem.Rewind(snapshotsAgo-1);
        rewindsUsed++;
    }

    //calculate cost of current rewind
    float RewindCost()
    {
        return rewindCost * (rewindsUsed + 1);
    }

    // Move player to a given tile
    public void MoveToTile(GameTile tile)
    {
        Vector3 newPos = new Vector3(tile.transform.position.x, tile.transform.position.y);
        newPos.z = -1;
        transform.position = newPos;
        GameTile oldTile = map.GetTileAt(curMapPos);
        curMapPos = tile.mapPosition;
        oldTile.DeOccupy();
        tile.Occupy();
        costSoFar += pathFinder.distance(oldTile.mapPosition.GetPosition(), tile.mapPosition.GetPosition());
    }

    // Move player to a given map position
    public void MoveToMapPosition(MapPosition pos)
    {
        MoveToTile(map.GetTileAt(pos));
    }

    // Override for x y
    public void MoveToMapPosition(int x, int y)
    {
        MoveToTile(map.GetTileAt(x, y));
    }

    // Takes a snapshot of the current player state
    public PlayerSnapshot TakeSnapshot()
    {
        return new PlayerSnapshot(curMapPos, costSoFar);
    }

    // Loads a snapshot of a previous state
    public void LoadSnapshot(PlayerSnapshot snapshot)
    {
        //Debug.Log("rewinding... Cost before: " + costSoFar);
        MoveToMapPosition(snapshot.position);
        costSoFar = snapshot.costSoFar;
        //Debug.Log("Cost after: " + costSoFar);
    }

    void handleInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameTile potentialMove = map.GetTileAt(curMapPos.x + 1, curMapPos.y);
            if (potentialMove.isPassable())
            {
                MoveToTile(potentialMove);
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            GameTile potentialMove = map.GetTileAt(curMapPos.x - 1, curMapPos.y);
            if (potentialMove.isPassable())
            {
                MoveToTile(potentialMove);
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            GameTile potentialMove = map.GetTileAt(curMapPos.x, curMapPos.y + 1);
            if (potentialMove.isPassable())
            {
                MoveToTile(potentialMove);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GameTile potentialMove = map.GetTileAt(curMapPos.x, curMapPos.y - 1);
            if (potentialMove.isPassable())
            {
                MoveToTile(potentialMove);
            }
        }
    }
}
