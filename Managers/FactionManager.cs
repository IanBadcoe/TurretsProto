using System;
using System.Collections;
using System.Collections.Generic;
using SubD;

public class FactionManager
{
    public enum FactionId
    {
        Player,
        Enemy,          //< add more later
    }

    public class FactionData : IReadOnlyCollection<Turret>
    {
        List<Turret> Turrets = [];

        public void Add(Turret turret)
        {
            Util.Assert(!Turrets.Contains(turret));

            Turrets.Add(turret);

            MemberAdded?.Invoke(turret);
        }

        public void Remove(Turret turret)
        {
            Util.Assert(Turrets.Contains(turret));

            Turrets.Remove(turret);

            MemberRemoved?.Invoke(turret);
        }

        // --------------------------------------------------------------
        // delegates
        public delegate void MemberChangeDelegate(Turret added);

        public MemberChangeDelegate MemberAdded;

        public MemberChangeDelegate MemberRemoved;
        // --------------------------------------------------------------

        // --------------------------------------------------------------
        // enumeration/collection
        public int Count => Turrets.Count;

        public IEnumerator<Turret> GetEnumerator()
        {
            return Turrets.GetEnumerator();
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