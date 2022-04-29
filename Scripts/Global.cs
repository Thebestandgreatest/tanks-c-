using System.Runtime.InteropServices;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Global : Node
{
    public static Node2D InstanceNodeAtLocation(PackedScene node, Node parent, Vector2 location, [Optional] float rotation)
    {
        Node2D nodeInstance = node.Instance<Node2D>();
        nodeInstance.GlobalPosition = location;
        nodeInstance.GlobalRotationDegrees = rotation;
        parent.AddChild(nodeInstance);
        return nodeInstance;
    }
}