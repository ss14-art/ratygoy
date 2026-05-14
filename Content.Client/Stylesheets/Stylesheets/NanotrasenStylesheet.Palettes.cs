using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

// PERSISTENCE 2026/05/14 Class updated to not cause build fail
public partial class NanotrasenStylesheet
{
    public override ColorPalette PrimaryPalette => Palettes.Navy;
    public override ColorPalette SecondaryPalette => Palettes.Slate;
    public override ColorPalette PositivePalette => Palettes.Green;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => Palettes.Gold;
}
