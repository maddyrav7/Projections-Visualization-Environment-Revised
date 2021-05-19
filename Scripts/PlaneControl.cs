// The plane that displays the projections of an object onto a 2D screen

using Godot;
using System;
using System.Collections.Generic;

public class PlaneControl : Spatial
{
    // Tolerances for comparing vectors and floats in 2D and 3D
    private const float TOLERANCE_3D = 0.01f;
    private const float TOLERANCE_2D = 1f;

    // Variables for 2D projection display
    public static float PLANE_X_SCALE;
    public static float PLANE_Y_SCALE;
    public static float MAX_FOCUS;

    // Integers for specifying mode of projection
    public const int PERSPECTIVE = 0;
    public const int ORTHOGRAPHIC = 1;
    public static int projectionMode = 0;

    // Meshes and data required for projection calculations
    private MeshInstance eye;
    private MeshInstance planeMesh;
    private ArrayMesh planeArrayMesh = new ArrayMesh();
    private MeshDataTool planeData = new MeshDataTool();
    public static MeshInstance objectMesh;
    private static ArrayMesh objectArrayMesh = new ArrayMesh();
    private static MeshDataTool objectData = new MeshDataTool();
    private static SimplifiedMesh basicObjectMesh;

    // Constants for accessing plane data and using matrices
    private const int OBJ_DIMENSIONS = 3;
    private const int PROJ_DIMENSIONS = 2;
    private const int CORNER_INDEX = 0;
    private const int X_CORNER = 2;
    private const int Y_CORNER = 1;

    // Plane data vectors
    private Vector3 planeCorner;
    private Vector3 planeNormal;
    private Vector3 xDirection;
    private Vector3 yDirection;

    // No solution vector for if the ray does not intersect plane
    private static Vector2 noSolutionVector = new Vector2(-100, -100);

    // Plane distance/size information (for zoom of display in orthographic projection)
    public static double zoomValue = 1;
    public static float zoomFactor = 1;

    // Display root node (to display 2D projection)
    private Node2D projectionRoot;

    // List of (projected) Vector2s that are to be displayed onto the screen (projection root node)
    // This list is later altered to hold Vector2s corresponding to the projection of each vertex, as well as each projected point of intersection
    private List<Vector2> displayedVertices = new List<Vector2>();

    // List of SimplifiedMesh indices that correspond to each displayedVertices vertex
    private List<int> displayedVertexIndices = new List<int>();

    // List of tuples for storing edge pairs with local (displayedVertices) indices
    // This list is later subdivided by projected points of intersection to hold line segments that are entirely hidden/shown
    private List<(int, int)> edgePairs = new List<(int, int)>();

    // Ray used to determine whether a line is hidden or shown
    public static RayCast ray;
    
    // List of Vector3s holding the vertices and projected points of intersection at which rays are to be fired
    private List<Vector3> rayPoints = new List<Vector3>();

    // Lists of bools for whether each edge section is shown
    private List<bool> isEdgeShown = new List<bool>();

