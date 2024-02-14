namespace WebApplication1.Models;

public struct Position
{
    public readonly double X;
    public readonly double Y;
    public readonly int Floor;
    
    public Position(double x, double y, int floor)
    {
        X = x;
        Y = y;
        Floor = floor;
    }
    
    public Position(double x, double y)
    {
        X = x;
        Y = y;
        Floor = 0;
    }
}
