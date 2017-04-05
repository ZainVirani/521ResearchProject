using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour {
    public Material passableMaterial;
    public Material impassableMaterial;
    public Material startMaterial;
    public Material goalMaterial;
    public float G;
    public float H;
    public float F;
    public GameTile parent;

    [HideInInspector]
    public MapPosition mapPosition;

    private bool passable = true;
    private bool occupied = false;
    private new Renderer renderer; // Shadows Component.renderer because it's not supported

    private void Awake()
    {
        renderer = gameObject.GetComponent<Renderer>();
        MakePassable();
    }

    // Make tile passable
    public void MakePassable()
    {
        passable = true;
        renderer.material = passableMaterial;
    }

    // Make tile impassable
    public void MakeImpassable()
    {
        passable = false;
        renderer.material = impassableMaterial;
    }

    // Make tile impassable
    public void MakeStart()
    {
        passable = true;
        renderer.material = startMaterial;
    }

    // Make tile impassable
    public void MakeGoal()
    {
        passable = true;
        renderer.material = goalMaterial;
    }

    // Get passability
    public bool isPassable()
    {
        return passable;
    }

    // Occupy the space
    public void Occupy()
    {
        occupied = true;
    }

    // De occupy the space
    public void DeOccupy()
    {
        occupied = false;
    }

    // Check occupation
    public bool isOccupied()
    {
        return occupied;
    }
}