    // Ready function, called once at the beginning of the scene when all children are ready
    public override void _Ready()
    {
        // Assignment of required nodes
        eye = this.GetChild<MeshInstance>(0);
        planeMesh = this.GetChild<MeshInstance>(1);

        // Creating plane data from mesh
        planeArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, planeMesh.Mesh.SurfaceGetArrays(0));
        planeData.CreateFromSurface(planeArrayMesh, 0);
    }

    // Function used to initialize the static variables of the PlaneControl class
    public static void Initialize(MeshInstance objectMeshInstance, RayCast rayCast, int mode)
    {
        // Assign the object mesh
        objectMesh = objectMeshInstance;

        // Reset the objectArrayMesh and add the object's data
        objectArrayMesh = new ArrayMesh();
        objectArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, objectMesh.Mesh.SurfaceGetArrays(0));

        // Reset the objectData MDT and add the object's data
        objectData = new MeshDataTool();
        objectData.CreateFromSurface(objectArrayMesh, 0);

        // Create the SimplifiedMesh using the object's MDT
        basicObjectMesh = new SimplifiedMesh(objectData);

        // Assign ray
        ray = rayCast;

        // Assign projection mode
        projectionMode = mode;
    }

    // Function to set the size of the display
    // This function is called in the Node and TechniqueC scripts
    public static void SetScreenSize(int planeXScale, int planeYScale, float maxFocus)
    {
        // Assign display variables
        PLANE_X_SCALE = planeXScale;
        PLANE_Y_SCALE = planeYScale;
        MAX_FOCUS = maxFocus;

        // Set the screen size
        ProjectionRoot.SetScreenSize(planeXScale, planeYScale);
    }

    // Function to set the plane's focus/zoom value (used to change zoomValue from other scripts)
    public static void SetZoom(float newZoomValue)
    {
        zoomValue = newZoomValue;
    }

    // Function that updates the planeMesh and zoom based on the zoomValue
    private void UpdateZoom()
    {
        // Set the plane's translation according to the new zoom value if in perspective mode, otherwise, fix the plane at a particular position
        if (projectionMode == PERSPECTIVE)
        {
            planeMesh.Translation = (new Vector3(-((float)zoomValue), planeMesh.Translation.y, planeMesh.Translation.z)) + eye.Translation;
        }
        else if (projectionMode == ORTHOGRAPHIC)
        {
            planeMesh.Translation = (new Vector3(-1f, planeMesh.Translation.y, planeMesh.Translation.z)) + eye.Translation;
        }

        // Update the zoom factor for orthographic projection, or otherwise reset
        if (projectionMode == ORTHOGRAPHIC)
        {
            zoomFactor = MAX_FOCUS/((float)zoomValue);
        }
        else
        {
            zoomFactor = 1;
        }

        // Scale the plane in the X and Z directions according to the zoomFactor (since zoomFactor is 1 in perspective mode, the plane only scales in orthographic mode)
        planeMesh.Scale = new Vector3(zoomFactor, 1, zoomFactor);

        // Force the planeMesh to update its transform without waiting for another frame
        planeMesh.ForceUpdateTransform();
    }

    // Displays the plane onto the projection root node and updates it
    // Called whenever the controls are changed
    public void DisplayPlane()
    {
        // Reset lists
        Reset();

        // Update required plane vectors
        UpdateVectors();

        // Draw onto projection root and update it
        DrawVertices();
        DrawLines();
        projectionRoot.Update();
    }

    // Function to update required plane vectors and zoom information
    public void UpdateVectors()
    {
        // Update zoom information
        UpdateZoom();
        
        // Update plane corner and normal vectors
        planeCorner = planeMesh.GlobalTransform.Xform(planeData.GetVertex(CORNER_INDEX));
        planeNormal = planeMesh.GlobalTransform.basis.y;

        // Update the orthogonal X and Y axis vectors of plane
        xDirection = planeMesh.GlobalTransform.Xform(planeData.GetVertex(X_CORNER)) - planeCorner;
        yDirection = planeMesh.GlobalTransform.Xform(planeData.GetVertex(Y_CORNER)) - planeCorner;
    }

    // Function that calculates vertex information and displays it
    public void DrawVertices()
    {
        // For loop iterating over each vertex in the SimplifiedMesh object
        for (int vertex = 0; vertex < basicObjectMesh.points.Count; vertex++)
        {
            // Vector of object vertex in global coordinates
            Vector3 globalVertexVector = objectMesh.GlobalTransform.Xform(basicObjectMesh.points[vertex]);

            // Direction of ray from eye/plane (NOTE: THIS DOES NOT USE THE RAYCAST NODE)
            Vector3 rayDirection = new Vector3();

            // If in orthographic mode, cast according to the planeNormal vector, otherwise use the direction from eye to vertex
            if (projectionMode == ORTHOGRAPHIC)
            {
                rayDirection = planeNormal;
            }
            else if (projectionMode == PERSPECTIVE)
            {
                rayDirection = globalVertexVector - eye.GlobalTransform.origin;
            }

            // Gets 2D screen coordinates of projection (if they exist)
            Vector2 coordinates = CalculatePlaneCoordinates(globalVertexVector, rayDirection);

            // If the coordinates represent a valid solution, add to lists
            if (coordinates != noSolutionVector)
            {
                displayedVertices.Add(coordinates);
                displayedVertexIndices.Add(vertex);
                rayPoints.Add(globalVertexVector);
            }
        }
    }

    // Projects vertex onto plane and calculates the 2D display coordinates (if they exist)
    public Vector2 CalculatePlaneCoordinates(Vector3 objectVertex, Vector3 rayDirectionVector)
    {
        // Matrix of doubles used to calculate coordinates
        double[,] matrix = new double[OBJ_DIMENSIONS, OBJ_DIMENSIONS + 1];

        // Different matrix depending on projection type
        // Express point as linear combination of rayDirection or planeNormal, xDirection, and yDirection (thus projecting the point onto the plane)
        MatrixSolver.ConvertTo3x3(rayDirectionVector, xDirection, yDirection, matrix);

        // Perspective: LinePoint - PlanePoint = -r*LineDir + s*PlaneDir1 + t*PlaneDir2
        // Orthographic: Point = r*planeNormalVector + s*PlaneDir1 + t*PlaneDir2
        Vector3 constantMatrix = objectVertex - planeCorner;

        // Add constant matrix to 4th (index 3) column
        MatrixSolver.AddVector3Col(3, constantMatrix, matrix);

        // Reduce (solve) matrix and find the number of solutions
        int numOfSolutions = MatrixSolver.SolveMatrix(OBJ_DIMENSIONS, OBJ_DIMENSIONS + 1, matrix, true);

        // Make sure the matrix has a unique solutions and that the point and eye are on opposite sides of the plane (r < 0, that is, -r > 0) or there will be no projection
        if ((numOfSolutions == (int) MatrixSolver.Solutions.UNIQUE_SOLUTION) && (matrix[0, 3] > 0))
        {
            return new Vector2((float)(PLANE_X_SCALE * matrix[1, 3]), (float)(PLANE_Y_SCALE * matrix[2, 3]));
        }
        else
        {
            return noSolutionVector;
        }
    }

    // Function that calculates edge information and displays it
    public void DrawLines()
    {
        // For every edge in the Simplfied Mesh object, add it to edgePairs list if both vertices are displayed (the projection exists)
        for (int edge = 0; edge < basicObjectMesh.displayedEdges.Count; edge++)
        {
            int pointA = basicObjectMesh.displayedEdges[edge].Item1;
            int pointB = basicObjectMesh.displayedEdges[edge].Item2;

            int indexA = displayedVertexIndices.IndexOf(pointA);
            int indexB = displayedVertexIndices.IndexOf(pointB);

            if ((indexA != -1) && (indexB != -1))
            {
                edgePairs.Add((indexA, indexB));
            }
        }

        // Find the intersections of edges in the 2D projection and compute 3D equivalents
        findIntersections();

        // Find which lines are hidden/shown
        // NOTE: AT THIS POINT, rayPoints CONTAINS VERTICES AND 3D INTERSECTION POINTS
        findHiddenEdges();

        // Display every edge in the edgePairs list
        for (int edge = 0; edge < edgePairs.Count; edge++)
        {
            int pointA = edgePairs[edge].Item1;
            int pointB = edgePairs[edge].Item2;

            projectionRoot.Call("AddLine", displayedVertices[pointA], displayedVertices[pointB], isEdgeShown[edge]);
        }
    }

    // Find the intersections of edges in the 2D projection, calculate 3D equivalents of intersection points, and divide edges into components that are completely shown/hidden
    public void findIntersections()
    {
        // Integer storing the index of the current (last added) vertex in the displayedVertices list
        int currentVertex = displayedVertices.Count - 1;

        // Nested for loops iterating through every possible pair (order does not matter) of edge sections in the edgePairsList
        for (int edgeA = 0; edgeA < edgePairs.Count; edgeA++)
        {
            for (int edgeB = (edgeA + 1); edgeB < edgePairs.Count; edgeB++)
            {
                // Integers storing the indices of each vertex in each edge to make it easier to reference the vertices
                int edgeAPointA = edgePairs[edgeA].Item1;
                int edgeAPointB = edgePairs[edgeA].Item2;
                int edgeBPointA = edgePairs[edgeB].Item1;
                int edgeBPointB = edgePairs[edgeB].Item2;

                // Check if the edges do not share a vertex, and only find intersection if they do not
                if ((!MatrixSolver.CompareVector2s(displayedVertices[edgeAPointA], displayedVertices[edgeBPointA], TOLERANCE_2D)) && (!MatrixSolver.CompareVector2s(displayedVertices[edgeAPointA], displayedVertices[edgeBPointB], TOLERANCE_2D)) && (!MatrixSolver.CompareVector2s(displayedVertices[edgeAPointB], displayedVertices[edgeBPointA], TOLERANCE_2D)) && (!MatrixSolver.CompareVector2s(displayedVertices[edgeAPointB], displayedVertices[edgeBPointB], TOLERANCE_2D)))
                {
                    // 2 x 3 augmented matrix used to calculate points of intersection
                    double[,] matrix = new double[PROJ_DIMENSIONS, PROJ_DIMENSIONS + 1];

                    // Direction vectors of the two edge sections
                    // LineDir = Point2 (or Item2) - Point1 (or Item1)
                    // Subtraction for Line1Dir is flipped to account for negative sign in equation
                    Vector2 edgeADirection = displayedVertices[edgeAPointA] - displayedVertices[edgeAPointB];
                    Vector2 edgeBDirection = displayedVertices[edgeBPointB] - displayedVertices[edgeBPointA];

                    // Line1Point - Line2Point = t*Line2Dir - s*Line1Dir
                    // NOTE: IN ORDER FOR THE MATRIX TO RETRUN s AND NOT -s, THE SIGN OF edgeADirection was flipped
                    Vector2 constantMatrix = displayedVertices[edgeAPointA] - displayedVertices[edgeBPointA];

                    // Convert the two edge direction vectors into a 2 x 2 coefficient matrix
                    MatrixSolver.ConvertTo2x2(edgeADirection, edgeBDirection, matrix);

                    // Add the Vector2 constant matrix to the coefficient matrix to create the augmented matrix
                    MatrixSolver.AddVector2Col(2, constantMatrix, matrix);

                    // Solve the 2 x 3 matrix to find the parameter values used to express the point of intersection in the vector equations of the two edge sections
                    // Store the number of solutions to the matrix as an integer
                    int numOfSolutions = MatrixSolver.SolveMatrix(PROJ_DIMENSIONS, PROJ_DIMENSIONS + 1, matrix, true);

                    // Check if lines actually intersect (coefficients must be >= 0 and <= 1 but a tolerance is used to account for small differences) and matrix has a unique solution
                    if ((matrix[0, 2] >= (-TOLERANCE_3D)) && (matrix[0, 2] <= (1 + TOLERANCE_3D)) && (matrix[1, 2] >= (-TOLERANCE_3D)) && (matrix[1, 2] <= (1 + TOLERANCE_3D)) && (numOfSolutions == (int) MatrixSolver.Solutions.UNIQUE_SOLUTION))
                    {
                        // Intersection = Line2Point + t*Line2Dir (t is matrix[1, 2])
                        Vector2 pointOfIntersection = displayedVertices[edgeBPointA] + ((float)matrix[1, 2] * edgeBDirection);

                        // Find the edge directions of the 3D edge sections that correspond to the 2D edge sections, and find the two 3D coordinates of the point of intersection
                        Vector3 objectEdgeADirection = rayPoints[edgeAPointB] - rayPoints[edgeAPointA];
                        Vector3 edgeANewPoint = rayPoints[edgeAPointA] + ((float)matrix[0, 2] * objectEdgeADirection);

                        Vector3 objectEdgeBDirection = rayPoints[edgeBPointB] - rayPoints[edgeBPointA];
                        Vector3 edgeBNewPoint = rayPoints[edgeBPointA] + ((float)matrix[1, 2] * objectEdgeBDirection);

                        // Check if the new 3D point of intersection on edge B is coincident to any of the endpoints of edge B and if not, add the necessary points and edges to the required lists
                        // NOTE: THE INTERSECTION OF EDGE B IS CHECKED FIRST AS THE EDGE B LOOP IS THE INSIDE LOOP, AND THEREFORE ONCE THE EDGE IS BROKEN UP, ONLY THE INDEX NEEDS TO BE DECREMENETED (INSTEAD OF BREAKING THE LOOP)
                        if ((!MatrixSolver.CompareVector3s(edgeBNewPoint, rayPoints[edgeBPointA], TOLERANCE_3D)) && (!MatrixSolver.CompareVector3s(edgeBNewPoint, rayPoints[edgeBPointB], TOLERANCE_3D)))
                        {
                            // Add the new point to the rayPoints list
                            rayPoints.Add(edgeBNewPoint);

                            // Add the 2D intersection point to the displayedVertices list (so that the edge can be broken into two sections based on the intersection point)
                            displayedVertices.Add(pointOfIntersection);

                            // Since a new point has been added to the displayedVertices list, the current vertex index is now 1 higher than before, so increment the currentVertex
                            currentVertex++;

                            // Break edge B according to the 2D intersection point, add the two new edges to the edgePairs list, and remove the existing edge from the edgePairs list
                            edgePairs.Add((edgeBPointA, currentVertex));
                            edgePairs.Add((edgeBPointB, currentVertex));
                            edgePairs.RemoveAt(edgeB);

                            // Decrement edgeB (the index value) so that the next edge B can be calculated (the current iteration number is redone) as edgeB is incremented at the top of the loop, since the current edge was removed from the list
                            // NOTE: THE ITERATION ONLY NEEDS TO BE REDONE IF EDGE A IS NOT BROKEN UP, OTHERWISE, THE ENTIRE LOOP IS REDONE (WITH A NEW EDGE A)
                            edgeB--;
                        }

                        // Check if the new 3D point of intersection on edge A is coincident to any of the endpoints of edge A and if not, add the necessary points and edges to the required lists
                        // NOTE: THE INTERSECTION OF EDGE A IS CHECKED LAST AS THE EDGE A LOOP IS THE OUTSIDE LOOP, AND THEREFORE ONCE THE EDGE IS BROKEN UP, THE ITERATION FOR EDGE A NEEDS TO BE REDONE, AS THE CURRENT EDGE WAS REMOVED, SO THE FOR LOOP FOR EDGE B NEEDS TO BE BROKEN
                        if ((!MatrixSolver.CompareVector3s(edgeANewPoint, rayPoints[edgeAPointA], TOLERANCE_3D)) && (!MatrixSolver.CompareVector3s(edgeANewPoint, rayPoints[edgeAPointB], TOLERANCE_3D)))
                        {
                            // Add the new point to the rayPoints list
                            rayPoints.Add(edgeANewPoint);

                            // Add the 2D intersection point to the displayedVertices list (so that the edge can be broken into two sections based on the intersection point)
                            displayedVertices.Add(pointOfIntersection);

                            // Since a new point has been added to the displayedVertices list, the current vertex index is now 1 higher than before, so increment the currentVertex
                            currentVertex++;

                            // Break edge A according to the 2D intersection point, add the two new edges to the edgePairs list, and remove the existing edge from the edgePairs list
                            edgePairs.Add((edgeAPointA, currentVertex));
                            edgePairs.Add((edgeAPointB, currentVertex));
                            edgePairs.RemoveAt(edgeA);

                            // Decrement edgeA (the index value) so that the next edge A (with the current iteration number) can be calculated (the current iteration is redone), as edgeA is incremented at the top of the loop, since the current edge was removed from the list
                            edgeA--;

                            // Break the for loop for edgeB so that the edgeB for loop can be restarted with a new edgeA
                            break;
                        }
                    }
                }
            }
        }
    }

    // Finds which edges are hidden and shown
    // NOTE: THESE ARE EDGE SECTIONS, AS WHEN THIS FUNCTION IS CALLED, rayPoints HOLDS BOTH VERTICES AND 3D INTERSECTION POINTS
    // NOTE AGAIN: THE RAY IS CAST AT THE EDGE SECTION MIDPOINT, AS SINCE THE EDGE SECTION IS EITHER COMPLETELY SHOWN/HIDDEN, THE MIDPOINT MUST DETERMINE WHETHER THE EDGE IS SHOWN/HIDDEN
    private void findHiddenEdges()
    {
        // Move the ray to the eye's position (ray is casted from the eye in perspective mode)
        ray.Translation = eye.GlobalTransform.origin;
        ray.ForceUpdateTransform();
        
        // Iterate through each edge section
        for (int edge = 0; edge < edgePairs.Count; edge++)
        {
            // Calculate the midpoint of the two rayPoints that create an edge section
            Vector3 midPoint = (rayPoints[edgePairs[edge].Item1] + rayPoints[edgePairs[edge].Item2])/2;
            
            // In orthographic mode, move the ray to the midpoint's 3D projection point on the plane and cast to the midpoint
            // In perspective mode, simply cast the ray at each midpoint, movement of the ray to the eye is not required as that was done before
            if (projectionMode == ORTHOGRAPHIC)
            {
                // Find the 2D coordinates of the midpoint projected onto the plane
                Vector2 projectionCoordinates = CalculatePlaneCoordinates(midPoint, planeNormal);

                // Calculate the 3D coordinates of the projected midpoint from 2D coordinates (projectionCoordinates) using orthogonal plane coordinate vectors and plane size
                // Move the ray to the calculated 3D projection point
                ray.Translation = planeCorner + (projectionCoordinates.x * xDirection / PLANE_X_SCALE) + (projectionCoordinates.y * yDirection / PLANE_Y_SCALE);
                ray.ForceUpdateTransform();                
            }

            // Cast the ray at the midpoint and force the ray to update rather than waiting for a physics frame
            ray.CastTo = midPoint - ray.GlobalTransform.origin;
            ray.ForceRaycastUpdate();

            // Check for ray collision
            if (ray.IsColliding())
            {
                // If the ray is colliding near the midpoint, it is shown, otherwise, it is hidden
                if (MatrixSolver.CompareVector3s(ray.GetCollisionPoint(), midPoint, TOLERANCE_3D))
                {
                    isEdgeShown.Add(true);
                }
                else
                {
                    isEdgeShown.Add(false);
                }
            }
            else
            {
                // If the ray does not collide, the point is shown, as there is no collision between eye/plane and midpoint
                isEdgeShown.Add(true);
            }
        }
    }

    // Function to reset all lists and clear display
    public void Reset()
    {
        // Reset lists
        displayedVertices.Clear();
        displayedVertexIndices.Clear();
        edgePairs.Clear();
        rayPoints.Clear();
        isEdgeShown.Clear();

        // Clear display by having the projectionRoot Node reset its lists
        projectionRoot.Call("Reset");
    }

    public void UpdateImage()
    {
        projectionRoot.Update();
    }

    // Function to set the display node (projectionRoot) of the planeControl scene/object
    // Used for switching between different views (all views/only top/only front/only right/only auxiliary) and viewports
    public void SetDisplayNode(NodePath projectionRootPath)
    {
        // Set projectionRoot using NodePath, as simply passing the node would convert it to an object and projectionRoot's functions cannot be called
        projectionRoot = GetNode<Node2D>(projectionRootPath);
    }
}
