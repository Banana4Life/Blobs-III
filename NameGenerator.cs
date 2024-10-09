using System;
using System.Collections.Generic;

namespace LD56;

public static class NameGenerator
{
    static readonly List<string> animals = new()
    {
        "kitten",
        "puppy",
        "bunny",
        "hamster",
        "duckling",
        "chick",
        "foal",
        "calf",
        "fawn",
        "cub",
        "lamb",
        "piglet",
        "mouse",
        "gerbil",
        "guinea",
        "hedgehog",
        "chinchilla",
        "koala",
        "otter",
        "ferret",
        "panda",
        "squirrel",
        "racoon",
        "penguin",
        "seal",
        "sloth",
        "feline",
        "canary",
        "sparrow",
        "owl",
        "bat",
        "goat",
        "pika",
        "vole",
        "mole",
        "weasel",
        "stoat",
        "badger",
        "meerkat",
        "shrew",
        "gecko",
        "frog",
        "toad",
        "turtle",
        "newt",
        "dormouse",
        "lemur",
        "possum",
        "koala",
        "beaver",
        "legionaire"
    };

    private static readonly List<string> robotAdjectives =
    [
        "Intelligent", "Autonomous", "Efficient", "Precise", "Advanced", "Adaptive",
        "Responsive", "Futuristic", "Programmable", "Analytical", "Powerful",
        "Innovative", "Interactive", "Agile", "Synthetic", "Mechanical", "Sleek",
        "Fast", "Reliable", "Intuitive", "Sophisticated", "Networked", "Cognitive",
        "Automated", "Dynamic", "Flexible", "Versatile", "Predictive", "Smart",
        "Logical", "Systematic", "Proactive", "Streamlined", "Modular", "Robust",
        "Compact", "Precautionary", "Scalable", "Computational", "Immersive",
        "Optimized", "Seamless", "Durable", "Strategic", "Integrated", "Responsive",
        "Conscious", "Connected", "Algorithmic", "Self-learning"
    ];


    static readonly List<string> tinyAdjectives = new()
    {
        "adorable",
        "tiny",
        "fluffy",
        "cuddly",
        "playful",
        "charming",
        "petite",
        "precious",
        "sweet",
        "dainty",
        "lovable",
        "delicate",
        "wee",
        "miniature",
        "soft",
        "snuggly",
        "cute",
        "endearing",
        "little",
        "small",
        "mini",
        "baby",
        "innocent",
        "gentle",
        "light",
        "darling",
        "bouncy",
        "squeaky",
        "fuzzy",
        "whimsical",
        "plush",
        "chubby",
        "curious",
        "nimble",
        "happy",
        "angelic",
        "joyful",
        "frisky",
        "friendly",
        "warm",
        "bright",
        "sprightly",
        "spunky",
        "teensy",
        "bubbly",
        "jolly",
        "rotund",
        "plump",
        "perky",
        "legendary"
    };


    public static string RandomName()
    {
        var random = new Random();
        return tinyAdjectives[random.Next(0, tinyAdjectives.Count)] + " " +
               animals[random.Next(0, animals.Count)];
    }

    public static string RandomAIName()
    {
        var random = new Random();
        return robotAdjectives[random.Next(0, tinyAdjectives.Count)] + " " +
               animals[random.Next(0, animals.Count)];
    }
}