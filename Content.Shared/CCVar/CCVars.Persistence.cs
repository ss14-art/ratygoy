using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<int>
        YearOffset = CVarDef.Create("lore.year_offset", 120, CVar.ARCHIVE);

    public static readonly CVarDef<int>
        AcceptDeathTime = CVarDef.Create("acceptdeath.time", 60 * 15, CVar.ARCHIVE);

    public static readonly CVarDef<int>
        AcceptDeathSOSTime = CVarDef.Create("acceptdeath.sostime", 60 * 15, CVar.ARCHIVE);

    public static readonly CVarDef<int>
        GridClaimPersonalMaxTiles = CVarDef.Create("gridconfig.claim_personal_max_tiles", 150, CVar.ARCHIVE);

    #region Bluespace Parking
    public static readonly CVarDef<int>
        BluespaceParkingMaxTiles = CVarDef.Create("bsp.max_tiles", 800, CVar.ARCHIVE);
    public static readonly CVarDef<int>
        BluespaceParkingParkDelay = CVarDef.Create("bsp.park_delay", 25, CVar.ARCHIVE);
    public static readonly CVarDef<int>
        BluespaceParkingUnparkDelay = CVarDef.Create("bsp.unpark_delay", 25, CVar.ARCHIVE);
    public static readonly CVarDef<float>
        BluespaceUnparkMaxDistance = CVarDef.Create("bsp.unpark_max_distance", 100f, CVar.ARCHIVE);

    public static readonly CVarDef<string>
        BluespaceParkingDirectory = CVarDef.Create("bsp.save_dir", "BluespaceParking", CVar.SERVERONLY);

    #endregion
}
