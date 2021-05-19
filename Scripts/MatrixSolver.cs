// A custom matrix solver to help with the linear algebra of calculating projections and so on
// This script is autoloaded

using Godot;
using System;

public class MatrixSolver
{

    // Enum storing the index for the number of solutions to the system
    public enum Solutions : int
    {
        NO_SOLUTIONS,
        UNIQUE_SOLUTION,
        INFINITE_SOLUTIONS
    }

    // A tolerance used to compare whether values are (almost) the same
    private const double TOLERANCE = 0.001;

    // A variable used to determine the length of the main diagonal
    private static int numOfIterations = 0;

    // Swap two rows of a matrix
    private static void SwapRows(int columns, double[,] matrix, int rowA, int rowB)
    {
        double temp = 0;
        for (int col = 0; col < columns; col++)
        {
            temp = matrix[rowA, col];
            matrix[rowA, col] = matrix[rowB, col];
            matrix[rowB, col] = temp;
        }
    }

    // Divide a row of a matrix by a divisor
    private static void DivideRow(int columns, double[,] matrix, int row, double divisor)
    {
        for (int col = 0; col < columns; col++)
        {
            matrix[row, col] /= divisor;
        }
    }

    // Adds coefficient * rowB to rowA
    private static void AddRowMultiple(int columns, double[,] matrix, int rowA, int rowB, double coefficient)
    {
        for (int col = 0; col < columns; col++)
        {
            matrix[rowA, col] += coefficient * matrix[rowB, col];
        }
    }

    // Takes an augmented matrix to (almost) RREF (with leading entries on the main diagonal) and returns whether it has a unique solution
    public static int SolveMatrix(int rows, int columns, double[,] matrix, bool augmented)
    {
        // Int to store how many solutions there are
        int numOfSolutions = (int) Solutions.UNIQUE_SOLUTION;

        // Boolean to check if first entry of row is nonzero (for division)
        bool divisible = false;

        // Find minimum of rows and columns
        if (rows < columns)
        {
            numOfIterations = rows;
        }
        else if (augmented)
        {
            numOfIterations = columns - 1;
        }

        // Iterate over the minimum to reduce to RREF
        for (int iteration = 0; iteration < numOfIterations; iteration++)
        {
            // Swap the iteration row to be one with a leading entry (if possible)
            // Look only at incomplete rows (with index higher than iteration number)
            for (int row = iteration; row < rows; row++)
            {
                if (matrix[row, iteration] != 0)
                {
                    SwapRows(columns, matrix, iteration, row);
                    divisible = true;
                    break;
                }
                divisible = false;
            }

            // Divide row by first entry (if nonzero) and subtract from all rows other than iteration row (with leading 1)
            if (divisible)
            {
                DivideRow(columns, matrix, iteration, matrix[iteration, iteration]);
                for (int row = 0; row < rows; row++)
                {
                    if (row != iteration)
                    {
                        AddRowMultiple(columns, matrix, row, iteration, -matrix[row, iteration]);
                    }
                }
            }
        }

        if (augmented)
        {
            // Check for zeroes on the main diagonal
            for (int coefficientEntry = 0; coefficientEntry < numOfIterations; coefficientEntry++)
            {
                if ((CompareDoubles(matrix[coefficientEntry, coefficientEntry], 0, TOLERANCE)))
                {
                    if (CompareDoubles(matrix[coefficientEntry, columns - 1], 0, TOLERANCE))
                    {
                        numOfSolutions = (int) Solutions.INFINITE_SOLUTIONS;
                    }
                    else
                    {
                        numOfSolutions = (int) Solutions.NO_SOLUTIONS;
                        break;
                    }
                    break;
                }
            }

            // Check for leading entries in constant matrix
            for (int constantEntry = numOfIterations; constantEntry < rows; constantEntry++)
            {
                if (!CompareDoubles(matrix[constantEntry, columns - 1], 0, TOLERANCE))
                {
                    numOfSolutions = (int) Solutions.NO_SOLUTIONS;
                    break;
                }
            }
        }

        return numOfSolutions;
    }

    // Compare doubles with tolerance
    public static bool CompareDoubles(double numA, double numB, double tolerance)
    {
        return (Math.Abs(numA - numB) < tolerance);
    }

    // Compare Vector2s using tolerance
    public static bool CompareVector2s(Vector2 vectorA, Vector2 vectorB, double tolerance)
    {
        return (CompareDoubles(vectorA.x, vectorB.x, tolerance) && CompareDoubles(vectorA.y, vectorB.y, tolerance));
    }

    // Compare Vector3s using tolerance
    public static bool CompareVector3s(Vector3 vectorA, Vector3 vectorB, double tolerance)
    {
        return (CompareDoubles(vectorA.x, vectorB.x, tolerance) && CompareDoubles(vectorA.y, vectorB.y, tolerance) && CompareDoubles(vectorA.z, vectorB.z, tolerance));
    }

    // Add a Vector2 as a specified column of a matrix
    public static void AddVector2Col(int columnNum, Vector2 column, double[,] matrix)
    {
        matrix[0, columnNum] = column.x;
        matrix[1, columnNum] = column.y;
    }

    // Convert two Vector2 columns into a 2x2 matrix
    public static void ConvertTo2x2(Vector2 column1, Vector2 column2, double[,] matrix)
    {
        AddVector2Col(0, column1, matrix);
        AddVector2Col(1, column2, matrix);
    }

    // Add a Vector3 as a specified column of a matrix
    public static void AddVector3Col(int columnNum, Vector3 column, double[,] matrix)
    {
        matrix[0, columnNum] = column.x;
        matrix[1, columnNum] = column.y;
        matrix[2, columnNum] = column.z;
    }

    // Convert three Vector3 columns into a 3x3 matrix
    public static void ConvertTo3x3(Vector3 column1, Vector3 column2, Vector3 column3, double[,] matrix)
    {
        AddVector3Col(0, column1, matrix);
        AddVector3Col(1, column2, matrix);
        AddVector3Col(2, column3, matrix);
    }
}