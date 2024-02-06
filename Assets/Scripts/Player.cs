using System;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Player controller class
/// </summary>
public class Player : MonoBehaviour
{
    public Animator animator;
    public float speed = 1.0f;

    private NavGridNode[] _currentPath = Array.Empty<NavGridNode>();
    private int _currentPathIndex = 0;
    
    [SerializeField]
    private NavGrid _grid;

    [SerializeField]
    private float _speed = 10.0f;

    /// <summary>
    /// Handle logic in Awake
    /// </summary>
    private void Awake()
    {
        GameObject plane = GameObject.Find("NavGridPlane");
        _grid = plane.GetComponent<NavGrid>();
        animator = GetComponent<Animator>();
     
    }
    /// <summary>
    /// Handling our input processing etc
    /// </summary>
    void Update()
    {
        // Check for mouse button Input
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
                    // Implement logic for when a Barrier is hit
                }

                //Reset any pathfinding data from a previous run
                _grid.ResetPathFindingData();

                _currentPath = _grid.GetPath(transform.position, hitInfo.point);
                _currentPath = _grid.SmoothPath(_currentPath);
                _currentPathIndex = 0;
            }
        }

        float originalx = transform.position.x;
        float originalz = transform.position.z;
        Vector3 targetDestination = Vector3.zero;

        float Xcellsize = _grid.PlaneXSize / _grid.GridXSize;
        float Zcellsize = _grid.PlaneZSize / _grid.GridZSize;
        // Traverse
        if (_currentPathIndex < _currentPath.Length)
        {
            var currentNode = _currentPath[_currentPathIndex];
            var maxDistance = _speed * Time.deltaTime;

            //We're calculating where the grid location is and adjusting to the midddle of the cell
            float xAdjustment =  _currentPath[_currentPathIndex].gridX * Xcellsize - (_grid.PlaneXSize / 2) + (_grid.GridXSize / 4);
            float zAdjustment =  _currentPath[_currentPathIndex].gridZ * Zcellsize - (_grid.PlaneZSize / 2) + (_grid.GridZSize / 4);

            targetDestination = new Vector3(xAdjustment, 0, zAdjustment) - transform.position;

            // targetDestination = new Vector3(currentNode.gridX ,_grid.PlaneXSize / currentNode.gridX *
            var vectorToDestination = targetDestination;// - transform.position; 
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);

            //If the length of the vector is less than the maxDistance we're close enough and move to the next.  This is in case we don't hit the exact location with floats.
            if (vectorToDestination.magnitude <= maxDistance)
            {

                originalx = transform.position.x;
                originalz = transform.position.z;
                _currentPathIndex++; // Proceed to the next node
                animator.SetFloat("Speed", 0);
            }
            else
            {
               
                    // Create a quaternion for the rotation towards the move direction
                    Quaternion targetRotation = Quaternion.LookRotation(vectorToDestination);

                    // Smoothly rotate towards the target rotation
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _speed * Time.deltaTime);
                
                var moveVector = vectorToDestination.normalized * moveDistance;
                animator.SetFloat("Speed", vectorToDestination.magnitude * speed);
                transform.position += moveVector;
            }
            //If we hit the location go to the next
            if (transform.position.x == currentNode.worldPosition.x && transform.parent.position.z == currentNode.worldPosition.z)
            {
                _currentPathIndex++;
                animator.SetFloat("Speed", 0);
            }
        }
    }

}
