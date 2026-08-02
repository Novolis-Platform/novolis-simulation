using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidPoseBakeTests
{
    [Test]
    public async Task BakeLocal_RoundTrip_ReproducesWorldPositions()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var pose = HumanoidPose.FromBind(bind);
        pose[HumanoidBone.LeftArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.35f);
        pose[HumanoidBone.RightUpLeg] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.25f);
        pose.RootTranslation = bind[HumanoidBone.Hips] + new Vector3(0.1f, 0f, -0.05f);

        var world = HumanoidPoseSolver.SolveWorld(bind, pose);
        var baked = new HumanoidPose();
        HumanoidPoseSolver.BakeLocal(bind, world, baked);
        var again = HumanoidPoseSolver.SolveWorld(bind, baked);

        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            var err = Vector3.Distance(world.Position(bone), again.Position(bone));
            await Assert.That(err).IsLessThan(1e-3f);
        }
    }
}
