using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Represents CameraExtensions.</summary>
public static class CameraExtensions
{
    /// <summary>Gets ProjectionMatrix.</summary>
    public static Matrix4x4 GetProjectionMatrix(this Camera camera)
    {
        var fovRadians = camera.FieldOfView * (MathF.PI / 180f);
        return Matrix4x4.CreatePerspectiveFieldOfView(
            fovRadians,
            camera.AspectRatio,
            camera.NearPlaneDistance,
            camera.FarPlaneDistance);
    }

    /// <summary>Gets ViewMatrix.</summary>
    public static Matrix4x4 GetViewMatrix(this Camera camera) =>
        Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);

    /// <summary>MoveForward operation.</summary>
    public static void MoveForward(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        camera.Position += direction * speed;
        camera.Target += direction * speed;
    }

    /// <summary>MoveBackward operation.</summary>
    public static void MoveBackward(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        camera.Position -= direction * speed;
        camera.Target -= direction * speed;
    }

    /// <summary>MoveLeft operation.</summary>
    public static void MoveLeft(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var left = Vector3.Normalize(Vector3.Cross(direction, camera.Up));
        camera.Position -= left * speed;
        camera.Target -= left * speed;
    }

    /// <summary>MoveRight operation.</summary>
    public static void MoveRight(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var left = Vector3.Normalize(Vector3.Cross(direction, camera.Up));
        camera.Position += left * speed;
        camera.Target += left * speed;
    }

    /// <summary>MoveUp operation.</summary>
    public static void MoveUp(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var up = Vector3.Normalize(Vector3.Cross(direction, Vector3.Cross(camera.Up, direction)));
        camera.Position += up * speed;
        camera.Target += up * speed;
    }

    /// <summary>MoveDown operation.</summary>
    public static void MoveDown(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var up = Vector3.Normalize(Vector3.Cross(direction, Vector3.Cross(camera.Up, direction)));
        camera.Position -= up * speed;
        camera.Target -= up * speed;
    }
}
