using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LD56;

public record UnlockableColor(string name, Color Color, Material Material);

public class UnlockableColors
{
    public IReadOnlyDictionary<string, UnlockableColor> Colors = new Dictionary<string, UnlockableColor>()
    {
        { "PureRed", new UnlockableColor("PureRed", Color.FromHtml("#FF0000"), null) },
        { "PureGreen", new UnlockableColor("PureGreen", Color.FromHtml("#00FF00"), null) },
        { "PureBlue", new UnlockableColor("PureBlue", Color.FromHtml("#0000FF"), null) },
        { "White", new UnlockableColor("White", Color.FromHtml("#FFFFFF"), null) },
        { "LightYellow", new UnlockableColor("LightYellow", Color.FromHtml("#FFFFE0"), null) },
        { "Gold", new UnlockableColor("Gold", Color.FromHtml("#FFD700"), null) },
        { "Yellow", new UnlockableColor("Yellow", Color.FromHtml("#FFFF00"), null) },
        { "LightGoldenRodYellow", new UnlockableColor("LightGoldenRodYellow", Color.FromHtml("#FAFAD2"), null) },
        { "Khaki", new UnlockableColor("Khaki", Color.FromHtml("#F0E68C"), null) },
        { "LemonChiffon", new UnlockableColor("LemonChiffon", Color.FromHtml("#FFFACD"), null) },
        { "PeachPuff", new UnlockableColor("PeachPuff", Color.FromHtml("#FFDAB9"), null) },
        { "PapayaWhip", new UnlockableColor("PapayaWhip", Color.FromHtml("#FFEFD5"), null) },
        { "BlanchedAlmond", new UnlockableColor("BlanchedAlmond", Color.FromHtml("#FFEBCD"), null) },
        { "Bisque", new UnlockableColor("Bisque", Color.FromHtml("#FFE4C4"), null) },
        { "Moccasin", new UnlockableColor("Moccasin", Color.FromHtml("#FFE4B5"), null) },
        { "NavajoWhite", new UnlockableColor("NavajoWhite", Color.FromHtml("#FFDEAD"), null) },
        { "Cornsilk", new UnlockableColor("Cornsilk", Color.FromHtml("#FFF8DC"), null) },
        { "PaleGoldenRod", new UnlockableColor("PaleGoldenRod", Color.FromHtml("#EEE8AA"), null) },
        { "Wheat", new UnlockableColor("Wheat", Color.FromHtml("#F5DEB3"), null) },
        { "Beige", new UnlockableColor("Beige", Color.FromHtml("#F5F5DC"), null) },
        { "Linen", new UnlockableColor("Linen", Color.FromHtml("#FAF0E6"), null) },
        { "AntiqueWhite", new UnlockableColor("AntiqueWhite", Color.FromHtml("#FAEBD7"), null) },
        { "Snow", new UnlockableColor("Snow", Color.FromHtml("#FFFAFA"), null) },
        { "MintCream", new UnlockableColor("MintCream", Color.FromHtml("#F5FFFA"), null) },
        { "Ivory", new UnlockableColor("Ivory", Color.FromHtml("#FFFFF0"), null) },
        { "LavenderBlush", new UnlockableColor("LavenderBlush", Color.FromHtml("#FFF0F5"), null) },
        { "SeaShell", new UnlockableColor("SeaShell", Color.FromHtml("#FFF5EE"), null) },
        { "HoneyDew", new UnlockableColor("HoneyDew", Color.FromHtml("#F0FFF0"), null) },
        { "FloralWhite", new UnlockableColor("FloralWhite", Color.FromHtml("#FFFAF0"), null) },
        { "AliceBlue", new UnlockableColor("AliceBlue", Color.FromHtml("#F0F8FF"), null) },
        { "Lavender", new UnlockableColor("Lavender", Color.FromHtml("#E6E6FA"), null) },
        { "Gainsboro", new UnlockableColor("Gainsboro", Color.FromHtml("#DCDCDC"), null) },
        { "LightGray", new UnlockableColor("LightGray", Color.FromHtml("#D3D3D3"), null) },
        { "Thistle", new UnlockableColor("Thistle", Color.FromHtml("#D8BFD8"), null) },
        { "Plum", new UnlockableColor("Plum", Color.FromHtml("#DDA0DD"), null) },
        { "LightPink", new UnlockableColor("LightPink", Color.FromHtml("#FFB6C1"), null) },
        { "Pink", new UnlockableColor("Pink", Color.FromHtml("#FFC0CB"), null) },
        { "HotPink", new UnlockableColor("HotPink", Color.FromHtml("#FF69B4"), null) },
        { "Coral", new UnlockableColor("Coral", Color.FromHtml("#FF7F50"), null) },
        { "Tomato", new UnlockableColor("Tomato", Color.FromHtml("#FF6347"), null) },
        { "OrangeRed", new UnlockableColor("OrangeRed", Color.FromHtml("#FF4500"), null) },
        { "Orange", new UnlockableColor("Orange", Color.FromHtml("#FFA500"), null) },
        { "Salmon", new UnlockableColor("Salmon", Color.FromHtml("#FA8072"), null) },
        { "LightSalmon", new UnlockableColor("LightSalmon", Color.FromHtml("#FFA07A"), null) },
        { "DeepSkyBlue", new UnlockableColor("DeepSkyBlue", Color.FromHtml("#00BFFF"), null) },
        { "PaleTurquoise", new UnlockableColor("PaleTurquoise", Color.FromHtml("#AFEEEE"), null) },
        { "Turquoise", new UnlockableColor("Turquoise", Color.FromHtml("#40E0D0"), null) },
        { "Cyan", new UnlockableColor("Cyan", Color.FromHtml("#00FFFF"), null) },
        { "Aquamarine", new UnlockableColor("Aquamarine", Color.FromHtml("#7FFFD4"), null) },
        { "SpringGreen", new UnlockableColor("SpringGreen", Color.FromHtml("#00FF7F"), null) },
        { "MediumSpringGreen", new UnlockableColor("MediumSpringGreen", Color.FromHtml("#00FA9A"), null) },
        { "Chartreuse", new UnlockableColor("Chartreuse", Color.FromHtml("#7FFF00"), null) },
        { "LawnGreen", new UnlockableColor("LawnGreen", Color.FromHtml("#7CFC00"), null) },
    };

    public UnlockableColor PickRandomColor()
    {
        var colors = Colors.Values.ToList();
        return colors[Global.Instance.Random.RandiRange(0, colors.Count - 1)];
    }
}