using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AStar : MonoBehaviour
{
    static int width = 50;
    static int height = 35;
    private Tile[,] tiles = new Tile[width,height];
    private const float dist = 1.0f;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] float frameDuration = 0.1f;
    [SerializeField] bool paused = true;
    [SerializeField] Button startButton;
    [SerializeField] Button clearButton;
    Tile startTile;
    Tile endTile;
    bool algoRunning = false;
    void Start()
    {
        if (tilePrefab != null) // Instantiate all tiles within width*height 
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tiles[i, j] = Instantiate(tilePrefab, new Vector3(i * dist, 0, j * dist), tilePrefab.transform.rotation).GetComponent<Tile>();
                    tiles[i,j].traversable = true;
                    tiles[i, j].iCoord = i; tiles[i, j].jCoord = j;
                    tiles[i, j].SetColour();
                }
            }
            // set start and end tiles and colour them
            startTile = tiles[1,1]; startTile.SetColour(Color.magenta);
            endTile = tiles[width-2,height-2]; endTile.SetColour(Color.blue);
        }
        startButton.onClick.AddListener(() => Run());
        clearButton.onClick.AddListener(() => Clear());
    }


    void Update()
    {
        
        if(paused) // if program is paused, allow changing of traversable state 
        {
            if (Input.GetMouseButton(0)) // LMB sets non-traversable
            {
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    try
                    {
                        Tile tile = hit.collider.gameObject.GetComponent<Tile>();
                        tile.traversable = false;
                        tile.SetColour();
                    }
                    catch { }
                }
            }
            if (Input.GetMouseButton(1)) // RMB reverts
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    try
                    {
                        Tile tile = hit.collider.gameObject.GetComponent<Tile>();
                        tile.traversable = true;
                        tile.SetColour();
                    }
                    catch { }
                }
            }
        } // make tiles traversable/not
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="curr"></param>
    /// <returns>A list of <paramref name="curr"/> tile's neighbors that are within the grid.</returns>
    List<Tile> GetNeighbors(Tile curr)
    {
        List<Tile> neighbors = new List<Tile>();
        for (int x = curr.iCoord - 1; x <= curr.iCoord + 1; x++)
        {
            if(x<0 || x>width-1) continue; // discard if out of bounds
            for (int y = curr.jCoord - 1; y <= curr.jCoord + 1; y++)
            {
                if(y<0 || y>height-1) continue; // discard if out of bounds

                // Exclude current tile
                if (!(x == curr.iCoord && y == curr.jCoord) && tiles[x, y] == true)
                {
                    neighbors.Add(tiles[x, y]);
                }
            }
        }
        return neighbors;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>The "cost distance" between <paramref name="a"/> and <paramref name="b"/> </returns>
    int GetDistance(Tile a, Tile b)
    {
        int distX = Mathf.Abs(a.iCoord - b.iCoord);
        int distY = Mathf.Abs(a.jCoord - b.jCoord);
        int smaller, larger;
        if (distX < distY)
        {
            smaller = distX; larger = distY;
        }
        else {larger = distX; smaller = distY; }

        // "smaller" is whichever is less, the horizontal or vertical distance
        // we have to take that many diagonal steps (multiply by 14, ~10*sqrt2)
        // after that we are on either the same horizontal or vertical level
        // then we need to take larger-smaller amount of straight steps
        return 14 *smaller + 10*(larger-smaller); 
    }


    void Run()
    {
        paused = !paused;
        startButton.GetComponentInChildren<Text>().text = paused ? "Start" : "Pause";
        if (paused ) { Time.timeScale = 0.0f; }
        else { Time.timeScale = 1.0f; }
        if(!algoRunning) { StartCoroutine(FindPath()); algoRunning=true; }
    }

    void Clear()
    {
        for(int i = 0;i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                tiles[i, j].traversable = true;
                tiles[i, j].SetColour();
                startTile.SetColour(Color.magenta);
                endTile.SetColour(Color.blue);
            }
        }
        
    }
    /// <summary>
    /// Actual path finding algorithm, delayed by frameDuration after each iteration
    /// </summary>
    IEnumerator FindPath()
    {
        algoRunning = true;
        List<Tile> openTiles = new List<Tile>();
        HashSet<Tile> closedTiles = new HashSet<Tile>();
        Tile currentTile;
        openTiles.Add(startTile); // add start tile as the first open tile
        while (openTiles.Count > 0) // while there are open tiles, loop
        {
            currentTile = openTiles[0];
            for(int i = 1; i < openTiles.Count; i++) // loop through all open tiles and find the one with smallest fCost (if tied, choose one with lower h)
            {
                if (openTiles[i].fCost < currentTile.fCost 
                    || (openTiles[i].fCost == currentTile.fCost && openTiles[i].hCost < currentTile.hCost))
                {
                    currentTile = openTiles[i];
                }
            }
            // "traverse" to selected tile
            // it's no longer open, add it to closedTiles (colour=red)
            openTiles.Remove(currentTile);
            closedTiles.Add(currentTile);
            if (currentTile != startTile && currentTile != endTile)
            {
                currentTile.SetColour(Color.red);
            }

            // if endTile was found, draw path and exit the loop and function
            if(currentTile == endTile)
            {
                DrawPath();
                break;
            }

            List<Tile> neighbors = GetNeighbors(currentTile);
            foreach (Tile neighbor in neighbors) // loop through all current tile's neighbors
            {
                if (!neighbor.traversable || closedTiles.Contains(neighbor)) { continue; } // discard not traversable ones and closed ones

                int costToNeighbor = currentTile.gCost + GetDistance(currentTile, neighbor); // calculate cost to get to this neighbor through current tile
                if (costToNeighbor < neighbor.gCost || !openTiles.Contains(neighbor)) // update its cost if necessary
                {
                    neighbor.gCost = costToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endTile);
                    neighbor.previous = currentTile; // mark current tile as the one through which we got to this neighbor (we use this to trace the final path)
                    neighbor.UpdateCosts(); // update visuals

                    if (!openTiles.Contains(neighbor)) // add neighbor to open tiles if not there already
                    {
                        openTiles.Add(neighbor);
                        neighbor.SetColour(Color.green);
                    }
                }
            }
            yield return new WaitForSeconds(frameDuration);
        }
        algoRunning = false;
    }

    void DrawPath()
    {
        Tile curr = endTile;
        //List<Tile> path = new List<Tile>();
        while(curr != startTile)
        {
            if (curr != endTile && curr != startTile)
            {
                curr.SetColour(Color.cyan);
            }
            curr = curr.previous;
        }
    }
}
