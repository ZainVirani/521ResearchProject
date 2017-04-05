using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapPattern { None, ZigZag, BottomBars, TopBars }

[System.Serializable]
public struct MapPosition
{
    public int x;
    public int y;

    public MapPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MapPosition))
            return false;
        MapPosition other = (MapPosition) obj;
        return Equals(other);
    }

    public bool Equals(MapPosition obj)
    {
        if (x == obj.x && y == obj.y)
            return true;
        return false;
    }

    public static bool operator ==(MapPosition m1, MapPosition m2)
    {
        return m1.Equals(m2);
    }

    public static bool operator !=(MapPosition m1, MapPosition m2)
    {
        return !m1.Equals(m2);
    }

    public Vector2 GetPosition()
    {
        return new Vector2(x, y);
    }
}

public struct MapSnapshot
{
    public bool[] dynamicTilesPassability;
    public bool[] dynamicTilesOccupation;

    public MapSnapshot(bool[] dynamicTilesPassability, bool[] dynamicTilesOccupation)
    {
        this.dynamicTilesPassability = dynamicTilesPassability;
        this.dynamicTilesOccupation = dynamicTilesOccupation;
    }
}

public class GameMapBehavior : MonoBehaviour {
    public int width;
    public int height;
    public MapPattern pattern; // Base pattern for the map before randomization
    public int numHoles; // Number of holes in randomization
    public MapPosition startPosition;
    public MapPosition goalPosition;
    public GameTile tilePrefab; // Tile prefab to use

    GameTile[,] mapData;
    List<GameTile> dynamicTiles = new List<GameTile>();

    private static System.Random rng = new System.Random();

    private void Awake()
    {
        mapData = new GameTile[width + 2, height + 2]; // +2 for borders
        initializeMap();
        RandomizeMap();
    }

    // Map location to world position
    public Vector3 MapToWorld(MapPosition mapLocation)
    {
        Vector3 loc = new Vector3(mapLocation.x + 1, mapLocation.y + 1, 0); // +1 cause of border
        return gameObject.transform.position + loc;
    }
    // Overload
    public Vector3 MapToWorld(int x, int y)
    {
        Vector3 loc = new Vector3(x + 1, y + 1, 0);
        return gameObject.transform.position + loc;
    }

    // Get tile at a given location
    public GameTile GetTileAt(int x, int y)
    {
        return mapData[x + 1, y + 1];
    }
    // Overload
    public GameTile GetTileAt(MapPosition pos)
    {
        if (pos.x >= width || pos.y >= height)
            return null;
        return mapData[pos.x + 1, pos.y + 1];
    }
    // Overload
    public GameTile GetTileAt(Vector2 pos)
    {
        return mapData[(int)pos.x + 1, (int)pos.y + 1];
    }

    // Randomizes the holes in the map
    public void RandomizeMap()
    {
        // Reset the map
        ResetMap();

        // Make a copy so when we make passable we can remove from the list
        List<GameTile> impassables = new List<GameTile>();
        foreach (GameTile gt in dynamicTiles)
        {
            if (!gt.isOccupied())
            {
                impassables.Add(gt);
            }
        }

        for (int i = 0; i < numHoles; i++)
        {
            // Check list isn't empty
            if (impassables.Count == 0)
                break;

            // Set a random tile to passable
            int ind = rng.Next(0, impassables.Count);
            impassables[ind].MakePassable();
            impassables.RemoveAt(ind);
        }
    }

    // Reset dynamics to impassable
    public void ResetMap()
    {
        foreach (GameTile gt in dynamicTiles)
        {
            gt.MakeImpassable();
        }
    }

    // Get a snapshot of the current state of the map
    public MapSnapshot TakeSnapshot()
    {
        bool[] passability = new bool[dynamicTiles.Count];
        bool[] occupation = new bool[dynamicTiles.Count];

        for (int i = 0; i < dynamicTiles.Count; i++)
        {
            passability[i] = dynamicTiles[i].isPassable();
            occupation[i] = dynamicTiles[i].isOccupied();
        }

        return new MapSnapshot(passability, occupation);
    }

