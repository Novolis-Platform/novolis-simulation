using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Active character camera mode.</summary>
public enum CharacterCameraMode
{
    /// <summary>First-person eye camera.</summary>
    FirstPerson = 0,

    /// <summary>Third-person boom camera.</summary>
    ThirdPerson = 1,

    /// <summary>Orbit rig around the character.</summary>
    Orbit = 2
}

/// <summary>
/// Shares one <see cref="YawPitchController"/> across FP / TP / Orbit and exposes the active <see cref="IViewController"/>.
/// </summary>
public sealed class CharacterCameraDirector
{
    readonly FirstPersonCameraRig _firstPerson;
    readonly ThirdPersonCameraRig _thirdPerson;
    readonly OrbitCameraRig _orbit;
    readonly OrbitViewAdapter _orbitAdapter;

    /// <summary>Creates a director with shared look state.</summary>
    public CharacterCameraDirector(
        YawPitchController? look = null,
        float eyeHeight = 1.7f,
        float boomDistance = 4f)
    {
        Look = look ?? new YawPitchController();
        _firstPerson = new FirstPersonCameraRig(Look, eyeHeight);
        _thirdPerson = new ThirdPersonCameraRig(Look, boomDistance);
        _orbit = new OrbitCameraRig
        {
            Distance = Math.Max(boomDistance, 3f),
            MinDistance = 1f,
            MaxDistance = 80f,
            FieldOfViewDegrees = 60f
        };
        _orbitAdapter = new OrbitViewAdapter(_orbit);
        Mode = CharacterCameraMode.FirstPerson;
    }

    /// <summary>Shared feet position + yaw/pitch.</summary>
    public YawPitchController Look { get; }

    /// <summary>First-person rig.</summary>
    public FirstPersonCameraRig FirstPerson => _firstPerson;

    /// <summary>Third-person rig.</summary>
    public ThirdPersonCameraRig ThirdPerson => _thirdPerson;

    /// <summary>Orbit rig (target synced from <see cref="Look"/> each tick).</summary>
    public OrbitCameraRig Orbit => _orbit;

    /// <summary>Active mode.</summary>
    public CharacterCameraMode Mode { get; private set; }

    /// <summary>Active controller for the current mode.</summary>
    public IViewController Active => Mode switch
    {
        CharacterCameraMode.ThirdPerson => _thirdPerson,
        CharacterCameraMode.Orbit => _orbitAdapter,
        _ => _firstPerson
    };

    /// <summary>Switches mode; syncs orbit target/yaw/pitch from shared look.</summary>
    public void SetMode(CharacterCameraMode mode)
    {
        Mode = mode;
        if (mode == CharacterCameraMode.Orbit)
        {
            _orbit.Target = Look.Position + new Vector3(0f, _thirdPerson.ShoulderHeight, 0f);
            _orbit.Yaw = Look.Yaw;
            _orbit.Pitch = Math.Clamp(Look.Pitch, -0.1f, MathF.PI * 0.49f);
            _orbit.SnapTarget(_orbit.Target);
        }
    }

    /// <summary>Applies look intent to the active mode.</summary>
    public void ApplyLook(in LookIntent intent)
    {
        switch (Mode)
        {
            case CharacterCameraMode.ThirdPerson:
                _thirdPerson.ApplyLook(intent);
                break;
            case CharacterCameraMode.Orbit:
                _orbit.AddLookDelta(intent.DeltaYaw, intent.DeltaPitch);
                if (MathF.Abs(intent.ZoomDelta) > 1e-6f)
                    _orbit.AdjustDistance(intent.ZoomDelta);
                Look.Yaw = _orbit.Yaw;
                Look.Pitch = _orbit.Pitch;
                break;
            default:
                _firstPerson.ApplyLook(intent);
                break;
        }
    }

    /// <summary>Ticks the active camera; keeps orbit target on the character.</summary>
    public ViewPose Tick(float deltaSeconds)
    {
        switch (Mode)
        {
            case CharacterCameraMode.ThirdPerson:
                _thirdPerson.Tick(deltaSeconds);
                return _thirdPerson.Pose;
            case CharacterCameraMode.Orbit:
                _orbit.Target = Look.Position + new Vector3(0f, _thirdPerson.ShoulderHeight, 0f);
                return _orbit.BuildViewPose(deltaSeconds);
            default:
                _firstPerson.Tick(deltaSeconds);
                return _firstPerson.Pose;
        }
    }

    /// <summary>Adapts <see cref="OrbitCameraRig"/> to <see cref="IViewController"/> for <see cref="Active"/>.</summary>
    sealed class OrbitViewAdapter : IViewController
    {
        readonly OrbitCameraRig _rig;
        ViewPose _pose;

        public OrbitViewAdapter(OrbitCameraRig rig) => _rig = rig;

        public ViewPose Pose => _pose;

        public void Tick(float deltaSeconds) => _pose = _rig.BuildViewPose(deltaSeconds);
    }
}
