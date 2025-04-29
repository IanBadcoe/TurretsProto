using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot_Util;

public class FactionManager
{
    public enum FactionId
    {
        Player,
        Enemy,          //< add more later
    }

    public class FactionData : IReadOnlyCollection<Actor>
    {
        List<Actor> Actors = [];

        public void Add(Actor actor)
        {
            Util.Assert(!Actors.Contains(actor));

            Actors.Add(actor);

            MemberAdded?.Invoke(actor);
        }

        public void Remove(Actor actor)
        {
            Util.Assert(Actors.Contains(actor));

            Actors.Remove(actor);

            MemberRemoved?.Invoke(actor);
        }

        // --------------------------------------------------------------
        // delegates
        public delegate void MemberChangeDelegate(Actor added);

        public MemberChangeDelegate MemberAdded;

        public MemberChangeDelegate MemberRemoved;
        // --------------------------------------------------------------

        // --------------------------------------------------------------
        // enumeration/collection
        public int Count => Actors.Count;

        public IEnumerator<Actor> GetEnumerator()
        {
            return Actors.Where(x => GodotObject.IsInstanceValid(x.AsNode3D())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        // --------------------------------------------------------------
    }

    Dictionary<FactionId, FactionData> Factions = [];

    public FactionData Faction(FactionId faction)
    {
        if (!Factions.ContainsKey(faction))
        {
            Factions[faction] = new FactionData();
        }

        return Factions[faction];
    }
}