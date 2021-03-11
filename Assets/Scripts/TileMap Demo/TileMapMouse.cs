using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(TileMap))]
public class TileMapMouse : MonoBehaviour
{
    TileMap _tileMap;
    Vector3 currentTileCoord;
    public GameObject selectionCube;
    public LayerMask whatCanBeClickedOn;

    public NavMeshAgent player;

    private Color basiColor = Color.green;
    private Color hoverColor = Color.red;
    private Renderer mapRenderer;

    void Start() {
        _tileMap = GetComponent<TileMap>();
        mapRenderer = GetComponent<Renderer>();
        mapRenderer.material.color = basiColor;

        player = GameObject.Find("Player").GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, whatCanBeClickedOn)) 
        {
            HighlightTile(hitInfo);
        }
        else {
        }

        if (Input.GetMouseButtonDown(0))
        {
            // move unit to selected coordinates
            GetCurrentTileCoordinates(hitInfo);
            MovePlayer();
        }
    }

    void HighlightTile(RaycastHit hitInfo) 
    {
        GetCurrentTileCoordinates(hitInfo);
        // replaced this snippet with above
        // int x = Mathf.FloorToInt(hitInfo.point.x / _tileMap.tileSize);
        // int z = Mathf.FloorToInt(hitInfo.point.z / _tileMap.tileSize);
        // Debug.Log("Tile: " + x + ", " + z);
        //currentTileCoord.x = x;
        //currentTileCoord.z = z;
        selectionCube.transform.position = currentTileCoord;
    }

    void GetCurrentTileCoordinates(RaycastHit hitInfo) {
        currentTileCoord.x = Mathf.FloorToInt(hitInfo.point.x / _tileMap.tileSize);
        currentTileCoord.z = Mathf.FloorToInt(hitInfo.point.z / _tileMap.tileSize);
    }

    void MovePlayer() {
        float clickedX = currentTileCoord.x;
        float clickedZ = currentTileCoord.z;
        Debug.Log("Clicked Tile: " + currentTileCoord.x + ", " + currentTileCoord.z);
        player.SetDestination(new Vector3(clickedX, 0, clickedZ));
    }
}
