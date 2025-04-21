using Godot;
using System.Collections.Generic;

public static class RNGExtensions
{
    public static T RandChoice<T>(this RandomNumberGenerator rng, IList<T> collection)
    {
        return collection[rng.RandiRange(0, collection.Count - 1)];
    }
}