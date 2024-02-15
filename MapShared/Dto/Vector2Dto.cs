using System.Numerics;

namespace MapShared.Dto;

public class Vector2Dto
{
    public Vector2Dto(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2Dto(Vector2 vec)
    {
        X = vec.X;
        Y = vec.Y;
    }
    
    public float X { get; set; }
    public float Y { get; set; }
    
    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    public Vector2 Vector2() => new Vector2(X, Y);

}

