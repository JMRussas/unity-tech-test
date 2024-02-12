using Codice.Client.Common.GameUI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Editor class to change where our obstacles will be
/// </summary>
public class GridEditor : EditorWindow
{
    /// <summary>
    /// Playing surface GameObject
    /// </summary>
    private GameObject _playingPlane;

    /// <summary>
    /// Class that handles generating maze 
    /// </summary>
    private MazeGenerator mg;

    /// <summary>
    /// Data object containing our node array
    /// </summary>
    private NavGrid _navGrid;
    
    /// <summary>
    /// Visual Element that we're putting our buttons into
    /// </summary>
    private VisualElement _GridContainer;

    /// <summary>
    /// Show the window
    /// </summary>
    [MenuItem("Window/Pathfinding/GridEditor")]
    public static void ShowGridEditor()
    {
        GridEditor wnd = GetWindow<GridEditor>();
        wnd.titleContent = new GUIContent("Grid Editor");
    }

    /// <summary>
    /// Handle reading our uxml etc. We're hardocded size wise so changing grid would require changing the size
    /// </summary>
    private void OnEnable()
    {

        mg = new MazeGenerator();
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/GridEditor.uxml");
        visualTree.CloneTree(rootVisualElement);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Styles/pathfinder_USS.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        _GridContainer = rootVisualElement.Q<VisualElement>("GridContainer");

        Vector2 windowSize = new Vector2(1200, 600); 
        this.minSize = windowSize;
        this.maxSize = windowSize;

        RegisterButtonEvents();
        GenerateNavGrid();
    }


    /// <summary>
    /// Get our data object with all our grid data
    /// </summary>
    private void GetNavGrid()
    {
        _playingPlane = GameObject.Find("NavGridPlane");
        if (null != _playingPlane )
            _navGrid = _playingPlane.GetComponent<NavGrid>();
    }

    /// <summary>
    /// Register our callbacks for our buttons
    /// </summary>
    private void RegisterButtonEvents()
    {
        Button genMaze = (Button)rootVisualElement.Q<VisualElement>("generateMaze");
        genMaze.clicked += () => OnGenMazeClicked(genMaze);
        Button clearPlane = (Button)rootVisualElement.Q<VisualElement>("clearPlane");
        clearPlane.clicked += () => OnclearPlaneClicked(clearPlane);
        Button fillPlane = (Button)rootVisualElement.Q<VisualElement>("fillPlane");
        fillPlane.clicked += () => OnfillPlaneClicked(fillPlane);
    }

    /// <summary>
    /// Handle our maze generation event
    /// </summary>
    /// <param name="btn"></param>
    private void OnGenMazeClicked(Button btn)
    {
        if (null == _navGrid)
            return;

        mg.GenerateMaze(_navGrid);
        RefreshNavGrid();
    }

    /// <summary>
    /// Handles out clear all obstacles event callback
    /// </summary>
    /// <param name="btn"></param>
    private void OnclearPlaneClicked(Button btn)
    {
        mg.ToggleAllWalkableBits(_navGrid, true);
        ToggleAllButtons(true);

    }

