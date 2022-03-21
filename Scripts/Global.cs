using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Global : Node
{
    public Node2D InstanceNodeAtLocation(PackedScene node, Node parent, Vector2 location)
    {
        Node2D nodeInstance = InstanceNode(node, parent);
        nodeInstance.GlobalPosition = location;
        return nodeInstance;
    }

    private static Node2D InstanceNode(PackedScene node, Node parent)
    {
        Node2D nodeInstance = node.Instance<Node2D>();
        parent.AddChild(nodeInstance);
        return nodeInstance;
    }
}