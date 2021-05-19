// This scene is the root for displaying projections in 2D

using Godot;
using System;
using System.Collections.Generic;

public class ProjectionRoot : Node2D
{
    // Tolerances for comparing points and angles (in radians) in 2D
    private const float TOLERANCE_2D = 3f;
    private const float TOLERANCE_RAD = 0.01f;

    // Floats for screen/viewport size
    private static float SCREEN_WIDTH;
    private static float SCREEN_HEIGHT;

    // Variables for sizes of hidden and shown lines
    private static float LINE_WIDTH = 4f;
    private static float HIDDEN_LINE_WIDTH = 2f;
    private static float HIDDEN_LINE_DASH_SIZE = 12f;
    private static int HIDDEN_LINE_DASH_ITERATIONS = 2;

    // Colours used for the background and hidden/shown edge sections
    private static Color WHITE = new Color(1, 1, 1);
    private static Color GRAY = new Color((float)77/255, (float)77/255, (float)77/255);
    private static Color BLACK = new Color(0, 0, 0);
    private  static Color backgroundColor;
    private static Color displayColor;

    // Background rectangle
    private static Rect2 Background;

    // List storing the Vector2 coordinates of hidden vertices
    private List<Vector2> hiddenVertices = new List<Vector2>();

    // Lists of pairs of Vectors2s that create an edge section that is either completely hidden or completely shown
    private List<(Vector2, Vector2)> hiddenEdges = new List<(Vector2, Vector2)>();
    private List<(Vector2, Vector2)> displayedEdges = new List<(Vector2, Vector2)>();

