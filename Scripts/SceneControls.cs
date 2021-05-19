// The row of buttons that switches scenes (techniques)

using Godot;
using System;

public class SceneControls : Control
{
    // The HBoxContainer holding the buttons
    private Godot.Node sceneButtonsContainer;

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Assign the buttons' container node
        sceneButtonsContainer = GetNode("SceneControlsPanel/SceneButtonsContainer");

        // When this node is ready, immediately disable the button for the currently selected scene
        // NOTE: THE currentScene VARIABLE IS USED AS THE INDEX THAT CORRESPONDS TO THE ORDER OF THE BUTTONS AS CHILDRENS OF THE HBOXCONTAINER
        sceneButtonsContainer.GetChild<Button>(GlobalControl.currentScene).Disabled = true;
    }

    // Whenever one of the (enabled) buttons is pressed, change the current scene
    public void _on_ProjectionsButton_button_down()
    {
        GlobalControl.currentScene = (int) GlobalControl.Scenes.PROJECTIONS;
        GetTree().ChangeScene("res://Scenes/Projections.tscn");
    }

    public void _on_TechniqueAButton_button_down()
    {
        GlobalControl.currentScene = (int) GlobalControl.Scenes.TECHNIQUE_A;
        GetTree().ChangeScene("res://Scenes/TechniqueA.tscn");
    }

    public void _on_TechniqueBButton_button_down()
    {
        GlobalControl.currentScene = (int) GlobalControl.Scenes.TECHNIQUE_B;
        GetTree().ChangeScene("res://Scenes/TechniqueB.tscn");
    }

    public void _on_TechniqueCButton_button_down()
    {
        GlobalControl.currentScene = (int) GlobalControl.Scenes.TECHNIQUE_C;
        GetTree().ChangeScene("res://Scenes/TechniqueC.tscn");
    }
}
