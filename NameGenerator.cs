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

    
    static readonly List<string> adjectives = new()
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
        return adjectives[random.Next(0, adjectives.Count)] + " " + 
               animals[random.Next(0, animals.Count)];
    }
}