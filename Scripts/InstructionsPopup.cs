// The instructions text that pops up whenever a scene is first visited

using Godot;
using System;

public class InstructionsPopup : PopupDialog
{
    // Boolean storing whether this popup is only shown once (only for the welcome popup)
    [Export] public bool onlyOnce;

    // Popup when the scene is ready
    public override void _Ready()
    {
        // If the popup is shown only once, only show the welcome popup if it has not been shown before
        // Otherwise (if it is not the welcome popup), show the popup
        if (onlyOnce)
        {
            if (!GlobalControl.welcomePopupShown)
            {
                CallDeferred("popup_centered");
                GlobalControl.welcomePopupShown = true;
            }
        }
        else
        {
            CallDeferred("popup_centered");
        }
    }

    // Function that is called when the "PopupCloseButton" ("Okay") button is pressed
    public void _on_PopupCloseButton_button_down()
    {
        // When the "Okay" button is pressed, close the popup
        Visible = false;
    }
}
