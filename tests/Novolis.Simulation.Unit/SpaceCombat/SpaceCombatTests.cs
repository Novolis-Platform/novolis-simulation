using System.Numerics;
using Novolis.Simulation.SpaceCombat;
using Novolis.Simulation.View;

namespace Novolis.Simulation.Unit.SpaceCombat;

public class SpaceCombatTests
{
    [Test]
    public async Task ArcadeFlight_AdvancesPosition()
    {
        var craft = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 20f };
        craft.ResetVitals();
        var intent = new FlightIntent { ThrottleUp = 1f };
        ArcadeFlight.Apply(craft, intent, 0.5f);
        await Assert.That(craft.Position.Length()).IsGreaterThan(0.1f);
    }

    [Test]
    public async Task SegmentHitsSphere_DetectsHit()
    {
        var hit = CombatHits.SegmentHitsSphere(
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 10),
            new Vector3(0, 0, 5),
            1f);
        await Assert.That(hit).IsTrue();
    }

    [Test]
    public async Task MissionSession_TransferAndComplete()
    {
        var session = new MissionSession(new MissionDescriptor
        {
            Id = "test",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 0.01f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 2);
        session.Begin();

        for (var i = 0; i < 40 && !session.CanTransfer; i++)
            session.Tick(default, 0.05f);

        await Assert.That(session.CanTransfer).IsTrue();
        session.Tick(new FlightIntent { Transfer = true }, 0.016f);
        await Assert.That(session.Phase).IsEqualTo(MissionPhase.Fighter);

        var target = session.Hostiles.First(h => h.Active);
        for (var i = 0; i < 80 && session.Phase == MissionPhase.Fighter; i++)
        {
            target.Position = session.Player.Position + session.Player.Forward * 8f;
            session.Tick(new FlightIntent { Fire = true }, 0.05f);
        }

        await Assert.That(session.Phase).IsEqualTo(MissionPhase.Complete);
    }

    [Test]
    public async Task CraftCamera_Cockpit_HasForwardTarget()
    {
        var pose = CraftCamera.Cockpit(Vector3.Zero, new Vector3(0, 0, 1), 0f);
        await Assert.That(pose.Target.Z).IsGreaterThan(pose.Position.Z);
    }
}