    // Static function used to initialize the screen size that the projectionRoot node must display to
    // Called in a static function in the PlaneControl script
    public static void SetScreenSize(float width, float height)
    {
        // Set the screen size
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;

        // Set the background according to the screen size
        Background = new Rect2(0, 0, new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT));
    }

    // Static function to set the display colours depending on the mode of display
    public static void SetColours(bool isOnCube, bool isHighContrast)
    {
        // Set the background and display colours depending on whether the high contrast mode is selected
        if (isHighContrast)
        {
            backgroundColor = WHITE;
            displayColor = BLACK;
        }
        else
        {
            backgroundColor = GRAY;
            displayColor = WHITE;
        }

        // If the plane is the face of a cube (Technique C), then change the alpha value of the background colour to make it more transparent, and increase line widths
        if (isOnCube)
        {
            backgroundColor.a = (float)150/255;
            LINE_WIDTH = 8f;
            HIDDEN_LINE_WIDTH = 6f;
            HIDDEN_LINE_DASH_SIZE = 20f;
        }
        else
        {
            backgroundColor.a = 1;
            LINE_WIDTH = 4f;
            HIDDEN_LINE_WIDTH = 2f;
            HIDDEN_LINE_DASH_SIZE = 12f;
        }
    }

    // Draw the hidden lines by first removing duplicate hidden edge sections and then connecting parallel hidden edge sections that share a point
    private void DrawHidden()
    {
        // Iterate through every possible pairing (order does not matter) of hidden edge sections and removes duplicate edge sections
        for (int edgeA = 0; edgeA < hiddenEdges.Count; edgeA++)
        {
            for (int edgeB = edgeA + 1; edgeB < hiddenEdges.Count; edgeB++)
            {
                // If the two hidden edge sections share the same points (compared with a tolerance), remove one of them
                if ((MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item1, TOLERANCE_2D) && MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item2, TOLERANCE_2D)) || (MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item2, TOLERANCE_2D) && MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item1, TOLERANCE_2D)))
                {
                    hiddenEdges.RemoveAt(edgeB);
                    edgeB--;
                }
            }
        }

        // Iterate through every possible pairing of hidden edge sections (order does not matter) and if two parallel sections share only one point, combine them
        for (int edgeA = 0; edgeA < hiddenEdges.Count; edgeA++)
        {
            for (int edgeB = edgeA + 1; edgeB < hiddenEdges.Count; edgeB++)
            {
                // If the two edge sections have the same (compared with tolerance) direction (angle to the x axis), compare their points
                if (MatrixSolver.CompareDoubles(hiddenEdges[edgeA].Item1.AngleToPoint(hiddenEdges[edgeA].Item2), hiddenEdges[edgeB].Item1.AngleToPoint(hiddenEdges[edgeB].Item2), TOLERANCE_RAD) || MatrixSolver.CompareDoubles(hiddenEdges[edgeA].Item2.AngleToPoint(hiddenEdges[edgeA].Item1), hiddenEdges[edgeB].Item1.AngleToPoint(hiddenEdges[edgeB].Item2), TOLERANCE_RAD))
                {
                    // Pairs (tuples) of Vector2s storing the old endpoints (the duplicate points) and the new endpoints (without the duplicate), so they can be removed/added from/to lists
                    (Vector2, Vector2) oldEndPoints = (new Vector2(), new Vector2());
                    (Vector2, Vector2) newEndPoints = (new Vector2(), new Vector2());

                    // Check if any of the points are shared, and assign oldEndPoints and newEndPoints accordingly, otherwise, skip this iteration
                    if (MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item1, TOLERANCE_2D))
                    {
                        oldEndPoints = (hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item1);
                        newEndPoints = (hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item2);
                    }
                    else if (MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item2, TOLERANCE_2D))
                    {
                        oldEndPoints = (hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item2);
                        newEndPoints = (hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item1);
                    }
                    else if (MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item1, TOLERANCE_2D))
                    {
                        oldEndPoints = (hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item1);
                        newEndPoints = (hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item2);
                    }
                    else if (MatrixSolver.CompareVector2s(hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item2, TOLERANCE_2D))
                    {
                        oldEndPoints = (hiddenEdges[edgeA].Item2, hiddenEdges[edgeB].Item2);
                        newEndPoints = (hiddenEdges[edgeA].Item1, hiddenEdges[edgeB].Item1);
                    }
                    else
                    {
                        continue;
                    }

                    // Check that the edge sections share only one point, and if so, combine them
                    if (!MatrixSolver.CompareDoubles(newEndPoints.Item1.AngleToPoint(oldEndPoints.Item1), newEndPoints.Item2.AngleToPoint(oldEndPoints.Item2), TOLERANCE_RAD))
                    {
                        hiddenVertices.Remove(oldEndPoints.Item1);
                        hiddenVertices.Remove(oldEndPoints.Item2);

                        hiddenEdges.RemoveAt(edgeB);
                        hiddenEdges.RemoveAt(edgeA);
                        hiddenEdges.Add(newEndPoints);

                        edgeA--;
                        break;
                    }
                }
            }
        }

        // For each hidden edge, draw a dashed line
        for (int edge = 0; edge < hiddenEdges.Count; edge++)
        {
            int totalDashes = (int)(hiddenEdges[edge].Item1.DistanceTo(hiddenEdges[edge].Item2)/HIDDEN_LINE_DASH_SIZE);
            Vector2 lineDirection = (hiddenEdges[edge].Item2 - hiddenEdges[edge].Item1).Normalized()*HIDDEN_LINE_DASH_SIZE;
            Vector2 currentDashPoint = hiddenEdges[edge].Item1;

            for (int shownDash = 0; shownDash < totalDashes; shownDash += HIDDEN_LINE_DASH_ITERATIONS)
            {
                DrawLine(currentDashPoint, currentDashPoint + lineDirection, displayColor, HIDDEN_LINE_WIDTH);
                currentDashPoint += HIDDEN_LINE_DASH_ITERATIONS*lineDirection;
            }

            if ((totalDashes % HIDDEN_LINE_DASH_ITERATIONS) == 0)
            {
                DrawLine(currentDashPoint, hiddenEdges[edge].Item2, displayColor, HIDDEN_LINE_WIDTH);
            }
        }
    }

    // Adds an edge section to the correct list depending on whether it is shown/hidden
    public void AddLine(Vector2 edgePointA, Vector2 edgePointB, bool isShown)
    {
        if (isShown)
        {
            displayedEdges.Add((edgePointA, edgePointB));
        }
        else
        {
            hiddenEdges.Add((edgePointA, edgePointB));
        }
    }

    // Draws all shown edges
    private void DrawShown()
    {
        for (int edge = 0; edge < displayedEdges.Count; edge++)
        {
            DrawLine(displayedEdges[edge].Item1, displayedEdges[edge].Item2, displayColor, LINE_WIDTH);
        }
    }

    // Clear the display by clearing the point and edge lists
    public void Reset()
    {
        hiddenVertices.Clear();
        hiddenEdges.Clear();
        displayedEdges.Clear();
    }

    // Called from other scripts as projectionRoot.Update()
    // Set the camera position, draw the background, draw hidden lines, and then draw shown lines
    public override void _Draw()
    {
        GetChild<Camera2D>(0).Offset = new Vector2(SCREEN_WIDTH/2, SCREEN_HEIGHT/2);
        DrawRect(Background, backgroundColor);
        DrawHidden();
        DrawShown();
    }
}