    /// <summary>
    /// Handle our fill everyting with obstacles callback 
    /// </summary>
    /// <param name="btn"></param>
    private void OnfillPlaneClicked(Button btn)
    {
        mg.ToggleAllWalkableBits(_navGrid, false);
        ToggleAllButtons(false);
    }

   
    /// <summary>
    /// Create our grid of controls for the obstacles
    /// </summary>
    /// <returns></returns>
    private bool GenerateNavGrid()
    {        
        GetNavGrid();
        if (null == _navGrid) return false;
        if (null == _navGrid.navGridArray ) return false;

        _GridContainer = rootVisualElement.Q<VisualElement>(name = "GridContainer");
        if(null == _GridContainer ) return false;

        for (int x = 0; x < (int)_navGrid.GridXSize; x++)
        {
            // Create a new VisualElement for each row
            VisualElement rowContainer = new VisualElement();
            
            rowContainer.AddToClassList("grid-rows");

            for (int z = (int)_navGrid.GridZSize - 1; z >= 0; z--)
            {
                NavGridNode ngn = _navGrid.navGridArray[x, z];
                Button btn = new Button { text = $"({x},{z})" };
                btn.style.backgroundColor = ngn.isWalkable ? Color.green : Color.red;
                btn.style.color = ngn.isWalkable ? Color.black : Color.black; //both black atm
                btn.style.minHeight = 50;
                btn.style.minWidth = 50;
                btn.AddToClassList("gridbutton");
                btn.userData = new Vector2(x, z);
                btn.clicked += () => {
                    Vector2 pos = (Vector2)btn.userData;
                    OnNodeClicked((int)pos.x, (int)pos.y, btn);
                };
                btn.style.borderTopWidth = btn.style.borderRightWidth = btn.style.borderBottomWidth = btn.style.borderLeftWidth = 2;
                btn.style.borderTopWidth = btn.style.borderRightWidth = btn.style.borderBottomWidth = btn.style.borderLeftWidth = 2;
                btn.style.borderTopColor = btn.style.borderRightColor = btn.style.borderBottomColor = btn.style.borderLeftColor = Color.black;
                btn.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    btn.style.borderTopColor = btn.style.borderRightColor = btn.style.borderBottomColor = btn.style.borderLeftColor = Color.white;
                });
                btn.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    btn.style.borderTopColor = btn.style.borderRightColor = btn.style.borderBottomColor = btn.style.borderLeftColor = Color.black;
                });
                rowContainer.Add(btn);
            }
            _GridContainer.Add(rowContainer);
        }

        return true;
    }

    /// <summary>
    /// Refresh our data after changes
    /// </summary>
    /// <returns></returns>
    private bool RefreshNavGrid()
    {
        GetNavGrid();
        if (null == _navGrid) return false;

        if (null == _navGrid.navGridArray) return false;

        _GridContainer = rootVisualElement.Q<VisualElement>(name = "GridContainer");
        if (null == _GridContainer) return false;

        // Query all row containers within the GridContainer
        var rows = _GridContainer.Query<VisualElement>().Where(e => e.ClassListContains("grid-rows")).ToList();
        int x = 0;
        int z = 0;
        foreach (var row in rows)
        {
            
            // Now iterate through each button within the row
            var buttons = row.Query<Button>().ToList();
            z = buttons.Count - 1;
            foreach (var btn in buttons)
            {
               
                btn.style.backgroundColor = _navGrid.navGridArray[x,z].isWalkable ? Color.green : Color.red;
                z--;
            }
            x++;
        }
        return true;
    }


    /// <summary>
    /// Let's handle when the grid node is clicked.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="btn"></param>
    private void  OnNodeClicked(int x, int z, Button btn)
    {
        bool isWalkable = _navGrid.navGridArray[x, z].isWalkable;
        _navGrid.navGridArray[x, z].isWalkable = !isWalkable;
        btn.style.backgroundColor = !isWalkable ? Color.green : Color.red;
        btn.style.color = !isWalkable ? Color.black : Color.black;
        _navGrid.SaveData();
    }

    /// <summary>
    /// Toggle all the buttons to all walkable or not
    /// </summary>
    /// <param name="walkable"></param>
    public void ToggleAllButtons(bool walkable)
    {
        // Query all row containers within the GridContainer
        var rows = _GridContainer.Query<VisualElement>().Where(e => e.ClassListContains("grid-rows")).ToList();
        int x = 0;
        int z = 0;
        foreach (var row in rows)
        {
            // Now iterate through each button within the row
            var buttons = row.Query<Button>().ToList();
            z = buttons.Count - 1;
            foreach (var btn in buttons)
            {
                bool isWalkable = _navGrid.navGridArray[x, z].isWalkable;
                _navGrid.navGridArray[x, z].isWalkable = isWalkable;
                btn.style.backgroundColor = walkable ? Color.green: Color.red;
                btn.style.color = isWalkable ? Color.black : Color.black;
                z--;
            }
            x++;
        }
    }

}