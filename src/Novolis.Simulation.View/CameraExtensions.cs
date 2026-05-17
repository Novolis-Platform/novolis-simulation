using System.Numerics;

namespace Novolis.Simulation.View;

public static class CameraExtensions
{
    public static Matrix4x4 GetProjectionMatrix(this Camera camera)
    {
        var fovRadians = camera.FieldOfView * (MathF.PI / 180f);
        return Matrix4x4.CreatePerspectiveFieldOfView(
            fovRadians,
            camera.AspectRatio,
            camera.NearPlaneDistance,
            camera.FarPlaneDistance);
    }

    public static Matrix4x4 GetViewMatrix(this Camera camera) =>
        Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);

    public static void MoveForward(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        camera.Position += direction * speed;
        camera.Target += direction * speed;
    }

    public static void MoveBackward(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        camera.Position -= direction * speed;
        camera.Target -= direction * speed;
    }

    public static void MoveLeft(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var left = Vector3.Normalize(Vector3.Cross(direction, camera.Up));
        camera.Position -= left * speed;
        camera.Target -= left * speed;
    }

    public static void MoveRight(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var left = Vector3.Normalize(Vector3.Cross(direction, camera.Up));
        camera.Position += left * speed;
        camera.Target += left * speed;
    }

    public static void MoveUp(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var up = Vector3.Normalize(Vector3.Cross(direction, Vector3.Cross(camera.Up, direction)));
        camera.Position += up * speed;
        camera.Target += up * speed;
    }

    public static void MoveDown(this Camera camera, float speed)
    {
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        var up = Vector3.Normalize(Vector3.Cross(direction, Vector3.Cross(camera.Up, direction)));
        camera.Position -= up * speed;
        camera.Target -= up * speed;
    }
}
