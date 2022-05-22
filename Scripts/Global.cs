using System.Runtime.InteropServices;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Global : Node
{
    public static Node2D InstanceNodeAtLocation(PackedScene node, Node parent, [Optional] Vector2 location, [Optional] float rotation, [Optional] int zindex)
    {
        Node2D nodeInstance = node.Instance<Node2D>();
        nodeInstance.GlobalPosition = location;
        nodeInstance.GlobalRotationDegrees = rotation;
        nodeInstance.ZIndex = zindex;
        parent.AddChild(nodeInstance);
        return nodeInstance;
    }

    public static Node2D InstanceNetworkMasterNode(PackedScene node, Node parent, Vector2 location, float rotation,
        int owner)
    {
        Node2D nodeInstance = node.Instance<Node2D>();
        nodeInstance.GlobalPosition = location;
        nodeInstance.GlobalRotationDegrees = rotation;
        nodeInstance.SetNetworkMaster(owner);
        parent.AddChild(nodeInstance);
        return nodeInstance;
    }
}