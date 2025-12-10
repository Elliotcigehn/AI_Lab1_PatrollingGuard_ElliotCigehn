using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float tileSize = 1f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject TilePrefab;
    [SerializeField] private Material WalkableMaterial;
    [SerializeField] private Material WallMaterial;
    [SerializeField] public Material PathMaterial;
    [SerializeField] private Material StartMaterial;
    [SerializeField] private Material goalMaterial;
    private Node[,] nodes;
    private Dictionary<GameObject, Node> tileNodeMap = new();

    private InputAction clickAction;

    public int Width => width;
    public int Height => height;
    public float TileSize => tileSize;

    private void Awake()
    {
        GenerateGrid();
    }

    private void OnEnable()
    {
        clickAction = new InputAction(
        name: "Click",
        type: InputActionType.Button,
        binding: "<Mouse>/leftButton"
        );
        clickAction.performed += OnClickPerformed;
        clickAction.Enable();
    }
    private void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.performed -= OnClickPerformed;
            clickAction.Disable();
        }
    }

    private void GenerateGrid()
    {
        nodes = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject tileGO = Instantiate(TilePrefab, worldPos, Quaternion.identity, transform);
                tileGO.name = $"Tile_{x}_{y}";
                Node node = new Node(x, y, true, tileGO);
                nodes[x, y] = node;
                tileNodeMap[tileGO] = node;
                SetTileMaterial(node, WalkableMaterial);
            }
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedTile = hit.collider.gameObject;
            if (tileNodeMap.TryGetValue(clickedTile, out Node node))
            {
                node.walkable = !node.walkable;
                SetTileMaterial(node, node.walkable ? WalkableMaterial : WallMaterial);
            }
        }
    }

    public Node GetNode(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return null;
        }
        return nodes[x, y];
    }

    public Node GetNodeFromWorldPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.z / tileSize);
        return GetNode(x, y);
    }

    public IEnumerable<Node> GetNeighbors(Node node, bool allowDiagonals = false)
    {
        int x = node.x;
        int y = node.y;
        // 4-neighbour
        yield return GetNode(x + 1, y);
        yield return GetNode(x - 1, y);
        yield return GetNode(x, y + 1);
        yield return GetNode(x, y - 1);
        if (allowDiagonals)
        {
            yield return GetNode(x + 1, y + 1);
            yield return GetNode(x - 1, y + 1);
            yield return GetNode(x + 1, y - 1);
            yield return GetNode(x - 1, y - 1);
        }
    }

    public void SetWalkable(Node node, bool walkable)
    {
        node.walkable = walkable;
        SetTileMaterial(node, walkable ? WalkableMaterial : WallMaterial);
    }

    public void SetTileMaterial(Node node, Material material)
    {
        Renderer renderer = node.tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }
}
