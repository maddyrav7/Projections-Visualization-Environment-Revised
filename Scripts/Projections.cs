// This scene is the Projections exercise

using Godot;
using System;

public class Projections : Node
{
    // Plane Nodes
    private static Spatial topViewPlane;
    private static Spatial frontViewPlane;
    private static Spatial sideViewPlane;
    private static Spatial auxiliaryViewPlane;

    // Display Nodes
    private static Node2D mainDisplayRoot;
    private static Node2D topViewRoot;
    private static Node2D frontViewRoot;
    private static Node2D sideViewRoot;
    private static Node2D auxiliaryViewRoot;

    // RayCast Node
    private static RayCast ray;

    // Constants for 2D projection display
    private const int LARGE_PLANE_X_SCALE = 681;
    private const int LARGE_PLANE_Y_SCALE = 705;
    private const int SMALL_PLANE_X_SCALE = 340;
    private const int SMALL_PLANE_Y_SCALE = 352;
    private const float MAX_FOCUS = 4;

    // StaticBody of object
    private static StaticBody objectBody;

    // UI Nodes for controlling object, plane, and projection
    private static Spatial objectRoot;
    private static Control ControlsNode;
    private static HSlider FocusZoomSlider;
    private static HSlider ObjectXDegSlider;
    private static HSlider ObjectYDegSlider;
    private static HSlider ObjectZDegSlider;
    private static OptionButton viewList;
    private static GridContainer viewGrid;

    // Enum storing indices for differentiating views
    enum Views : int
    {
        ALL_VIEWS,
        TOP_VIEW,
        FRONT_VIEW,
        SIDE_VIEW,
        AUX_VIEW,
    }
    
    // Bool for whether the auxiliary view is enabled
    private static bool isAuxEnabled;

    // Variable to indicate which view(s) is (are) selected
    private static int selectedView = (int) Views.ALL_VIEWS;

    // The number of objects
    private const int OBJECT_COUNT = 8;

    // The object index that uses an auxiliary view
    private const int AUX_OBJECT = 7;

    // An array of NodePaths of each of the displayed objects
    private static NodePath[] objects = new NodePath[OBJECT_COUNT];

    // The child index of the first object relative to ObjectRoot
    private const int FIRST_OBJECT_INDEX = 3;

    // The index of the selected object
    private static int currentObjectIndex;

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Assignment of required nodes
        // ObjectRoot (root of all 3D nodes)
        objectRoot = GetNode<Spatial>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot");

        // PlaneControl Objects
        topViewPlane = GetNode<Spatial>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot/TopViewPlane");
        frontViewPlane = GetNode<Spatial>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot/FrontViewPlane");
        sideViewPlane = GetNode<Spatial>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot/RightViewPlane");
        auxiliaryViewPlane = GetNode<Spatial>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot/AuxiliaryViewPlane");

        // ProjectionRoot Objects
        mainDisplayRoot = GetNode<Node2D>("HorizontalViewContainer/MainProjectionDisplayContainer/MainProjectionViewport/ProjectionRoot");
        topViewRoot = GetNode<Node2D>("HorizontalViewContainer/MainProjectionDisplayContainer/ViewGridContainer/TopViewportContainer/TopViewport/TopViewRoot");
        frontViewRoot = GetNode<Node2D>("HorizontalViewContainer/MainProjectionDisplayContainer/ViewGridContainer/FrontViewportContainer/FrontViewport/FrontViewRoot");
        sideViewRoot = GetNode<Node2D>("HorizontalViewContainer/MainProjectionDisplayContainer/ViewGridContainer/SideViewportContainer/SideViewport/SideViewRoot");
        auxiliaryViewRoot = GetNode<Node2D>("HorizontalViewContainer/MainProjectionDisplayContainer/ViewGridContainer/AuxiliaryViewportContainer/AuxiliaryViewport/AuxiliaryViewRoot");

