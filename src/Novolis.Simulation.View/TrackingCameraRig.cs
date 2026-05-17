using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Smoothly tracks a moving world position (e.g. projectile) with an offset.</summary>
public sealed class TrackingCameraRig
{
    private Vector3 _smoothedPosition;
    private bool _initialized;

    public Vector3 Offset { get; set; } = new(0f, 6f, 0f);

    public float SmoothRate { get; set; } = 12f;

    public void Snap(Vector3 worldPosition)
    {
        _smoothedPosition = worldPosition;
        _initialized = true;
    }

    public void ResetSmoothing() => _initialized = false;

    public Vector3 Update(float deltaSeconds, Vector3 worldPosition)
    {
        if (!_initialized)
        {
            _smoothedPosition = worldPosition;
            _initialized = true;
            return _smoothedPosition;
        }

        var t = 1f - MathF.Exp(-SmoothRate * MathF.Max(deltaSeconds, 1e-4f));
        _smoothedPosition = Vector3.Lerp(_smoothedPosition, worldPosition, t);
        return _smoothedPosition;
    }

    public Vector3 TrackedPoint => _smoothedPosition + Offset;
}
