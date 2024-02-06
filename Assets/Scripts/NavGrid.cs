using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using UnityEditor;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 
/// </summary>
[System.Serializable]
public struct NavGridNode
{
    /// <summary>
    /// We're tracking the parent X Index because we used a struct so we're tracking parent by it's grid location
    /// </summary>
    public int parentNodeX;
    /// <summary>
    /// We're tracking the parent Z Index because we used a struct so we're tracking parent by it's grid location
    /// </summary>
    public int parentNodeZ;
    /// <summary>
    /// 
    /// </summary>
    public int gCost;
    /// <summary>
    /// 
    /// </summary>
    public int hCost;
    /// <summary>
    /// Is the node something we can traverse
    /// </summary>
    public bool isWalkable;
    /// <summary>
    /// 
    /// </summary>
    public Vector3 worldPosition;

    /// <summary>
    /// 
    /// </summary>
    public Vector3 localPosition;
    /// <summary>
    /// 
    /// </summary>
    public int gridX;
    /// <summary>
    /// 
    /// </summary>
    public int gridZ;

    public int FCost
    {  get { return gCost + hCost; } }

    public NavGridNode(bool iswalkable, Vector3 localposition, Vector3 worldposition, int gridx, int gridz)
    { 
        isWalkable = iswalkable;
        localPosition = localposition;
        worldPosition = worldposition;
        gridX = gridx;
        gridZ = gridz;
        gCost = int.MaxValue;
        hCost = int.MaxValue;
        parentNodeX = 0;
        parentNodeZ = 0;

    }
}
public class NavGrid : MonoBehaviour
{


    /// <summary>
    /// 
    /// </summary>
    public Transform player;

    /// <summary>
    /// 
    /// </summary>
    public Vector2 gridWorldSize;

    #region properties
    /// <summary>
    /// Our prefab to use for our walkable areas
    /// </summary>
    public GameObject walkableIndicatorPrefab;

    /// <summary>
    /// Our prefab to use to generate our obstacles 
    /// </summary>
    public GameObject nonWalkableIndicatorPrefab;

    /// <summary>
    /// Local copy of the camera
    /// </summary>
    private Camera cam;

    /// <summary>
    /// 
    /// </summary>
    private Vector3 lastTransformScale;

 
    /// <summary>
    /// path and filename for data storage
    /// </summary>
    private string filePath = string.Empty;


    /// <summary>
    /// plane scales by a factor of 10
    /// </summary>
    const int PlaneScaleNumber = 10;

    /// <summary>
    /// The Plane we're going to be using
    /// </summary>
    public GameObject navigationPlane;

    /// <summary>
    /// Size value for the X axis of the surface that the grid is overlayed on
    /// </summary>
    private float gridXSize;

    public float GridXSize
    {
        get { return gridXSize; }
    }

    /// <summary>
    /// 
    /// </summary>
    private float planeXSize;

    /// <summary>
    /// Read only property for Plan X size
    /// </summary>
    public float PlaneXSize
    {
        get { return planeXSize; }
    }

    /// <summary>
    /// 
    /// </summary>
    private int xPlaneScale;

    /// <summary>
    /// Size value for the Y axis of the surface that the grid is overlayed on
    /// </summary>
    private float gridZSize;

    public float GridZSize
    {
        get { return gridZSize; }
    }
    /// <summary>
    /// 
    /// </summary>
    private float planeZSize;

    /// <summary>
    /// Read only property for Plane Z size
    /// </summary>
    public float PlaneZSize
    {
        get { return planeZSize; }
    }

    /// <summary>
    /// 
    /// </summary>
    private int zPlaneScale;

    /// <summary>
    /// GraphView Node Object for editor
    /// </summary>
    private Node[,] visualGridArray;

    [SerializeField]
    public NavGridNode[,] navGridArray;

    #endregion

