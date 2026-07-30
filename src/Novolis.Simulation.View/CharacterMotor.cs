using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Collision-free character motor: integrates XZ wish + gravity/jump and writes feet position
/// onto a shared <see cref="YawPitchController"/>. Apps apply collision separately (e.g. planar agents).
/// </summary>
public sealed class CharacterMotor
{
    /// <summary>Creates a motor bound to shared look/feet state.</summary>
    public CharacterMotor(YawPitchController look)
    {
        Look = look ?? throw new ArgumentNullException(nameof(look));
    }

    /// <summary>Shared feet position + facing.</summary>
    public YawPitchController Look { get; }

    /// <summary>Walk speed (m/s).</summary>
    public float WalkSpeed { get; set; } = 4.5f;

    /// <summary>Sprint multiplier applied to walk speed.</summary>
    public float SprintMultiplier { get; set; } = 1.7f;

    /// <summary>Jump vertical speed (m/s).</summary>
    public float JumpSpeed { get; set; } = 6.5f;

    /// <summary>Gravity acceleration (m/s², negative).</summary>
    public float Gravity { get; set; } = -20f;

    /// <summary>Vertical velocity.</summary>
    public float VerticalVelocity { get; set; }

    /// <summary>Whether the character is considered grounded.</summary>
    public bool IsGrounded { get; set; } = true;

    /// <summary>Ground Y plane used when <see cref="IsGrounded"/> snaps.</summary>
    public float GroundY { get; set; }

    /// <summary>Last horizontal delta applied (before vertical).</summary>
    public Vector3 LastHorizontalDelta { get; private set; }

    /// <summary>
    /// Integrates <paramref name="move"/> for <paramref name="deltaSeconds"/> and updates <see cref="Look"/>.Position.
    /// Returns the full position delta applied this tick.
    /// </summary>
    public Vector3 Tick(in MoveIntent move, float deltaSeconds)
    {
        deltaSeconds = Math.Max(0f, deltaSeconds);
        var wish = move.WishDirection;
        wish.Y = 0f;
        if (wish.LengthSquared() > 1e-8f)
            wish = Vector3.Normalize(wish);

        // Wish is camera-relative when length>0: treat as local forward/right if only XZ components.
        // Convention: WishDirection.Z = forward, WishDirection.X = strafe (right).
        Vector3 worldWish;
        if (wish.LengthSquared() < 1e-8f)
        {
            worldWish = Vector3.Zero;
        }
        else
        {
            var forward = Look.GetForwardXZ();
            var right = Look.GetRightXZ();
            worldWish = Vector3.Normalize(forward * wish.Z + right * wish.X);
        }

        var speed = WalkSpeed * (move.Sprint ? SprintMultiplier : 1f);
        LastHorizontalDelta = worldWish * speed * deltaSeconds;

        if (move.Jump && IsGrounded)
        {
            VerticalVelocity = JumpSpeed;
            IsGrounded = false;
        }

        if (!IsGrounded)
            VerticalVelocity += Gravity * deltaSeconds;
        else
            VerticalVelocity = 0f;

        var vertical = new Vector3(0f, VerticalVelocity * deltaSeconds, 0f);
        var delta = LastHorizontalDelta + vertical;
        var next = Look.Position + delta;

        if (!IsGrounded && next.Y <= GroundY)
        {
            next.Y = GroundY;
            VerticalVelocity = 0f;
            IsGrounded = true;
            delta = next - Look.Position;
        }

        Look.Position = next;
        return delta;
    }
}
