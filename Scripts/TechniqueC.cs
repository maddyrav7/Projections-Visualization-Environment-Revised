// This scene is the Technique C exercise

using Godot;
using System;

public class TechniqueC : Node
{
    // Constants for 2D projection display
    private const int PLANE_X_SCALE = 1000;
    private const int PLANE_Y_SCALE = 1000;
    private const float MAX_FOCUS = 1;

    // Number of BoxPlaneControl objects in the scene
    private const int PLANE_COUNT = 6;

    // An array of all the Spatial node instances of the BoxPlaneControl objects (an array of the 6 planes)
    // ORDER: FRONT, RIGHT, BACK, LEFT, TOP, BOTTOM
    private static Spatial[] planes;

    // An array of booleans for toggling the box faces
    private static bool[] isFaceShown;

    // RayCast Node
    private static RayCast ray;

    // Static body of object
    private static StaticBody objectBody;

    // The number of different views (camera positions)
    private const int VIEW_COUNT = 7;

    // Enum storing indices for the different views
    enum Views : int
    {
        FREE_CAMERA,
        FRONT_VIEW,
        RIGHT_VIEW,
        BACK_VIEW,
        LEFT_VIEW,
        TOP_VIEW,
        BOTTOM_VIEW
    }

    // Different rotations for the orthographic camera depending on which view is selected
    private static Vector3[] viewRotations = new Vector3[VIEW_COUNT] {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, -90, 0), new Vector3(0, 0, 90), new Vector3(0, 0, -90)};

    // The free camera node
    private static Spatial freeCameraRoot = new Spatial();

    // The parent node controlling the orthographic camera
    private static Spatial orthoCameraRoot = new Spatial();

    // An integer storing the index of the selected view
    private static int selectedView = (int) Views.FREE_CAMERA;

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Assign required nodes
        objectBody = GetNode<StaticBody>("ViewportContainer/Viewport/ObjectRoot/Object1");
        ray = GetNode<RayCast>("ViewportContainer/Viewport/ObjectRoot/HiddenLineRay");
        freeCameraRoot = GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/CameraRoot");
        orthoCameraRoot = GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/OrthoCameraRoot");

        // Populate the list of planes with the nodes of each BoxPlaneControl object
        planes = new Spatial[PLANE_COUNT] {GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/FrontPlaneControl"), GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/RightPlaneControl"), GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/BackPlaneControl"), GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/LeftPlaneControl"), GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/TopPlaneControl"), GetNode<Spatial>("ViewportContainer/Viewport/ObjectRoot/BottomPlaneControl")};

        // All faces are hidden at first
        isFaceShown = new bool[PLANE_COUNT] {false, false, false, false, false, false};

        // Initialize the required static variables in the PlaneControl class
        PlaneControl.Initialize(objectBody.GetChild<MeshInstance>(0), ray, PlaneControl.ORTHOGRAPHIC);

        // Set the display size
        PlaneControl.SetScreenSize(PLANE_X_SCALE, PLANE_Y_SCALE, MAX_FOCUS);

        // Set the zoom of the PlaneControl class
        // A zoom value of 1 is set because this scene requires orthographic projection (without scaling to screen size)
        PlaneControl.SetZoom(MAX_FOCUS);

        // Set the projection colours
        ProjectionRoot.SetColours(true, false);
    }

    // Function to update the planes
    public static void UpdatePlanes()
    {
        // For each of the PlaneControl objects (planes), call its UpdateImage function
        for (int plane = 0; plane < PLANE_COUNT; plane++)
        {
            planes[plane].GetChild<Spatial>(0).Call("UpdateImage");
        }
    }

    // Function that is called when the selected view is changed
    // Connected to the "OrthoViewSelectList" dropdown list through the editor
    public void _on_OrthoViewSelectList_item_selected(int index)
    {
        // If the free camera WAS selected, disable the free camera and set the required variables
        // Otherwise, if the free camera is selected now, enable the free camera and set the required variables
        if (selectedView == (int)Views.FREE_CAMERA)
        {
            freeCameraRoot.GetChild<Camera>(0).Current = false;
            freeCameraRoot.SetProcessInput(false);
            orthoCameraRoot.GetChild<Camera>(0).Current = true;
        }
        else if (index == (int)Views.FREE_CAMERA)
        {
            orthoCameraRoot.GetChild<Camera>(0).Current = false;
            freeCameraRoot.GetChild<Camera>(0).Current = true;
            freeCameraRoot.SetProcessInput(true);
        }

        // Set the rotation of the orthoCameraRoot node according to which new view is selected
        orthoCameraRoot.RotationDegrees = viewRotations[index];

        // Update the selectedView
        selectedView = index;
    }

    // Function that is called when the high contrast mode is changed
    // Connected to the "ColoursSlider" slider through the editor
    public void _on_ColoursSlider_toggled(bool isPressed)
    {
        // Depending on whether the high contrast mode is enabled, set the projection colours
        if (isPressed)
        {
            ProjectionRoot.SetColours(true, true);
        }
        else
        {
            ProjectionRoot.SetColours(true, false);
        }

        // Update the display
        UpdatePlanes();
    }

    // Called whenever there are specific input events, such as mouse movement/clicks and key presses
    public override void _Input(InputEvent @event)
    {
        // Depending on which number key is pressed, reveal/hide the corresponding cube face

        if (@event.IsActionPressed("Reveal_Front"))
        {
            planes[0].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[0])
            {
                planes[0].GetChild<Spatial>(0).Call("Reset");
                planes[0].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[0] = false;
            }
            else
            {
                isFaceShown[0] = true;
            }
        }
        if (@event.IsActionPressed("Reveal_Right"))
        {
            planes[1].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[1])
            {
                planes[1].GetChild<Spatial>(0).Call("Reset");
                planes[1].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[1] = false;
            }
            else
            {
                isFaceShown[1] = true;
            }
        }
        if (@event.IsActionPressed("Reveal_Back"))
        {
            planes[2].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[2])
            {
                planes[2].GetChild<Spatial>(0).Call("Reset");
                planes[2].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[2] = false;
            }
            else
            {
                isFaceShown[2] = true;
            }
        }
        if (@event.IsActionPressed("Reveal_Left"))
        {
            planes[3].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[3])
            {
                planes[3].GetChild<Spatial>(0).Call("Reset");
                planes[3].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[3] = false;
            }
            else
            {
                isFaceShown[3] = true;
            }
        }
        if (@event.IsActionPressed("Reveal_Top"))
        {
            planes[4].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[4])
            {
                planes[4].GetChild<Spatial>(0).Call("Reset");
                planes[4].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[4] = false;
            }
            else
            {
                isFaceShown[4] = true;
            }
        }
        if (@event.IsActionPressed("Reveal_Bottom"))
        {
            planes[5].GetChild<Spatial>(0).Call("DisplayPlane");

            if (isFaceShown[5])
            {
                planes[5].GetChild<Spatial>(0).Call("Reset");
                planes[5].GetChild<Spatial>(0).Call("UpdateImage");
                isFaceShown[5] = false;
            }
            else
            {
                isFaceShown[5] = true;
            }
        }
    }
}
