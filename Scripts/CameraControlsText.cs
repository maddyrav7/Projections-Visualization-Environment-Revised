// A Control node with the camera controls and the show/hide toggle button as children

using Godot;
using System;

public class CameraControlsText : Control
{
    public Panel controlsPanel;
    public override void _Ready()
    {
        controlsPanel = GetNode<Panel>("ControlsPanel");
    }

    // When the "ShowExplanationButton" is pressed, show or hide the panel with the controls text
    public void _on_ShowExplanationButton_button_down()
    {
        controlsPanel.Visible = !controlsPanel.Visible;
    }
}
