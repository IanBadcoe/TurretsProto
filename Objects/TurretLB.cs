using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class TurretLB : LogicBlock<TurretLB.State> {
    public override Transition GetInitialState() => To<State.WaitingToStart>();

    public sealed record Data
    {
        public Actor Target { get; set; }
    }

    public TurretLB()
    {
        Set(new Data());
    }

    public static class Input
    {
        public readonly record struct Start;
        public readonly record struct FoundTarget(Actor target);
        public readonly record struct NoTargetFound;
        public readonly record struct EnemyAdded;
        public readonly record struct TrackComplete;
        public readonly record struct FireComplete;
        public readonly record struct CooldownComplete;
        public readonly record struct TargetLost;
    }

    public static class Output
    {
        public readonly record struct FindTarget;
        public readonly record struct TrackTo(Actor Actor);
        public readonly record struct FireOn(Actor Actor);
        public readonly record struct Cooldown;
    }

    public abstract partial record State : StateLogic<State> {
        public record WaitingToStart
            : State
            , IGet<Input.Start>
        {
            public Transition On(in Input.Start input) => To<FindingTarget>();
        }

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
                Get<Data>().Target = input.target;
                return To<Tracking>();
            }

            public Transition On(in Input.NoTargetFound _) => To<NoTarget>();
        }

        public record NoTarget
            : State
            , IGet<Input.EnemyAdded>
        {
            public Transition On(in Input.EnemyAdded _) => To<FindingTarget>();
        }

        public record Tracking
            : State
            , IGet<Input.TrackComplete>
            , IGet<Input.TargetLost>
        {
            public Tracking()
            {
                this.OnEnter(() => TrackToTarget());
            }

            public Transition On(in Input.TrackComplete _) => To<Firing>();

            public Transition On(in Input.TargetLost _) => To<FindingTarget>();

            private void TrackToTarget()
            {
                Output(new Output.TrackTo(Get<Data>().Target));
            }
        }

        public record Firing
            : State
            , IGet<Input.FireComplete>
            , IGet<Input.TargetLost>
        {
            public Firing()
            {
                this.OnEnter(() => FireOnTarget());
            }

            public Transition On(in Input.FireComplete _) => To<Coolingdown>();

            public Transition On(in Input.TargetLost input) => To<FindingTarget>();

            private void FireOnTarget()
            {
                Output(new Output.FireOn(Get<Data>().Target));
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