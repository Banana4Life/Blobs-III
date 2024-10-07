using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LD56;

public record UnlockableColor(Color Color, Material Material);

public static class UnlockableColors
{
    public static IReadOnlyDictionary<string, UnlockableColor> Colors = new Dictionary<string, UnlockableColor>()
    {
        { "Pure Red", new UnlockableColor(Color.FromHtml("#ff0000"), null) },
        { "Pure Green", new UnlockableColor(Color.FromHtml("#00ff00"), null) },
        { "Pure Blue", new UnlockableColor(Color.FromHtml("#0000ff"), null) },
        { "Dark Orange", new UnlockableColor(Color.FromHtml("#ff8c00"), null) },
        { "Dark Orchid", new UnlockableColor(Color.FromHtml("#9932cc"), null) },
        { "Dark Red", new UnlockableColor(Color.FromHtml("#8b0000"), null) },
        { "Dark Sea Green", new UnlockableColor(Color.FromHtml("#8fbc8f"), null) },
        { "Forest Green", new UnlockableColor(Color.FromHtml("#228b22"), null) },
        { "Gold", new UnlockableColor(Color.FromHtml("#ffd700"), null) },
        { "Golden Rod", new UnlockableColor(Color.FromHtml("#daa520"), null) },
        { "Green Yellow", new UnlockableColor(Color.FromHtml("#adff2f"), null) },
        { "Indigo", new UnlockableColor(Color.FromHtml("#4b0082"), null) },
        { "Khaki", new UnlockableColor(Color.FromHtml("#f0e68c"), null) },
        { "Pure White", new UnlockableColor(Color.FromHtml("#ffffff"), null) },
        { "Magenta", new UnlockableColor(Color.FromHtml("#ff00ff"), null) },
        { "Telekom Magenta", new UnlockableColor(Color.FromHtml("#bc4077"), null) },
        { "Glow Orange", new UnlockableColor(Color.FromHtml("#ff2300"), null) },
        { "Postgelb", new UnlockableColor(Color.FromHtml("#e65f00"), null) },
        { "Quark", new UnlockableColor(Color.FromHtml("#e7f1e6"), null) },
        { "Pure Yellow", new UnlockableColor(Color.FromHtml("#ffff00"), null) },
        { "Tomato", new UnlockableColor(Color.FromHtml("#ff6347"), null) },
        { "Purple", new UnlockableColor(Color.FromHtml("#800080"), null) },
        { "Gray", new UnlockableColor(Color.FromHtml("#808080"), null) },
        { "Silver", new UnlockableColor(Color.FromHtml("#c0c0c0"), null) },
        { "Teal", new UnlockableColor(Color.FromHtml("#008080"), null) },
        { "Olive", new UnlockableColor(Color.FromHtml("#808000"), null) },
        { "Aquamarine", new UnlockableColor(Color.FromHtml("#7fffd4"), null) },
    };

    public static UnlockableColor Default => Colors["PureRed"];

    public static string PickRandomColorName()
    {
        var colors = Colors.Keys.ToList();
        return colors[Global.Instance.Random.RandiRange(0, colors.Count - 1)];
    }
}