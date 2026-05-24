using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Observer camera: view/projection parameters for a scene.</summary>
public record Camera
{
    /// <summary>Position.</summary>
    public Vector3 Position { get; set; } = Vector3.UnitZ * 100;
    /// <summary>Target.</summary>
    public Vector3 Target { get; set; } = Vector3.Zero;
    /// <summary>AspectRatio.</summary>
    public float AspectRatio { get; set; } = 1.6666666666f;
    /// <summary>Up.</summary>
    public Vector3 Up { get; set; } = Vector3.UnitY;
    /// <summary>NearPlaneDistance.</summary>
    public float NearPlaneDistance { get; set; } = 1f;
    /// <summary>FarPlaneDistance.</summary>
    public float FarPlaneDistance { get; set; } = 1_000f;
    /// <summary>FieldOfView.</summary>
    public float FieldOfView { get; set; } = 45f;

    /// <summary>ToString operation.</summary>
    public override string ToString() =>
        $"Position: {Position}\nTarget: {Target}\nAspectRatio: {AspectRatio:N}";
}
