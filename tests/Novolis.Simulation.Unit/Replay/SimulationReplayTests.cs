using Novolis.Simulation.Abstractions;
using Novolis.Simulation.Replay;
using TUnit.Core;

namespace Novolis.Simulation.Unit.Replay;

public sealed class SimulationReplayTests
{
    [Test]
    public async Task Recorder_BuildsTimeline_VerifyAllStepsPasses()
    {
        var recorder = new InMemorySimulationRecorder<int>();
        recorder.SetInitial(0);

        var state = 0;
        for (var i = 0; i < 3; i++)
        {
            var step = new SimulationStep(0.5, (ulong)(i + 1));
            state += (int)step.Tick;
            recorder.RecordStep(step, state, stepSeed: (int)step.Tick);
        }

        var timeline = recorder.Build();
        var runner = new AddTickRunner();

        await Assert.That(timeline.Steps.Count).IsEqualTo(3);
        await Assert.That(ReplayPlayback.GetEndStateAt(timeline, -1)).IsEqualTo(0);
        await Assert.That(ReplayPlayback.GetEndStateAt(timeline, 2)).IsEqualTo(6);
        await Assert.That(ReplayPlayback.VerifyAllSteps(timeline, runner)).IsTrue();
    }

    private sealed class AddTickRunner : ISimulationStepRunner<int>
    {
        public int RunStep(int startState, SimulationStep step, int stepSeed) =>
            startState + (int)step.Tick;
    }
}
