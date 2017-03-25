using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Running, Rewinding, Paused }

public struct GameSnapshot
{
    public MapSnapshot mapState;
    public PlayerSnapshot playerState;

    public GameSnapshot(MapSnapshot mapState, PlayerSnapshot playerState)
    {
        this.mapState = mapState;
        this.playerState = playerState;
    }
}

public class GameSystem : MonoBehaviour {

    public static GameSystem instance;

    //PUBLIC

    [Tooltip("How many snapshots to store")]
    public int maxSnapshots;
    [Tooltip("Time in seconds between snapshots")]
    public float snapshotPeriod;
    [Tooltip("Rewind speed")]
    public float rewindSpeed = 1;
    public PlayerController player;
    public GameMapBehavior gamemap;
    public TimeFlowUIController rewindStateUI;

    [HideInInspector]
    public GameState state = GameState.Paused;

    //PRIVATE
    float timer = 0;
    FiniteList<GameSnapshot> snapshots;
    int rewindTarget;
    int curRewindState;
    float rewindTimer = 0;
    float rewindPeriod;

    private void Awake()
    {
        instance = this;
        snapshots = new FiniteList<GameSnapshot>(maxSnapshots);
        // mul by snapshot period so if snapshots are taken frequently, the speed should be faster to compensate
        rewindPeriod = 5 * 1 / (rewindSpeed/snapshotPeriod); // MAGIC NUMBER 5. Used to represent the max period.
    }

	// Use this for initialization
	void Start () {
        state = GameState.Running;
        timer = snapshotPeriod; //So we snapshot time 0 on the first update after everything is init
    }
	
	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case GameState.Running:
                updateOnRunning();
                break;
            case GameState.Rewinding:
                updateOnRewinding();
                break;
            default:
                break;
        }
	}

    // Take a snapshot of the current game state
    public GameSnapshot TakeSnapshot()
    {
        return new GameSnapshot(gamemap.TakeSnapshot(), player.TakeSnapshot());
    }

    // Loads a snapshot of a previous game state
    public void LoadSnapshot(GameSnapshot snapshot)
    {
        gamemap.LoadSnapshot(snapshot.mapState);
        player.LoadSnapshot(snapshot.playerState);
    }

    // Save and store a snapshot of a game state
    public void SaveSnapShot(GameSnapshot snapshot)
    {
        snapshots.AddFirst(snapshot);
        Debug.Log("Snapshot Saved!");
    }

    // Rewind to a given state
    public void Rewind(int snapshotsAgo)
    {
        if (snapshotsAgo >= maxSnapshots)
            throw new System.ArgumentOutOfRangeException("There aren't enough snapshots stored to access that time.");

        rewindTarget = snapshotsAgo;
        curRewindState = 0;
        state = GameState.Rewinding;
        rewindStateUI.SetBackward();
    }

    // Update function for when the game is in a running state
    void updateOnRunning()
    {
        checkSnapshotTimer();
        handleInput();
    }

    // Update function for when game is in a rewinding state
    void updateOnRewinding()
    {
        checkRewindTimer();
    }

    // Handle any input from the keyboard
    void handleInput()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                if (i < maxSnapshots)
                {
                    Rewind(i);
                }
            }
        }
    }

    // Checks the snapshot timer and takes a snapshot if needed
    void checkSnapshotTimer()
    {
        timer += Time.deltaTime;
        if (timer >= snapshotPeriod)
        {
            GameSnapshot state = TakeSnapshot();
            SaveSnapShot(state);
            timer = 0;
        }
    }

    // Checks the rewind timer and rewinds to next state if needed
    void checkRewindTimer()
    {
        rewindTimer += Time.deltaTime;
        Debug.Log("rewind: " + rewindTimer + " | period: " + rewindPeriod);
        if (rewindTimer >= rewindPeriod)
        {
            rewindTimer = 0;
            curRewindState++;
            LoadSnapshot(snapshots.GetAtPosition(curRewindState));
            if (curRewindState == rewindTarget)
            {
                state = GameState.Running;
                rewindStateUI.SetForward();
            }
        }
    }
}
