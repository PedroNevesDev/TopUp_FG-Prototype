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

    public void Connect(SuperTile other, Vector3Int direction)
    {
        if (direction == Vector3Int.right)
        {
            tileRight = other;
            other.tileLeft = this;
            rightWall.SetActive(false);
        }
        else if (direction == Vector3Int.left)
        {
            tileLeft = other;
            other.tileRight = this;
            leftWall.SetActive(false);
        }
        else if (direction == Vector3Int.forward)
        {
            tileForward = other;
            other.tileBackward = this;
            forwardWall.SetActive(false);
        }
        else if (direction == Vector3Int.back)
        {
            tileBackward = other;
            other.tileForward = this;
            backWall.SetActive(false);
        }
        else if (direction == Vector3Int.up)
        {
            tileUp = other;
            other.tileDown = this;
        }
        else if (direction == Vector3Int.down)
        {
            tileDown = other;
            other.tileUp = this;
            floor.SetActive(false);
        }
    }
}