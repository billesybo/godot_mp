using System;
using System.Collections.Generic;
using Godot;

namespace MPTest;

public class TextCollection
{
    private static List<string> _names = new List<string>()
        { "Bob", "Fred", "Thyra", "Wet Willie", "Catnip", "Theobald", "Duckmanager", "Craycray", "Mehfest", "Dingbat", "Oaf"};
    public static string GetRandomName()
    {
        Random random = new Random();
        return _names[GD.RandRange(0, _names.Count - 1)];
    }
    
    private static List<string> _ouch = new List<string>()
        { "Ouch!", "Stop it", "Fuck", "lortejob"};
    public static string GetRandomOuch()
    {
        Random random = new Random();
        return _ouch[GD.RandRange(0, _ouch.Count - 1)];
    }
}