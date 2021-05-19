// This script is autoloaded and is used to track and change scenes

using Godot;
using System;

public class GlobalControl : Node
{
    // Variable storing the current selected scene (technique)
    public static int currentScene;

    // The number of scenes (techniques)
    public const int SCENE_COUNT = 4;

    // Enum of integers storing the IDs for each scene (technique)
    // NOTE: THESE INTEGERS CORRESPOND TO THE ORDER OF THE BUTTONS AS CHILDREN OF THE BUTTONS' HBOX CONTAINER
    // THAT IS, TECHNIQUE_A IS THE FIRST CHILD (GetChild(0)), and so on
    public enum Scenes : int
    {
        TECHNIQUE_A,
        TECHNIQUE_B,
        TECHNIQUE_C,
        PROJECTIONS
    }

    // A boolean for whether the welcome popup has already been shown
    public static bool welcomePopupShown = false;

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Set the application to start at the projections scene
        currentScene = (int) Scenes.TECHNIQUE_A;
    }

    // Called when specific inputs are registered
    public override void _Input(InputEvent @event)
    {
        // If the Esc key is pressed, close the program
        if (@event.IsActionPressed("Quit"))
        {
            GetTree().Quit();
        }
    }
}
