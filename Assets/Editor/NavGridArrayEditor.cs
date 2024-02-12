using Codice.Client.BaseCommands.BranchExplorer;
using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Our class to handle our custom editor.
/// </summary>
[CustomEditor(typeof(NavGrid))]
public class NavGridArrayEditor : Editor
{


    /// <summary>
    /// Button to run when we're in the inspectorGUI
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draws the default inspector
        

        if (GUILayout.Button("Generate Grid"))
        {
            MazeGenerator mg = new MazeGenerator();
            NavGrid gridManager = (NavGrid)target;
            mg.ToggleAllWalkableBits(gridManager, false);
            mg.GenerateMaze(gridManager);
            gridManager.SaveData();
            
        }

    }

}

/// <summary>
/// Maze Generation Class
/// </summary>
public class MazeGenerator
{
    /// <summary>
    /// size of our x axis
    /// </summary>
    public int XSize = int.MaxValue;
    /// <summary>
    /// size of our z axis
    /// </summary>
    public int ZSize = int.MaxValue;
    /// <summary>
    /// container for our visited nodes
    /// </summary>
    private HashSet<NavGridNode>  visitedNodes;
    /// <summary>
    /// container for our unvisited nodes
    /// </summary>
    private List<NavGridNode> openNodes;
    /// <summary>
    /// So we can randomly switch neighbor checking bits
    /// </summary>
    private System.Random rand = new System.Random();


    /// <summary>
    /// Toggle all the bits based on our param
    /// </summary>
    /// <param name="gridManager"></param>
    /// <param name="walkable"></param>
    public void ToggleAllWalkableBits(NavGrid gridManager, bool walkable)
    {

        for (int x = 0;x < gridManager.GridXSize;x++) 
        {
            for(int z = 0;z < gridManager.GridZSize;z++) 
            {
                gridManager.navGridArray[x, z].isWalkable = walkable;
            }

        }

    }

    /// <summary>
    /// Do our maze generation 
    /// </summary>
    /// <param name="gridmanager"></param>
    public void GenerateMaze(NavGrid gridmanager)
    {
        ToggleAllWalkableBits(gridmanager, false);

        visitedNodes = new HashSet<NavGridNode>();

        // Randomly choose a cell to start
        int startX = rand.Next(0, (int)gridmanager.GridXSize);
        int startY = rand.Next(0, (int)gridmanager.GridZSize);

        // Carve the maze from the starting cell
        CarvePassagesFrom(startX, startY, gridmanager);
    }

    void CarvePassagesFrom(int x, int z, NavGrid gridmanager)
    {

        visitedNodes.Add(gridmanager.navGridArray[x, z]);

        // Randomly order the directions
        int[] dx = { 1, -1, 0, 0 };
        int[] dz = { 0, 0, 1, -1 };
        ShuffleDirections(dx, dz);

        gridmanager.navGridArray[x, z].isWalkable = true;
        // Explore neighboring cells
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int nz = z + dz[i];

            int nx2 = x + dx[i] * 2;
            int nz2 = z + dz[i] * 2;

            // Check bounds and if neighbor has been visited
            if (nx >= 0 && nz >= 0 && nx < gridmanager.GridXSize && nz < gridmanager.GridZSize && !visitedNodes.Contains(gridmanager.navGridArray[nx, nz]))
            {
                if (nx2  >= 0 && nz2 >= 0 && nx2 < gridmanager.GridXSize && nz2 < gridmanager.GridZSize && !visitedNodes.Contains(gridmanager.navGridArray[nx2, nz2]))
                {
                    CarvePassagesFrom(nx2, nz2, gridmanager);
                    gridmanager.navGridArray[nx, nz].isWalkable = true;
                }
                
            }
        }
    }

    /// <summary>
    /// Randomly change the order of the numbers
    /// </summary>
    /// <param name="dx"></param>
    /// <param name="dz"></param>
    void ShuffleDirections(int[] dx, int[] dz)
    {
        for (int i = 0; i < 4; i++)
        {
            int r = rand.Next(i, 4);
            int temp = dx[i];
            dx[i] = dx[r];
            dx[r] = temp;

            temp = dz[i];
            dz[i] = dz[r];
            dz[r] = temp;
        }
    }
}
