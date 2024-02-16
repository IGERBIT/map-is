using System.Numerics;

namespace MapShared.Dto;

public class SizeDto
{
    public SizeDto() {}
    
    public SizeDto(float width, float height)
    {
        Width = width;
        Height = height;
    }
    
    public SizeDto(Vector2 vec)
    {
        Width = vec.X;
        Height = vec.Y;
    }
    
    public float Width { get; set; }
    public float Height { get; set; }
    
    public void Deconstruct(out float width, out float height)
    {
        width = Width;
        height = Height;
    }
    
    public Vector2 Vector2() => new Vector2(Width, Height);
}

