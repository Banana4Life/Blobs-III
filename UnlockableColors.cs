using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LD56;

public record UnlockableColor(Color Color, Material Material);

public static class UnlockableColors
{
    public static IReadOnlyDictionary<string, UnlockableColor> Colors = new Dictionary<string, UnlockableColor>()
    {
        { "PureRed", new UnlockableColor(Color.FromHtml("#FF0000"), null) },
        { "PureGreen", new UnlockableColor(Color.FromHtml("#00FF00"), null) },
        { "PureBlue", new UnlockableColor(Color.FromHtml("#0000FF"), null) },
        { "White", new UnlockableColor(Color.FromHtml("#FFFFFF"), null) },
        { "LightYellow", new UnlockableColor(Color.FromHtml("#FFFFE0"), null) },
        { "Gold", new UnlockableColor(Color.FromHtml("#FFD700"), null) },
        { "Yellow", new UnlockableColor(Color.FromHtml("#FFFF00"), null) },
        { "LightGoldenRodYellow", new UnlockableColor(Color.FromHtml("#FAFAD2"), null) },
        { "Khaki", new UnlockableColor(Color.FromHtml("#F0E68C"), null) },
        { "LemonChiffon", new UnlockableColor(Color.FromHtml("#FFFACD"), null) },
        { "PeachPuff", new UnlockableColor(Color.FromHtml("#FFDAB9"), null) },
        { "PapayaWhip", new UnlockableColor(Color.FromHtml("#FFEFD5"), null) },
        { "BlanchedAlmond", new UnlockableColor(Color.FromHtml("#FFEBCD"), null) },
        { "Bisque", new UnlockableColor(Color.FromHtml("#FFE4C4"), null) },
        { "Moccasin", new UnlockableColor(Color.FromHtml("#FFE4B5"), null) },
        { "NavajoWhite", new UnlockableColor(Color.FromHtml("#FFDEAD"), null) },
        { "Cornsilk", new UnlockableColor(Color.FromHtml("#FFF8DC"), null) },
        { "PaleGoldenRod", new UnlockableColor(Color.FromHtml("#EEE8AA"), null) },
        { "Wheat", new UnlockableColor(Color.FromHtml("#F5DEB3"), null) },
        { "Beige", new UnlockableColor(Color.FromHtml("#F5F5DC"), null) },
        { "Linen", new UnlockableColor(Color.FromHtml("#FAF0E6"), null) },
        { "AntiqueWhite", new UnlockableColor(Color.FromHtml("#FAEBD7"), null) },
        { "Snow", new UnlockableColor(Color.FromHtml("#FFFAFA"), null) },
        { "MintCream", new UnlockableColor(Color.FromHtml("#F5FFFA"), null) },
        { "Ivory", new UnlockableColor(Color.FromHtml("#FFFFF0"), null) },
        { "LavenderBlush", new UnlockableColor(Color.FromHtml("#FFF0F5"), null) },
        { "SeaShell", new UnlockableColor(Color.FromHtml("#FFF5EE"), null) },
        { "HoneyDew", new UnlockableColor(Color.FromHtml("#F0FFF0"), null) },
        { "FloralWhite", new UnlockableColor(Color.FromHtml("#FFFAF0"), null) },
        { "AliceBlue", new UnlockableColor(Color.FromHtml("#F0F8FF"), null) },
        { "Lavender", new UnlockableColor(Color.FromHtml("#E6E6FA"), null) },
        { "Gainsboro", new UnlockableColor(Color.FromHtml("#DCDCDC"), null) },
        { "LightGray", new UnlockableColor(Color.FromHtml("#D3D3D3"), null) },
        { "Thistle", new UnlockableColor(Color.FromHtml("#D8BFD8"), null) },
        { "Plum", new UnlockableColor(Color.FromHtml("#DDA0DD"), null) },
        { "LightPink", new UnlockableColor(Color.FromHtml("#FFB6C1"), null) },
        { "Pink", new UnlockableColor(Color.FromHtml("#FFC0CB"), null) },
        { "HotPink", new UnlockableColor(Color.FromHtml("#FF69B4"), null) },
        { "Coral", new UnlockableColor(Color.FromHtml("#FF7F50"), null) },
        { "Tomato", new UnlockableColor(Color.FromHtml("#FF6347"), null) },
        { "OrangeRed", new UnlockableColor(Color.FromHtml("#FF4500"), null) },
        { "Orange", new UnlockableColor(Color.FromHtml("#FFA500"), null) },
        { "Salmon", new UnlockableColor(Color.FromHtml("#FA8072"), null) },
        { "LightSalmon", new UnlockableColor(Color.FromHtml("#FFA07A"), null) },
        { "DeepSkyBlue", new UnlockableColor(Color.FromHtml("#00BFFF"), null) },
        { "PaleTurquoise", new UnlockableColor(Color.FromHtml("#AFEEEE"), null) },
        { "Turquoise", new UnlockableColor(Color.FromHtml("#40E0D0"), null) },
        { "Cyan", new UnlockableColor(Color.FromHtml("#00FFFF"), null) },
        { "Aquamarine", new UnlockableColor(Color.FromHtml("#7FFFD4"), null) },
        { "SpringGreen", new UnlockableColor(Color.FromHtml("#00FF7F"), null) },
        { "MediumSpringGreen", new UnlockableColor(Color.FromHtml("#00FA9A"), null) },
        { "Chartreuse", new UnlockableColor(Color.FromHtml("#7FFF00"), null) },
        { "LawnGreen", new UnlockableColor(Color.FromHtml("#7CFC00"), null) },
    };

    public static UnlockableColor Default => Colors["PureRed"];

    public static string PickRandomColorName()
    {
        var colors = Colors.Keys.ToList();
        return colors[Global.Instance.Random.RandiRange(0, colors.Count - 1)];
    }
}