        // UI Control Nodes
        ControlsNode = GetNode<Control>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls");
        FocusZoomSlider = GetNode<HSlider>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls/ControlsPanel/ViewControl/FocusZoomSlider");
        ObjectXDegSlider = GetNode<HSlider>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls/ControlsPanel/ObjectControl/HBoxContainer/SlidersVBoxContainer/ObjectXDegSlider");
        ObjectYDegSlider = GetNode<HSlider>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls/ControlsPanel/ObjectControl/HBoxContainer/SlidersVBoxContainer/ObjectYDegSlider");
        ObjectZDegSlider = GetNode<HSlider>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls/ControlsPanel/ObjectControl/HBoxContainer/SlidersVBoxContainer/ObjectZDegSlider");
        viewList = GetNode<OptionButton>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/Controls/ControlsPanel/ViewControl/ViewSelectList");
        viewGrid = GetNode<GridContainer>("HorizontalViewContainer/MainProjectionDisplayContainer/ViewGridContainer");

        // RayCast Node
        ray = GetNode<RayCast>("HorizontalViewContainer/ObjectViewContainter/ObjectViewport/ObjectRoot/HiddenLineRayCast");

        // Populate the objects array with the required NodePaths
        for (int index = 0; index < OBJECT_COUNT; index++)
        {
            objects[index] = objectRoot.GetChild<StaticBody>(index + FIRST_OBJECT_INDEX).GetPath();
        }

        // The second object is initially selected
        currentObjectIndex = 1;

        // Get the object's static body from the selected NodePath in the objects array
        objectBody = GetNode<StaticBody>(objects[currentObjectIndex]);

        // Initialize the required static variables in the PlaneControl class
        PlaneControl.Initialize(objectBody.GetChild<MeshInstance>(0), ray, PlaneControl.ORTHOGRAPHIC);

        // Set the screen size so that the projection is properly scaled
        PlaneControl.SetScreenSize(LARGE_PLANE_X_SCALE, LARGE_PLANE_Y_SCALE, MAX_FOCUS);

        // Set the zoom value of the PlaneControl class according to the focus/zoom slider's value
        PlaneControl.SetZoom((float)FocusZoomSlider.Value);

        // Set the colours that the projection is to be displayed in, by specifying whether the projection is on the cube, and whether the colours have been switched
        ProjectionRoot.SetColours(false, false);

        // Auxiliary view is disabled at first
        isAuxEnabled = false;