    // Load a previous snapshot of the map state
    public void LoadSnapshot(MapSnapshot snapshot)
    {
        if (dynamicTiles.Count != snapshot.dynamicTilesOccupation.Length || dynamicTiles.Count != snapshot.dynamicTilesPassability.Length)
            throw new System.ArgumentException("Invalid Snapshot. Size does not match.");

        for (int i=0; i < dynamicTiles.Count; i++)
        {
            // Load passability
            if (snapshot.dynamicTilesPassability[i])
            {
                dynamicTiles[i].MakePassable();
            }
            else
            {
                dynamicTiles[i].MakeImpassable();
            }

            // Load occupation
            if (snapshot.dynamicTilesOccupation[i])
            {
                dynamicTiles[i].Occupy();
            }
            else
            {
                dynamicTiles[i].DeOccupy();
            }
        }
    }

    void initializeMap()
    {
        // Initialize tiles
        GameTile temp;
        for (int x = 0; x < width + 2; x++)
        {
            for (int y = 0; y < height + 2; y++)
            {
                // Instantiate
                // -1s are because 0,0 in game world is 1,1 in the array
                temp = Instantiate(tilePrefab, MapToWorld(new MapPosition(x-1, y-1)), Quaternion.identity);
                temp.mapPosition = new MapPosition(x - 1, y - 1);
                temp.transform.parent = transform;
                // Set border
                if (x == 0 || y == 0 || x == width + 1 || y == height + 1)
                    temp.MakeImpassable();
                // Store in data
                mapData[x, y] = temp;
            }
        }

        // Draw the pattern
        drawPattern();

        // Set start
        GameTile start = GetTileAt(startPosition);
        start.MakeStart();
        dynamicTiles.Remove(start);

        // Set goal
        GameTile goal = GetTileAt(goalPosition);
        goal.MakeGoal();
        dynamicTiles.Remove(goal);
    }

    // Draws the selected pattern
    void drawPattern()
    {
        switch(pattern)
        {
            case MapPattern.ZigZag:
                drawZigZagPattern();
                break;
            case MapPattern.BottomBars:
                drawBottomBarsPattern();
                break;
            case MapPattern.TopBars:
                drawTopBarsPattern();
                break;
            default:
                break;
        }
    }

    // Draws the zig zag pattern into the board
    void drawZigZagPattern()
    {
        for (int x = 1; x < width; x += 2)
        {
            // Make everything in the column impassable
            for (int y = 0; y < height; y++)
            {
                GameTile tile = GetTileAt(x, y);
                tile.MakeImpassable();
                dynamicTiles.Add(tile);
            }
            // Open the end
            if (x%4 == 1)
            {
                GameTile tile = GetTileAt(x, height - 1);
                tile.MakePassable();
                dynamicTiles.Remove(tile);
            } else
            {
                GameTile tile = GetTileAt(x, 0);
                tile.MakePassable();
                dynamicTiles.Remove(tile);
            }
        }
    }

    void drawBottomBarsPattern()
    {
        for (int x = 1; x < width; x += 2)
        {
            GameTile tile;
            // Make everything in the column impassable
            for (int y = 0; y < height; y++)
            {
                tile = GetTileAt(x, y);
                tile.MakeImpassable();
                dynamicTiles.Add(tile);
            }
            // Open the end
            tile = GetTileAt(x, height - 1);
            tile.MakePassable();
            dynamicTiles.Remove(tile);
        }
    }

    void drawTopBarsPattern()
    {
        for (int x = 1; x < width; x += 2)
        {
            GameTile tile;
            // Make everything in the column impassable
            for (int y = 0; y < height; y++)
            {
                tile = GetTileAt(x, y);
                tile.MakeImpassable();
                dynamicTiles.Add(tile);
            }
            // Open the end
            tile = GetTileAt(x, 0);
            tile.MakePassable();
            dynamicTiles.Remove(tile);
        }
    }
}
