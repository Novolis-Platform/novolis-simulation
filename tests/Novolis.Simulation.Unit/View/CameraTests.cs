using System.Numerics;
using Novolis.Simulation.View;
using TUnit.Core;

namespace Novolis.Simulation.View.Tests;

public class CameraTests
{
    [Test]
    public async Task GetViewMatrix_IsNotDefault()
    {
        var camera = new Camera
        {
            Position = new Vector3(0f, 0f, 5f),
            Target = Vector3.Zero
        };

        await Assert.That(camera.GetViewMatrix()).IsNotEqualTo(default(Matrix4x4));
    }

    [Test]
    public async Task GetProjectionMatrix_IsNotDefault()
    {
        var camera = new Camera { FieldOfView = 60f, AspectRatio = 16f / 9f };
        await Assert.That(camera.GetProjectionMatrix()).IsNotEqualTo(default(Matrix4x4));
    }

    [Test]
    public async Task Up_IsMutable_ForCustomOrientation()
    {
        var camera = new Camera();
        var custom = Vector3.Normalize(new Vector3(0.2f, 1f, 0f));
        camera.Up = custom;
        await Assert.That(camera.Up).IsEqualTo(custom);
    }

    [Test]
    public async Task MoveForward_TranslatesPositionAndTarget()
    {
        var camera = new Camera
        {
            Position = new Vector3(0f, 0f, 10f),
            Target = Vector3.Zero
        };
        var beforeTarget = camera.Target;

        camera.MoveForward(1f);

        await Assert.That(camera.Position.Z).IsLessThan(10f);
        await Assert.That((camera.Target - beforeTarget).Length()).IsGreaterThan(0f);
    }
}
