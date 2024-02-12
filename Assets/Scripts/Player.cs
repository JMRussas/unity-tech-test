using System;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Player controller class
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Our animation object
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Our pathing data object
    /// </summary>
    private NavGridNode[] _currentPath = Array.Empty<NavGridNode>();
    /// <summary>
    /// Keeping track of our path index
    /// </summary>
    private int _currentPathIndex = 0;
    
    /// <summary>
    /// our grid game object
    /// </summary>
    [SerializeField]
    private NavGrid _grid;

    /// <summary>
    /// Our obstacle data
    /// </summary>
    private NavGridNode[,] _navGridArray;

    /// <summary>
    /// Movement speed setting
    /// </summary>
    [SerializeField]
    private float _speed = 10.0f;

    /// <summary>
    /// grab out objects and data on start
    /// </summary>
    private void Start()
    {
        GameObject plane = GameObject.Find("NavGridPlane");
        _grid = plane.GetComponent<NavGrid>();
        _navGridArray = new NavGridNode[(int)_grid.GridXSize, (int)_grid.GridZSize];
        Array.Copy(_grid.navGridArray, _navGridArray, _grid.navGridArray.Length);
        animator = GetComponent<Animator>();

    }

    /// <summary>
    /// Handling our input processing etc
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //Grab click collision
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                //User clicked on an obstacle so we'll ignore the click
                if (hitInfo.collider.gameObject.CompareTag("Obstacle"))
                {
                    Debug.Log("Hit a Barrier object!");
                    return;
                    
                }

                //Reset any pathfinding data from a previous run
                _grid.ResetPathFindingData(_navGridArray);

                _currentPath = _grid.GetPath(transform.position, hitInfo.point, _navGridArray);
                _currentPath = _grid.SmoothPath(_currentPath, _navGridArray);
                _currentPathIndex = 0;
            }
        }

        Vector3 targetDestination = Vector3.zero;

        float Xcellsize = _grid.PlaneXSize / _grid.GridXSize;
        float Zcellsize = _grid.PlaneZSize / _grid.GridZSize;
        
        // Traverse
        if (_currentPathIndex < _currentPath.Length)
        {
            var currentNode = _currentPath[_currentPathIndex];
            var maxDistance = _speed * Time.deltaTime;

            targetDestination = currentNode.worldPosition;
            var vectorToDestination = targetDestination - transform.position; 
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);

           //First attempt at bailing if we're close enough
            if (vectorToDestination.magnitude <= maxDistance)
            {

                _currentPathIndex++; // Proceed to the next node
                animator.SetFloat("Speed", 0);
            }
            else
            {
                    Quaternion targetRotation = Quaternion.LookRotation(vectorToDestination);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _speed * Time.deltaTime);
                
                var moveVector = vectorToDestination.normalized * moveDistance;
                animator.SetFloat("Speed", 1);
                transform.position += moveVector;
            }
            //Bail if we hit the point
            if (transform.position.x == currentNode.worldPosition.x && transform.position.z == currentNode.worldPosition.z)
            {
                _currentPathIndex++;
                animator.SetFloat("Speed", 0);

            }
        }
        
    }

}
