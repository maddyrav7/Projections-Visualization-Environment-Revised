// The player camera that moves around an object

using Godot;
using System;

public class CameraRoot : Spatial
{
    // Transform holding the camera's initial state for resetting
    private Transform resetTransform;

    // Rotation speed of the user camera
    private const float ROTATION_SPEED_DEG = 60f;

    // Vectors3s used to rotate the camera root
    private Vector3 Y_ROTATION = new Vector3(ROTATION_SPEED_DEG, 0, 0);
    private Vector3 X_ROTATION = new Vector3(0, ROTATION_SPEED_DEG, 0);

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Set the resetTransform based on the camera root's state when it is ready
        resetTransform = this.Transform;
    }

    // Called whenever there are specific input events, such as mouse movement/clicks and key presses
    public override void _Input(InputEvent @event)
    {
        // If the Reset key (currently R) is pressed, reset the camera transform to its initial state
        if (@event.IsActionPressed("Reset"))
        {
            this.Transform = resetTransform;
        }
    }

    // Physics process function, called every physics frame (currently set to 60 fps)
    public override void _PhysicsProcess(float delta)
    {
        // If the arrow keys are pressed and the camera is enabled, rotate the camera root

        if (this.IsProcessingInput())
        {
            if (Input.IsActionPressed("Rotate_Up"))
            {
                this.RotationDegrees -= Y_ROTATION * delta;
            }
            if (Input.IsActionPressed("Rotate_Down"))
            {
                this.RotationDegrees += Y_ROTATION * delta;
            }
            if (Input.IsActionPressed("Rotate_Left"))
            {
                this.RotationDegrees -= X_ROTATION * delta;
            }
            if (Input.IsActionPressed("Rotate_Right"))
            {
                this.RotationDegrees += X_ROTATION * delta;
            }
        }
    }
}
