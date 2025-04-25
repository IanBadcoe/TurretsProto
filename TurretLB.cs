using System;
using System.Collections.Generic;
using System.Diagnostics;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class TurretLB : LogicBlock<TurretLB.State> {
    public override Transition GetInitialState() => To<State.FindingTarget>();

    public sealed record Data
    {
        public WeakReference<Turret> Target { get; set; }
        public List<WeakReference<Turret>> Turrets { get; set; }
    }

    public TurretLB()
    {
        Set(new Data());
    }

    public static class Input
    {
        public readonly record struct FoundTarget(Turret target);
        public readonly record struct NoTargetFound;
        public readonly record struct TurretAdded;
        public readonly record struct TrackComplete;
        public readonly record struct FireComplete;
        public readonly record struct CooldownComplete;
    }

    public static class Output
    {
        public readonly record struct FindTarget;
        public readonly record struct TrackTo(Turret turret);
        public readonly record struct FireOn(Turret turret);
        public readonly record struct Cooldown;
    }

    public abstract partial record State : StateLogic<State> {
        public record FindingTarget
            : State
            , IGet<Input.FoundTarget>
            , IGet<Input.NoTargetFound>
        {
            public FindingTarget()
            {
                this.OnEnter(() => Output(new Output.FindTarget()));
            }
            public Transition On(in Input.FoundTarget input) {
                Get<Data>().Target = new WeakReference<Turret>(input.target);
                return To<Tracking>();
            }

            public Transition On(in Input.NoTargetFound _) => To<NoTarget>();
        }

        public record NoTarget
            : State
            , IGet<Input.TurretAdded>
        {
            public Transition On(in Input.TurretAdded _) => To<FindingTarget>();
        }

        public record Tracking
            : State
            , IGet<Input.TrackComplete>
        {
            public Tracking()
            {
                this.OnEnter(() => TryTrackToTarget());
            }

            public Transition On(in Input.TrackComplete _) => To<Firing>();

            private void TryTrackToTarget()
            {
                if (Get<Data>().Target.TryGetTarget(out Turret target))
                {
                    Output(new Output.TrackTo(target));
                }
                else
                {
                    To<FindingTarget>();
                }
            }
        }

        public record Firing
            : State
            , IGet<Input.FireComplete>
        {
            public Firing()
            {
                this.OnEnter(() => TryFireOnTarget());
            }

            public Transition On(in Input.FireComplete _) => To<Coolingdown>();

            private void TryFireOnTarget()
            {
                if (Get<Data>().Target.TryGetTarget(out Turret target))
                {
                    Output(new Output.FireOn(target));
                }
                else
                {
                    To<FindingTarget>();
                }
            }
        }

        public record Coolingdown
            : State
            , IGet<Input.CooldownComplete>
        {
            public Coolingdown()
            {
                this.OnEnter(() => Output(new Output.Cooldown()));
            }

            public Transition On(in Input.CooldownComplete _) => To<FindingTarget>();
        }
    }
}