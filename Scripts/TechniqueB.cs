// This scene is the Technique B exercise

using Godot;
using System;

public class TechniqueB : Node
{
    // Rotate by Pi/90 radians each frame
    private const float DELTA_ROTATION_RAD = (float)Math.PI/90;

    // Rotate by 2 deg each frame
    private const float DELTA_ROTATION_DEG = 2f;

    // Axis vector used to determine which axis to rotate around when a button is pressed
    private Vector3 axisVector;

    // Object root used to rotate the orthographic camera and the object
    private Spatial objectMeshRoot;

    // MeshInstance node of object
    private MeshInstance objectMesh;

    // Amount of degrees left to rotate until a rotation is complete
    private float degsLeftToRotate = 0f;

    // Rotation is complete when there are 0 degrees left to rotate
    private const float ROTATION_COMPLETE = 0f;

    // The amount the object rotates when a button is pressed
    private const float ROTATION_STEP = 90f;

    // The number of different views (camera positions)
    private const int VIEW_COUNT = 7;

    // The number of objects
    private const int OBJECT_COUNT = 4;

    // The child index of the first object relative to ObjectMeshRoot
    private const int FIRST_OBJECT_INDEX = 1;

    // The index of the selected object
    private static int currentObjectIndex;

    // Enum storing indices for the different views
    enum Views : int
    {
        MAIN_VIEW,
        FRONT_VIEW,
        RIGHT_VIEW,
        BACK_VIEW,
        LEFT_VIEW,
        TOP_VIEW,
        BOTTOM_VIEW
    }

    // Different rotations for the orthographic camera depending on which view is selected
    private static Vector3[] viewRotations = new Vector3[VIEW_COUNT] {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, -90, 0), new Vector3(-90, 0, 0), new Vector3(90, 0, 0)};

    // The main camera
    private static Camera mainCamera = new Camera();

    // The parent node controlling the orthographic camera
    private static Spatial orthoCameraRoot = new Spatial();

    // An integer storing the index of the selected view
    private static int selectedView = (int) Views.MAIN_VIEW;

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Assign the object mesh root
        objectMeshRoot = GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/ObjectMeshRoot");

        // Assign the main camera
        mainCamera = GetNode<Camera>("ViewportContainer/Viewport/ObjectRoot/MainCamera");

        // Assign the camera root
        orthoCameraRoot = GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/ObjectMeshRoot/OrthoCameraRoot");

        // The first object is initially selected
        currentObjectIndex = 0;

        // Get the mesh of the first object
        objectMesh = objectMeshRoot.GetChild<MeshInstance>(FIRST_OBJECT_INDEX + currentObjectIndex);
    }

    // PhysicsProcess function, called every physics frame (currently set to 60 fps)
    public override void _PhysicsProcess(float delta)
    {
        // If the rotation animation is not complete (the object has not completed its full rotation), keep rotating the object
        // If the rotation is complete, unpause the controls (or the scene)
        if (degsLeftToRotate != ROTATION_COMPLETE)
        {
            objectMeshRoot.Rotate(axisVector, DELTA_ROTATION_RAD);
            degsLeftToRotate -= DELTA_ROTATION_DEG;
        }
        else
        {
            GetTree().Paused = false;
        }
    }

    // Whenever an arrow button is pressed, set the amount the object has to rotate, set the axis of rotation, and pause the controls (or scene)

    public void _on_UpButton_button_down()
    {
        degsLeftToRotate = ROTATION_STEP;
        axisVector = Vector3.Right;
        GetTree().Paused = true;
    }

    public void _on_LeftButton_button_down()
    {
        degsLeftToRotate = ROTATION_STEP;
        axisVector = Vector3.Up;
        GetTree().Paused = true;
    }

    public void _on_RightButton_button_down()
    {
        degsLeftToRotate = ROTATION_STEP;
        axisVector = Vector3.Down;
        GetTree().Paused = true;
    }

    public void _on_DownButton_button_down()
    {
        degsLeftToRotate = ROTATION_STEP;
        axisVector = Vector3.Left;
        GetTree().Paused = true;
    }

    // When the reset button is pressed, reset the object's rotation
    public void _on_ResetViewButton_button_down()
    {
        objectMeshRoot.RotationDegrees = new Vector3(0, 0, 0);
    }

    // Function that is called when the selected view is changed
    // Connected to the "ViewSelectList" dropdown list through the editor
    public void _on_ViewSelectList_item_selected(int index)
    {
        // If the main view is selected, or the view is switched from the main view to another view, switch the cameras
        if (index == (int) Views.MAIN_VIEW)
        {
            orthoCameraRoot.GetChild<Camera>(0).Current = false;
            mainCamera.Current = true;
        }
        else if (selectedView == (int) Views.MAIN_VIEW)
        {
            mainCamera.Current = false;
            orthoCameraRoot.GetChild<Camera>(0).Current = true;
        }

        // Set the rotation of the orthoCameraRoot node according to which new view is selected
        orthoCameraRoot.RotationDegrees = viewRotations[index];

        // Update the selectedView
        selectedView = index;
    }

    // Function that is called when the selected object is changed
    // Connected to the "ObjectSelectList" option button through the editor
    public void _on_ObjectSelectList_item_selected(int index)
    {
        // Hide the previously selected object
        objectMesh.Visible = false;

        // Set the current object's index
        currentObjectIndex = index;

        // Reassign the selected mesh instance to the new object
        objectMesh = objectMeshRoot.GetChild<MeshInstance>(FIRST_OBJECT_INDEX + currentObjectIndex);

        // Show the new object
        objectMesh.Visible = true;
    }
}
