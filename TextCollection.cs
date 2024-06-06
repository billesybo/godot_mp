using System;
using System.Collections.Generic;
using Godot;

namespace MPTest;

public class TextCollection
{
    private static List<string> _names = new List<string>()
        { "Bob", "Fred", "Thyra", "Wet Willie", "Catnip", "Theobald", "Duckmanager", "Craycray", "Mehfest"};
    public static string GetRandomName()
    {
        Random random = new Random();
        return _names[GD.RandRange(0, _names.Count - 1)];
    }
}