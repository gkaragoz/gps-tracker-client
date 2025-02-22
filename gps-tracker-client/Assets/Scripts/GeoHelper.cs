using System;
using System.Collections.Generic;

public static class GeoHelper
{
    /// <summary>
    /// Returns a dictionary mapping famous locations to their coordinates.
    /// </summary>
    public static readonly Dictionary<FamousLocation, LocationModel> FamousPlaces = new()
    {
        { FamousLocation.EiffelTower, new LocationModel(48.8584f, 2.2945f) },
        { FamousLocation.StatueOfLiberty, new LocationModel(40.6892f, -74.0445f) },
        { FamousLocation.GreatWallOfChina, new LocationModel(40.4319f, 116.5704f) },
        { FamousLocation.Colosseum, new LocationModel(41.8902f, 12.4922f) },
        { FamousLocation.MachuPicchu, new LocationModel(-13.1631f, -72.5450f) },
        { FamousLocation.TajMahal, new LocationModel(27.1751f, 78.0421f) },
        { FamousLocation.PyramidsOfGiza, new LocationModel(29.9792f, 31.1342f) },
        { FamousLocation.BigBen, new LocationModel(51.5007f, -0.1246f) },
        { FamousLocation.SydneyOperaHouse, new LocationModel(-33.8568f, 151.2153f) },
        { FamousLocation.MountEverest, new LocationModel(27.9881f, 86.9250f) },
        { FamousLocation.ChristTheRedeemer, new LocationModel(-22.9519f, -43.2105f) },
        { FamousLocation.BurjKhalifa, new LocationModel(25.1972f, 55.2744f) },
        { FamousLocation.Petra, new LocationModel(30.3285f, 35.4444f) },
        { FamousLocation.AngkorWat, new LocationModel(13.4125f, 103.8667f) },
        { FamousLocation.Stonehenge, new LocationModel(51.1789f, -1.8262f) },
        { FamousLocation.MountFuji, new LocationModel(35.3606f, 138.7274f) },
        { FamousLocation.LeaningTowerOfPisa, new LocationModel(43.7229f, 10.3966f) },
        { FamousLocation.NiagaraFalls, new LocationModel(43.0962f, -79.0377f) },
        { FamousLocation.GrandCanyon, new LocationModel(36.1069f, -112.1129f) },
        { FamousLocation.BuckinghamPalace, new LocationModel(51.5014f, -0.1419f) }
        // Add more places as needed...
    };
}

/// <summary>
/// Enum representing famous places around the world.
/// </summary>
public enum FamousLocation
{
    EiffelTower,
    StatueOfLiberty,
    GreatWallOfChina,
    Colosseum,
    MachuPicchu,
    TajMahal,
    PyramidsOfGiza,
    BigBen,
    SydneyOperaHouse,
    MountEverest,
    ChristTheRedeemer,
    BurjKhalifa,
    Petra,
    AngkorWat,
    Stonehenge,
    MountFuji,
    LeaningTowerOfPisa,
    NiagaraFalls,
    GrandCanyon,
    BuckinghamPalace
    // Add more famous locations as needed...
}