        // Set the display nodes and display onto the screen
        SetDisplay();
        Display();
    }

    // Function to change (refresh) display when any of the controls have been changed
    // Controls are connected to this function through the editor
    // Note that the newValue parameter (given by slider signals) is not used
    public static void ChangeDisplay(float newValue = 0)
    {
        // Rotate the object according to sliders
        objectBody.RotationDegrees = new Vector3((float)ObjectXDegSlider.Value, (float)ObjectYDegSlider.Value, (float)ObjectZDegSlider.Value);

        // Force the body to transform (so calculations can be done) rather than waiting for the next frame
        objectBody.ForceUpdateTransform();

        // Set the zoom value
        PlaneControl.SetZoom((float)FocusZoomSlider.Value);

        // Display 2D screens
        Display();
    }

    // This function sets which viewport/display to display to, as well as which view(s) is (are) visible
    public static void SetDisplay()
    {
        // Set grid view as invisible and main view as visible
        viewGrid.Visible = false;
        mainDisplayRoot.Visible = true;

        // Set planeControl objects (in 3D view) as invisible
        topViewPlane.Visible = false;
        frontViewPlane.Visible = false;
        sideViewPlane.Visible = false;
        auxiliaryViewPlane.Visible = false;
        
        // Set the display size to the main viewport size
        PlaneControl.SetScreenSize(LARGE_PLANE_X_SCALE, LARGE_PLANE_Y_SCALE, MAX_FOCUS);

        if (selectedView == (int) Views.ALL_VIEWS)
        {
            // If all views are visible, set the grid view to visible and main view to invisible
            viewGrid.Visible = true;
            mainDisplayRoot.Visible = false;

            // Set the individual view display ndoes
            topViewPlane.Call("SetDisplayNode", topViewRoot.GetPath());
            frontViewPlane.Call("SetDisplayNode", frontViewRoot.GetPath());
            sideViewPlane.Call("SetDisplayNode", sideViewRoot.GetPath());
            auxiliaryViewPlane.Call("SetDisplayNode", auxiliaryViewRoot.GetPath());

            // Set all planeControl objects (in 3D view) as visible
            topViewPlane.Visible = true;
            frontViewPlane.Visible = true;
            sideViewPlane.Visible = true;

            // If auxiliary view is enabled for this object, make the auxiliary planeControl object visible
            if (isAuxEnabled)
            {
                auxiliaryViewPlane.Visible = true;
            }

            // If all views are selected, change the display size to the small viewport size
            PlaneControl.SetScreenSize(SMALL_PLANE_X_SCALE, SMALL_PLANE_Y_SCALE, MAX_FOCUS);
        }
        else if (selectedView == (int) Views.TOP_VIEW)
        {
            // If top view is selected, set top view display node and set 3D planeControl object as visible
            topViewPlane.Call("SetDisplayNode", mainDisplayRoot.GetPath());
            topViewPlane.Visible = true;
        }
        else if (selectedView == (int) Views.FRONT_VIEW)
        {
            // If front view is selected, set front view display node and set 3D planeControl object as visible
            frontViewPlane.Call("SetDisplayNode", mainDisplayRoot.GetPath());
            frontViewPlane.Visible = true;
        }
        else if (selectedView == (int) Views.SIDE_VIEW)
        {
            // If side view is selected, set side view display node and set 3D planeControl object as visible
            sideViewPlane.Call("SetDisplayNode", mainDisplayRoot.GetPath());
            sideViewPlane.Visible = true;
        }
        else if (selectedView == (int) Views.AUX_VIEW && isAuxEnabled)
        {
            // If auxiliary view is selected, set auxiliary view display node and set 3D planeControl object as visible
            auxiliaryViewPlane.Call("SetDisplayNode", mainDisplayRoot.GetPath());
            auxiliaryViewPlane.Visible = true;
        }
    }

    // Display the views by calling the DisplayPlane function on the relevant planeControl objects
    public static void Display()
    {
        if (selectedView == (int) Views.ALL_VIEWS)
        {
            topViewPlane.Call("DisplayPlane");
            frontViewPlane.Call("DisplayPlane");
            sideViewPlane.Call("DisplayPlane");

            // If auxiliary view is enabled, display the auxiliary view, otherwise, update the view
            if (isAuxEnabled)
            {
                auxiliaryViewPlane.Call("DisplayPlane");
            }
            else
            {
                auxiliaryViewRoot.Update();
            }
        }
        else if (selectedView == (int) Views.TOP_VIEW)
        {
            topViewPlane.Call("DisplayPlane");
        }
        else if (selectedView == (int) Views.FRONT_VIEW)
        {
            frontViewPlane.Call("DisplayPlane");
        }
        else if (selectedView == (int) Views.SIDE_VIEW)
        {
            sideViewPlane.Call("DisplayPlane");
        }
        else if (selectedView == (int) Views.AUX_VIEW && isAuxEnabled)
        {
            auxiliaryViewPlane.Call("DisplayPlane");
        }
    }

    // Function that sets the projection mode of PlaneControl when "Perspective" check box is toggled
    // Connected to "Perspective" check box through editor
    public void _on_PerspectiveCheckBox_toggled(bool buttonPressed)
    {
        // Change the PlaneControl projectionMode depending on whether the checkbox is selected
        if (buttonPressed)
        {
            PlaneControl.projectionMode = PlaneControl.PERSPECTIVE;
        }
        else
        {
            PlaneControl.projectionMode = PlaneControl.ORTHOGRAPHIC;
        }

        // Set the visibility of the eye of the planes to be the same as the buttonPressed value for the checkbox
        topViewPlane.GetChild<MeshInstance>(0).Visible = buttonPressed;
        frontViewPlane.GetChild<MeshInstance>(0).Visible = buttonPressed;
        sideViewPlane.GetChild<MeshInstance>(0).Visible = buttonPressed;
        auxiliaryViewPlane.GetChild<MeshInstance>(0).Visible = buttonPressed;

        // Update the display
        Display();
    }

    // Function that sets the visibility of the controls when "Show Controls" button is toggled
    // Connected to "Show Controls" button through editor
    public void _on_ShowControlsButton_toggled(bool buttonPressed)
    {
        if (buttonPressed)
        {
            ControlsNode.Visible = true;
        }
        else
        {
            ControlsNode.Visible = false;
        }
    }

    // Function that is called when the selected view is changed
    // Sets the view according to the selected index on the drop down list, sets the display, and displays to screen
    // Connected to the "ViewSelectList" drop down list through the editor
    public void _on_ViewSelectList_item_selected(int index)
    {
        selectedView = index;
        SetDisplay();
        Display();
    }

    // Function that is called when the "Reset" button is pushed
    // Resets slider values that cause views to reset and then displays to screen
    // Connected to "ResetButton" button through the editor
    public void _on_ResetButton_button_down()
    {
        FocusZoomSlider.Value = 1;
        ObjectXDegSlider.Value = 0;
        ObjectYDegSlider.Value = 0;
        ObjectZDegSlider.Value = 0;
        Display();
    }

    // Function that is called when the selected object is changed
    // Connected to the "ObjectList" option button through the editor
    public void _on_ObjectList_item_selected(int index)
    {
        // Hide the previously selected object and disable its collision
        objectBody.Visible = false;
        objectBody.GetChild<CollisionShape>(1).Disabled = true;

        // If the auxiliary view object (Object 8) is selected, enable the auxiliary view option from the dropdown list, and enable the auxiliary view
        // Otherwise, if the auxiliary object WAS selected before, disable the auxiliary view and the auxiliary view option from the dropdown list, and reset the auxiliary view displays so they are blank
        if (index == AUX_OBJECT)
        {
            viewList.SetItemDisabled((int) Views.AUX_VIEW, false);
            isAuxEnabled = true;
        }
        else if (currentObjectIndex == AUX_OBJECT)
        {
            isAuxEnabled = false;

            if (selectedView == (int) Views.AUX_VIEW)
            {
                viewList.Select((int) Views.ALL_VIEWS);
                selectedView = (int) Views.ALL_VIEWS;
            }

            viewList.SetItemDisabled((int) Views.AUX_VIEW, true);

            auxiliaryViewPlane.Call("Reset");
            auxiliaryViewRoot.Call("Reset");
            auxiliaryViewRoot.Update();
        }

        // Set the current object's index
        currentObjectIndex = index;

        // Reassign the selected static body to the new object
        objectBody = GetNode<StaticBody>(objects[currentObjectIndex]);

        // Show the new object and enable its collision
        objectBody.Visible = true;
        objectBody.GetChild<CollisionShape>(1).Disabled = false;

        // Initialize the PlaneControl class to use the new object
        PlaneControl.Initialize(objectBody.GetChild<MeshInstance>(0), ray, PlaneControl.projectionMode);

        // Set the display again in case the selected object was changed from the auxiliary object
        // ChangeDisplay is used instead of Display so that the controls values are still used and the new object is not completely reset
        SetDisplay();
        ChangeDisplay();
    }

    // Function that is called when the "Switch Colours" slider is toggled
    // Connected to the "ColoursToggleSlider" slider through the editor
    public void _on_ColoursToggleSlider_toggled(bool isPressed)
    {
        // If enabled, set the projection colours to the high contrast mode, otherwise, set them to regular colours
        if (isPressed)
        {
            ProjectionRoot.SetColours(false, true);
        }
        else
        {
            ProjectionRoot.SetColours(false, false);
        }

        // Update the display
        Display();
    }
}
