using System.Numerics;
using Novolis.Simulation.View;

namespace Novolis.Simulation.Unit.View;

public sealed class CharacterCameraTests
{
    [Test]
    public async Task FirstPerson_Pose_Uses_EyeHeight_And_Look()
    {
        var look = new YawPitchController { Position = new Vector3(1, 0, 2), Yaw = 0f, Pitch = 0f };
        var fp = new FirstPersonCameraRig(look, eyeHeight: 1.7f);
        fp.Tick(0.016f);
        await Assert.That(fp.Pose.Position.Y).IsEqualTo(1.7f);
        await Assert.That(fp.Pose.Position.X).IsEqualTo(1f);
        await Assert.That(fp.Pose.Target.Z).IsGreaterThan(fp.Pose.Position.Z);
    }

    [Test]
    public async Task ThirdPerson_Clamps_Pitch()
    {
        var look = new YawPitchController();
        var tp = new ThirdPersonCameraRig(look) { MinPitch = -0.2f, MaxPitch = 0.5f };
        tp.ApplyLook(new LookIntent(0f, 10f));
        await Assert.That(look.Pitch).IsEqualTo(0.5f);
        tp.Tick(0.016f);
        await Assert.That(fpDistance(tp.Pose.Position, tp.Pose.Target)).IsGreaterThan(0.4f);
    }

    [Test]
    public async Task Director_Switches_Modes()
    {
        var director = new CharacterCameraDirector();
        director.Look.Position = new Vector3(0, 0, 0);
        director.SetMode(CharacterCameraMode.ThirdPerson);
        var tp = director.Tick(0.016f);
        director.SetMode(CharacterCameraMode.FirstPerson);
        var fp = director.Tick(0.016f);
        await Assert.That(fp.Position.Y).IsGreaterThan(tp.Position.Y - 0.01f);
        await Assert.That(director.Mode).IsEqualTo(CharacterCameraMode.FirstPerson);
    }

    [Test]
    public async Task Motor_Jump_Leaves_Ground()
    {
        var look = new YawPitchController { Position = new Vector3(0, 0, 0) };
        var motor = new CharacterMotor(look) { GroundY = 0f, IsGrounded = true };
        motor.Tick(new MoveIntent(Vector3.Zero, Jump: true), 0.05f);
        await Assert.That(motor.IsGrounded).IsFalse();
        await Assert.That(look.Position.Y).IsGreaterThan(0f);
    }

    [Test]
    public async Task Motor_Wish_Uses_Yaw_Forward()
    {
        var look = new YawPitchController { Position = Vector3.Zero, Yaw = 0f };
        var motor = new CharacterMotor(look) { WalkSpeed = 10f, IsGrounded = true, GroundY = 0f };
        motor.Tick(new MoveIntent(new Vector3(0, 0, 1)), 1f);
        await Assert.That(look.Position.Z).IsEqualTo(10f).Within(0.01f);
        await Assert.That(MathF.Abs(look.Position.X)).IsLessThan(0.01f);
    }

    static float fpDistance(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
}
