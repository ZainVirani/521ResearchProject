using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerSnapshot
{
    public MapPosition position;

    public PlayerSnapshot(MapPosition position)
    {
        this.position = position;
    }
}

public class PlayerController : MonoBehaviour {
    public GameMapBehavior map;

    private MapPosition curMapPos;

	// Use this for initialization
	void Start () {
        MoveToMapPosition(map.startPosition);
	}

    private void Update()
    {
        handleInput();
    }

    // Move player to a given tile
    public void MoveToTile(GameTile tile)
    {
        Vector3 newPos = new Vector3(tile.transform.position.x, tile.transform.position.y);
        newPos.z = -1;
        transform.position = newPos;
        curMapPos = tile.mapPosition;
        GameTile oldTile = map.GetTileAt(curMapPos);
        if (oldTile)
            oldTile.DeOccupy();
        tile.Occupy();
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
        return new PlayerSnapshot(curMapPos);
    }

    // Loads a snapshot of a previous state
    public void LoadSnapshot(PlayerSnapshot snapshot)
    {
        MoveToMapPosition(snapshot.position);
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
