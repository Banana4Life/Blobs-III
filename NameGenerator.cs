using System;
using System.Collections.Generic;

namespace LD56;

public static class NameGenerator
{
    static readonly List<String> Names =
    [
        "Bob",
        "Alice",
        "Charlie",
        "Eve",
        "Dave",
        "Grace",
        "Heidi",
        "Ivan",
        "Judy",
        "Mallory"
    ];

    public static string RandomName()
    {
        return Names[new Random().Next(0, Names.Count)];
    }
}