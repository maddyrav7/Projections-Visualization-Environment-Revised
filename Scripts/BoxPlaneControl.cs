// This scene is used to display projections onto the box faces in Technique C

using Godot;
using System;

public class BoxPlaneControl : Spatial
{
    public override void _Ready()
    {
        // Assign the planeControl object's display node
        GetNode<Spatial>("PlaneControl").Call("SetDisplayNode", GetNode<Node2D>("Viewport/ProjectionRoot").GetPath());
    }
}