    #region editor
    /// <summary>
    /// Let's draw our grid lines
    /// </summary>
    private void OnDrawGizmos()
    {
        if (null == navGridArray)
        {
            LoadGrid(filePath);
        }
        if (transform.localScale != lastTransformScale)
        {

            updateScale();
        }

        Vector3 planeScale = transform.localScale;
        Vector3 startPosition = transform.position - new Vector3(planeXSize / 2, 0, planeZSize / 2);

        float XVal = planeXSize / gridXSize;
        float ZVal = planeZSize / gridZSize;

        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {
                // Calculate the center position of each cell
                Vector3 cellCenter = startPosition + new Vector3((x * XVal) + XVal / 2, 0, (z * ZVal) + ZVal / 2);

                //Determine our cube size
                Vector3 cubeSize = new Vector3(XVal, 1f, ZVal);

                //Draw our solid color cube based on walkable or not
                Gizmos.color = navGridArray[x, z].isWalkable ? Color.green : Color.red;
                // Uncomment the line below to use DrawCube
                Gizmos.DrawCube(cellCenter, cubeSize);

                //Draw our outline in white
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(cellCenter, cubeSize);


            }
        }

    }

    #endregion

    /// <summary>
    /// Update our Scale data for when someone is changing parameters in the editor
    /// </summary>
    private void updateScaleData()
    {
        lastTransformScale = transform.localScale;
        gridXSize = transform.localScale.x;
        gridZSize = transform.localScale.z;

        planeXSize = gridXSize * PlaneScaleNumber;
        planeZSize = gridZSize * PlaneScaleNumber;

        if (null == navGridArray)
            return;

        NavGridNode[,] NodeGridArrayTMP = navGridArray;
        // NavGridNode navGridNodeTMP = new NavGridNode();

        int xlength = navGridArray.GetLength(0);
        int zLength = navGridArray.GetLength(1);

        navGridArray = new NavGridNode[(int)gridXSize, (int)gridZSize];

        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {
                if (x < xlength && z < zLength)
                {
                    navGridArray[x, z] = NodeGridArrayTMP[x, z];
                }
                else
                {
                    navGridArray[x, z].gridX = x;
                    navGridArray[x, z].gridZ = z;
                    navGridArray[x, z].localPosition = new Vector3(transform.position.x, 0, transform.position.z);
                    navGridArray[x, z].worldPosition = new Vector3(transform.position.x, 0, transform.position.z);//new Vector3(x - GridXSize / 2, 0, z - GridZSize / 2);
                    navGridArray[x, z].isWalkable = true;
                }
            }
        }

    }

    public void ResetPathFindingData()
    {
        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {
                navGridArray[x, z].gCost = int.MaxValue;
                navGridArray[x, z].hCost = int.MaxValue;
                navGridArray[x, z].parentNodeX = 0;
                navGridArray[x, z].parentNodeZ = 0;

            }
        }
    }

    /// <summary>
    /// If we don't have a grid we want to generate a default one
    /// </summary>
    private void generateDefaultNavGridArrayData()
    {
        navGridArray = new NavGridNode[(int)gridXSize, (int)gridZSize];
        NavGridNode navGridNodeTMP = new NavGridNode();

        for (int xCntr = 0; xCntr < navGridArray.GetLength(0); xCntr++)
        {
            for (int zCntr = 0; zCntr < navGridArray.GetLength(1); zCntr++)
            {
                navGridNodeTMP.gridX = xCntr;
                navGridNodeTMP.gridZ = zCntr;

                navGridNodeTMP.worldPosition = new Vector3(xCntr - GridXSize / 2, 0, zCntr - GridZSize / 2);
                navGridNodeTMP.isWalkable = true;
                navGridArray[xCntr, zCntr] = navGridNodeTMP;
            }

        }
        SaveGrid(filePath);
    }

    /// <summary>
    /// 
    /// </summary>
    private void updateScale()
    {

        updateScaleData();
        SaveGrid(filePath);

    }

    private void OnValidate()
    {
        updateScale();
    }

    public void Awake()
    {
        if (string.Empty == filePath)
        {
            filePath = Path.Combine(Application.persistentDataPath, "gridSave.bin");
        }
        LoadGrid(filePath);
        updateScale();
        PopulateObstacles();
    }
    public void Update()
    {

    }

    public void Start()
    {
        if (string.Empty == filePath)
        {
            filePath = Path.Combine(Application.persistentDataPath, "gridSave.bin");
        }
        updateScale();
        //  PopulateObstacles();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="worldPoint"></param>
    public void ToggleEditorCellState(Vector3 worldPoint)
    {
        // Convert the worldPoint to grid coordinates
        int x = Mathf.FloorToInt(worldPoint.x / 1);
        int z = Mathf.FloorToInt(worldPoint.z / 1); // Use z for grid y-coordinate in a 3D project

        // Check if the coordinates are within the grid bounds
        if (x >= 0 && x < gridXSize && z >= 0 && z < gridZSize)
        {
            // Toggle the walkability state
            navGridArray[x, z].isWalkable = !navGridArray[x, z].isWalkable;

            // Optionally, trigger an update to the visual representation of the grid
            //UpdateGridVisuals(x, z);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void PopulateObstacles()
    {
        if (transform.localScale != lastTransformScale)
        {

            updateScale();
        }

        Vector3 planeScale = transform.localScale;
        Vector3 startPosition = transform.position - new Vector3(PlaneXSize / 2, 1, PlaneZSize / 2);

        float XVal = PlaneXSize / GridXSize;
        float ZVal = PlaneZSize / GridZSize;


        //Vector3 startPosition = transform.position - new Vector3(planeXSize / 2, 0, planeZSize / 2);

        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {


                // Calculate the center position of each cell
                Vector3 cellCenter = startPosition + new Vector3((x * XVal) + XVal / 2, 1, (z * ZVal) + ZVal / 2);

                //Determine our cube size
                Vector3 cubeSize = new Vector3(XVal, 1f, ZVal);

                var item = navGridArray[x, z];
                Vector3 center = item.worldPosition + transform.position;

                Vector3 pos = new Vector3(x * 1 + transform.position.x, 0, z * 1 + transform.position.z);

                if (!item.isWalkable)
                {
                    GameObject newObstacle = Instantiate(nonWalkableIndicatorPrefab, cellCenter, Quaternion.identity, transform);
                    newObstacle.tag = "Obstacle";
                    //newObstacle.layer = LayerMask.NameToLayer("Ignore Raycast");


                }

            }
        }

    }

    public NavGrid()
    {


    }

    #region Data


    /// <summary>
    /// Function to figure out locations based on the index
    /// </summary>
    /// <param name="xIndex">X Index</param>
    /// <param name="zIndex">Z Index</param>
    /// <returns>Target Destination</returns>
    public Vector3 GetGridNodeLocation(int xIndex, int zIndex)
    {
        Vector3 targetDestination = new Vector3();
        float Xcellsize = PlaneXSize / GridXSize;
        float Zcellsize = PlaneZSize / GridZSize;

        //We're calculating where the grid location is and adjusting to the midddle of the cell
        float xAdjustment = navGridArray[xIndex, zIndex].gridX * Xcellsize - (PlaneXSize / 2) + (GridXSize / 4);
        float zAdjustment = navGridArray[xIndex, zIndex].gridZ * Zcellsize - (PlaneZSize / 2) + (GridZSize / 4);

        targetDestination = new Vector3(xAdjustment, 0, zAdjustment);
        return targetDestination;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="nextNode"></param>
    /// <param name="smoothingFactor"></param>
    /// <returns></returns>
    Vector3 SmoothBetweenTwoNodes(NavGridNode currentNode, NavGridNode nextNode, float smoothingFactor)
    {
        Vector3 startPosition = GetGridNodeLocation(currentNode.gridX, currentNode.gridZ);
        Vector3 endPosition = GetGridNodeLocation(nextNode.gridX, nextNode.gridZ);

        // Linearly interpolate between the start and end positions based on the smoothing factor
        // Smoothing factor determines how far along the line between the two nodes the smoothed point is.
        // A smoothing factor of 0.5 would return the midpoint between the two nodes.
        Vector3 smoothedPoint = Vector3.Lerp(startPosition, endPosition, smoothingFactor);

        return smoothedPoint;
    }

    /// <summary>
    /// Can see function for the smoothing
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    bool CanSee(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 direction = endPosition - startPosition;
        float distance = 60;

        // Optionally, ignore certain layers by adjusting the layerMask parameter
        int layerMask = Physics.DefaultRaycastLayers;

        RaycastHit hit;

        // Cast a ray from start to end position, checking for collisions
        if (Physics.Raycast(startPosition, direction,out hit, distance, layerMask))
        {
            // Hit something, so direct path is obstructed
            if ( hit.collider.CompareTag("Obstacle"))
                return false;
        }

        // No hit, direct path is clear
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="originalPath"></param>
    /// <returns></returns>
    public NavGridNode[] SmoothPath(NavGridNode[] originalPath)
    {
        if (originalPath == null || originalPath.Length < 2)
            return originalPath; // No smoothing needed for extremely short paths

        List<NavGridNode> smoothedPath = new List<NavGridNode>();
        smoothedPath.Add(originalPath[0]); // Always add the starting node

        int currentIndex = 0;
        while (currentIndex < originalPath.Length - 1)
        {
            int lookaheadIndex = currentIndex + 1;

            while (lookaheadIndex < originalPath.Length)
            {
                Vector3 currentPos = GetGridNodeLocation(originalPath[currentIndex].gridX, originalPath[currentIndex].gridZ);
                Vector3 lookaheadPos = GetGridNodeLocation(originalPath[lookaheadIndex].gridX, originalPath[lookaheadIndex].gridZ);

                // If cannot see the lookahead node or it's the last node, break
                if (!CanSee(currentPos, lookaheadPos) || lookaheadIndex == originalPath.Length - 1)
                {
                    if (!CanSee(currentPos, lookaheadPos))
                    {
                        lookaheadIndex--;
                    }

                    // To handle the case when all nodes are directly visible from the current node
                    if (lookaheadIndex == currentIndex)
                    {
                        lookaheadIndex++;
                    }

                    smoothedPath.Add(originalPath[lookaheadIndex]);
                    currentIndex = lookaheadIndex; // Move currentIndex to the last added node's position
                    break; // Exit the lookahead loop
                }

                lookaheadIndex++;
            }
        }

        // Ensure the last node is added, checking to avoid duplicates
        NavGridNode lastNode = originalPath[originalPath.Length - 1];
        if (smoothedPath[smoothedPath.Count - 1].gridX != lastNode.gridX && smoothedPath[smoothedPath.Count - 1].gridZ != lastNode.gridZ)
        {
            smoothedPath.Add(lastNode);
        }

        return smoothedPath.ToArray();
    }


    /// <summary>
    /// Given the current and desired location, return a path to the destination
    /// </summary>
    public NavGridNode[] GetPath(Vector3 origin, Vector3 destination)
    {

        NavGridNode[] currentPath = Array.Empty<NavGridNode>();
        NavGridNode startNode = NodeFromWorldPoint(origin);
        startNode.gCost = 0;
        NavGridNode targetNode = NodeFromWorldPoint(destination);
        startNode.hCost = GetDistance(startNode, targetNode);

        List<NavGridNode> openNodes = new List<NavGridNode>();
        HashSet<NavGridNode> closedNodes = new HashSet<NavGridNode>();
        openNodes.Add(startNode);

        while (openNodes.Count > 0)
        {
            NavGridNode currentNode = openNodes[0];
            for (int i = 1; i < openNodes.Count; i++)
            {
                if (openNodes[i].FCost < currentNode.FCost || openNodes[i].FCost == currentNode.FCost && openNodes[i].hCost < currentNode.hCost)
                {
                    currentNode = openNodes[i];
                }
            }

            //pull the one we're processing from open and add it to closed
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            //Check to see if we're the target node
            if (currentNode.gridZ == targetNode.gridZ && currentNode.gridX == targetNode.gridX)
            {
                //update the target node with our distance and parent info
                targetNode.parentNodeX = currentNode.parentNodeX;
                targetNode.parentNodeZ = currentNode.parentNodeZ;

                //Yay we found it, now return our path back
                return RetracePath(startNode, targetNode);
            }

            //Check our neighbors
            NavGridNode[] neighbors = GetNeighbors(currentNode);
            for (int i = 0; i < neighbors.Length; i++)
            {
                NavGridNode neighbor = neighbors[i];
                if (!neighbor.isWalkable || closedNodes.Contains(neighbor))
                {
                    continue;
                }

                //figure out our distance
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost)
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parentNodeX = currentNode.gridX;
                    neighbor.parentNodeZ = currentNode.gridZ;
                    navGridArray[neighbor.gridX, neighbor.gridZ].gCost = neighbor.gCost;
                    navGridArray[neighbor.gridX, neighbor.gridZ].hCost = neighbor.hCost;
                    navGridArray[neighbor.gridX, neighbor.gridZ].parentNodeX = currentNode.gridX;
                    navGridArray[neighbor.gridX, neighbor.gridZ].parentNodeZ = currentNode.gridZ;
                    if (!openNodes.Contains(neighbor))
                        openNodes.Add(neighbor);
                }
            }
        }

        return currentPath;
    }
    /// <summary>
    /// Walk back through our path parents to recreate our path
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    NavGridNode[] RetracePath(NavGridNode startNode, NavGridNode endNode)
    {
        List<NavGridNode> curList = new List<NavGridNode>();
        NavGridNode currentNode = endNode;
 
        while(currentNode.gridZ != startNode.gridZ || currentNode.gridX != startNode.gridX)
        {
            curList.Add(currentNode);
            currentNode = navGridArray[currentNode.parentNodeX, currentNode.parentNodeZ];
        }
        curList.Reverse();
        return curList.ToArray();
    }

    /// <summary>
    /// Get the distance between two nodes
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    int GetDistance(NavGridNode nodeA, NavGridNode nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        if (distX > distZ)
            return 14 * distZ + 10 * (distX - distZ);
        return 14 * distX + 10 * (distZ - distX);
    
    }

    /// <summary>
    /// Grab an array of neighbors to a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public NavGridNode[] GetNeighbors(NavGridNode node)
    {

        int numNeighbors = GetNumberOfNeighbors(node);
        int neighborCounter = 0;
        NavGridNode[] neighbors = new NavGridNode[GetNumberOfNeighbors(node)];
                      
        //Check 1 value to the left, same column, and column to the right
        for (int x = -1; x <= 1; x++)
        {
            //Check 1 row above, same row, and next row
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                //if we're greater than 0 and less then the max size row or column wise then we have a neighbor
                if (checkX >= 0 && checkX < gridXSize && checkZ >= 0 && checkZ < gridZSize)
                {
                    if (neighborCounter < numNeighbors)
                    {
                        neighbors[neighborCounter] = navGridArray[checkX, checkZ];
                        neighborCounter++;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to write more neighbors than we should have");
                    }
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Grab the number of neighbors for allocating array
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public int GetNumberOfNeighbors(NavGridNode node)
    {
        int cntr = 0;
        //Check 1 value to the left, same column and column to the right
        for (int x = -1; x <= 1; x++)
        {
            //Check 1 row above, same row, and next row
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                //if we're greater than 0 and less then the max size row or column wise then we have a neighbor
                if (checkX >= 0 && checkX < gridXSize && checkZ >= 0 && checkZ < gridZSize)
                {
                    cntr++;
                }
            }
        }
        return cntr;
    }

    /// <summary>
    /// Public save method
    /// </summary>
    public void SaveData()
    {
        SaveGrid(filePath);
    }

    /// <summary>
    /// Save our grid data to a binary file
    /// </summary>
    /// <param name="saveFilePath"></param>
    private void SaveGrid(String saveFilePath)
    {
        if(null == navGridArray)
        {
            return;
        }

        Debug.LogWarning("SaveGridEnter");
        if(string.Empty == saveFilePath)
        {
            saveFilePath = filePath = Path.Combine(Application.persistentDataPath, "gridSave.bin");
        }
        using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // Write the dimensions of the array
                writer.Write(navGridArray.GetLength(0)); // Number of rows
                writer.Write(navGridArray.GetLength(1)); // Number of columns

                // Iterate over the array and write each item
                for (int i = 0; i < navGridArray.GetLength(0); i++)
                {
                    for (int j = 0; j < navGridArray.GetLength(1); j++)
                    {
                        writer.Write(navGridArray[i, j].isWalkable);
                        writer.Write(navGridArray[i, j].gridX);
                        writer.Write(navGridArray[i, j].gridZ);
                        writer.Write(navGridArray[i, j].localPosition.x);
                        writer.Write(navGridArray[i, j].localPosition.y);
                        writer.Write(navGridArray[i, j].localPosition.z);
                        writer.Write(navGridArray[i, j].worldPosition.x);
                        writer.Write(navGridArray[i, j].worldPosition.y);
                        writer.Write(navGridArray[i, j].worldPosition.z);
                    }
                }
            }
        }
        Debug.LogWarning("SaveGridExit");
    }

    /// <summary>
    /// Load our grid from binary file
    /// </summary>
    /// <param name="loadFilePath"></param>
    private void LoadGrid(String loadFilePath)
    {
        Debug.LogWarning("LoadGridEnter");
        if (string.Empty == loadFilePath)
        {
            loadFilePath = Path.Combine(Application.persistentDataPath, "gridSave.bin");
        }
        if (!File.Exists(loadFilePath))
        {
            generateDefaultNavGridArrayData();
            Debug.LogError("Save file not found in " + loadFilePath);
            return;
        }

        using (var stream = new FileStream(loadFilePath, FileMode.Open, FileAccess.ReadWrite,FileShare.None))
        using (var reader = new BinaryReader(stream))
        {
            int rows = reader.ReadInt32();
            int cols = reader.ReadInt32();

            navGridArray = new NavGridNode[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    bool isWalkable = reader.ReadBoolean();
                    int gridX = reader.ReadInt32();
                    int gridZ = reader.ReadInt32();
                    float xLocal = reader.ReadSingle();
                    float yLocal = reader.ReadSingle();
                    float zLocal = reader.ReadSingle();
                    float xWorld = reader.ReadSingle();
                    float yWorld = reader.ReadSingle();
                    float zWorld = reader.ReadSingle();
                    Vector3 localPosition = new Vector3(xLocal, yLocal, zLocal);
                    Vector3 WorldPosition = new Vector3(xWorld, yWorld, zWorld);

                    navGridArray[i, j] = new NavGridNode(isWalkable, localPosition, WorldPosition, gridX, gridZ);
                }
            }

           
        }
        Debug.LogWarning("LoadGridExit");
    }

    public NavGridNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float XVal = PlaneXSize / GridXSize;
        float ZVal = PlaneZSize / GridZSize;
        //shift everything positive.
        //JMRBug We're assuming that our plane is laid out with our center at zero.  We should check for alignment in the future
        int xLoc = (int)((worldPosition.x + (PlaneXSize / 2)) / XVal);
        int zLoc = (int)((worldPosition.z + (PlaneZSize / 2)) / ZVal);
   
        return navGridArray[xLoc, zLoc];
              
    }

    #endregion
}
