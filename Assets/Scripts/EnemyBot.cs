using System;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Enemy controller class
/// </summary>
public class EnemyBot : MonoBehaviour
{
    /// <summary>
    /// Our animation object
    /// </summary>
    public Animator animator;
 

    private NavGridNode[] _currentPath = Array.Empty<NavGridNode>();
    private int _currentPathIndex = 0;
    
    [SerializeField]
    private NavGrid _grid;

    private NavGridNode[,] _navGridArray;

    [SerializeField]
    private float _speed = 10.0f;

    [SerializeField]
    public bool isEnemy;

    [SerializeField]
    private Player thePlayer;

    /// <summary>
    /// 
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
        if (isEnemy)
        {
            _grid.ResetPathFindingData(_navGridArray);
            _currentPath = _grid.GetPath(transform.position, _grid.GetGridNodeByWorldLocation(thePlayer.transform.position, _navGridArray).worldPosition, _navGridArray);
            _currentPath = _grid.SmoothPath(_currentPath, _navGridArray);
            _currentPathIndex = 0;
        }

        Vector3 targetDestination = Vector3.zero;

        // Traverse
        if (_currentPathIndex < _currentPath.Length)
        {
            var currentNode = _currentPath[_currentPathIndex];
            var maxDistance = _speed * Time.deltaTime;

            targetDestination = currentNode.worldPosition;
            var vectorToDestination = targetDestination - transform.position;
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);

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
                animator.SetFloat("Speed", vectorToDestination.magnitude * _speed);
                transform.position += moveVector;
            }
        
            if (transform.position.x == currentNode.worldPosition.x && transform.position.z == currentNode.worldPosition.z)
            {
                _currentPathIndex++;
                animator.SetFloat("Speed", 0);
            }
        }
        
    }

}
