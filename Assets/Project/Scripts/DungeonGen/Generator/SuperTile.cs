using UnityEngine;

public class SuperTile : MonoBehaviour
{
    [Header("ConnectedTiles")]
    public SuperTile tileUp;
    public SuperTile tileDown;
    public SuperTile tileLeft;
    public SuperTile tileRight;
    public SuperTile tileForward;
    public SuperTile tileBackward;

    public Vector3Int GridPosition { get; set; }

    [Header("Structure")]
    public GameObject rightWall;
    public GameObject leftWall;
    public GameObject forwardWall;
    public GameObject backWall;
    public GameObject floor;

    // Cache if the walls have been disabled to avoid redundant SetActive calls
    private bool rightWallDisabled = false;
    private bool leftWallDisabled = false;
    private bool forwardWallDisabled = false;
    private bool backWallDisabled = false;
    private bool floorDisabled = false;

    public void Connect(SuperTile other, Vector3Int direction)
    {
        if (direction == Vector3Int.right)
        {
            tileRight = other;
            if (other.tileLeft != this) other.tileLeft = this;
            
            if (!rightWallDisabled && rightWall != null)
            {
                rightWall.SetActive(false);
                rightWallDisabled = true;
            }
        }
        else if (direction == Vector3Int.left)
        {
            tileLeft = other;
            if (other.tileRight != this) other.tileRight = this;
            
            if (!leftWallDisabled && leftWall != null)
            {
                leftWall.SetActive(false);
                leftWallDisabled = true;
            }
        }
        else if (direction == Vector3Int.forward)
        {
            tileForward = other;
            if (other.tileBackward != this) other.tileBackward = this;
            
            if (!forwardWallDisabled && forwardWall != null)
            {
                forwardWall.SetActive(false);
                forwardWallDisabled = true;
            }
        }
        else if (direction == Vector3Int.back)
        {
            tileBackward = other;
            if (other.tileForward != this) other.tileForward = this;
            
            if (!backWallDisabled && backWall != null)
            {
                backWall.SetActive(false);
                backWallDisabled = true;
            }
        }
        else if (direction == Vector3Int.up)
        {
            tileUp = other;
            if (other.tileDown != this) other.tileDown = this;
        }
        else if (direction == Vector3Int.down)
        {
            tileDown = other;
            if (other.tileUp != this) other.tileUp = this;
            
            if (!floorDisabled && floor != null)
            {
                floor.SetActive(false);
                floorDisabled = true;
            }
        }
    }

    // Helper method to get edge positions for decoration placement
    public Vector3[] GetFloorEdgePositions()
    {
        if (floor == null) return new Vector3[0];
        
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        if (floorRenderer == null) return new Vector3[0];
        
        Bounds bounds = floorRenderer.bounds;
        return new Vector3[]
        {
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // Front-Left
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // Front-Right
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), // Back-Left
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)  // Back-Right
        };
    }
public Vector3 GetSurfacePosition()
{
    if (floor != null)
    {
        Collider col = floor.GetComponent<Collider>();
        if (col != null)
        {
            Bounds bounds = col.bounds; // world space bounds
            Vector3 surfacePos = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            return surfacePos;
        }

        // Optional: fallback if MeshRenderer exists but no collider
        MeshRenderer mr = floor.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Bounds bounds = mr.bounds;
            Vector3 surfacePos = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            return surfacePos;
        }
    }

    // Last fallback
    return transform.position;
}  
}