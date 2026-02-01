using UnityEngine;

public enum CableType { Vein, Artery, Nerve }
public enum SocketSide { Left, Right }

public class CableSocket : MonoBehaviour
{
    public CableType type;
    public SocketSide side;

    [HideInInspector] public bool occupied;
}
