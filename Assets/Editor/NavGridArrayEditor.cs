using Codice.Client.BaseCommands.BranchExplorer;
using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(NavGrid))]
public class NavGridArrayEditor : Editor
{


    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draws the default inspector
        

         //   if (GUILayout.Button("Generate Grid"))
        {
            // JMROptional: Implement functionality to generate or regenerate the grid in editor
        }

    }


    /// <summary>
    /// Handle interaction with the 
    /// JMR: This is not efficient to be iterating through everything.  Long term look at adding colliders and using raycasting to detect which grid was hit as a possible more efficient solution.
    /// </summary>
    protected virtual void OnSceneGUI()
    {

        NavGrid gridManager = (NavGrid)target;

        Vector3 planeScale = gridManager.transform.localScale;
        Vector3 startPosition = gridManager.transform.position - new Vector3(gridManager.PlaneXSize  / 2 , -100, gridManager.PlaneZSize / 2);

        float XVal = gridManager.PlaneXSize / gridManager.GridXSize;
        float ZVal = gridManager.PlaneZSize / gridManager.GridZSize;

        float cellSize = .925f;
        float pickSize = 1f;// cellSize * 0.5f;

        Handles.color = Color.green;

        //Calculate our offset
        Vector3 gridOffset = new Vector3(cellSize / 2, 0, cellSize / 2);
        
        
        for (int x = 0; x < gridManager.GridXSize; x++)
        {
            for (int z = 0; z < gridManager.GridZSize; z++)
            {

                // Calculate the center position of each cell
                Vector3 cellCenter = startPosition + new Vector3((x * XVal) + XVal, 0, (z * ZVal) + ZVal );

                //Determine our cube size
                Vector3 cubeSize = new Vector3(XVal, 1f, ZVal);


                Vector3 cellPosition = gridManager.transform.transform.position +
                     new Vector3(x * cellSize, 0, z * cellSize) + gridOffset;

                if (Handles.Button(cellCenter, Quaternion.LookRotation(Vector3.up), XVal/2, XVal/2, Handles.RectangleHandleCap))
                {
                    gridManager.navGridArray[x, z].isWalkable = !gridManager.navGridArray[x, z].isWalkable;
                    // Optionally, you might want to immediately reflect this change in the editor
                    EditorUtility.SetDirty(target); // Mark the target object as dirty to ensure changes are saved
                    gridManager.SaveData();
                }
            }
        }
 
    }
}
