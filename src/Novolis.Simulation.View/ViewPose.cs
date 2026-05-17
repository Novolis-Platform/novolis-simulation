using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Observer position and orientation for adapters to math or render cameras.</summary>
public readonly record struct ViewPose(Vector3 Position, float Yaw, float Pitch);
