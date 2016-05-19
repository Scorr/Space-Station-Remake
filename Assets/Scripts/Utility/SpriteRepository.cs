using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpriteRepository {

    public static Dictionary<string, Sprite[]> dict = new Dictionary<string, Sprite[]>();

    public static Sprite GetSpriteFromSheet(string name) {
        const string query = @"((\w|-|_|\s)+/)*((\w|-|_|\s)+)(_)(\d+)$";

        Match match = Regex.Match(name, query);

        string sprName = match.Groups[3].Value;
        string sprPath = name.Remove(name.LastIndexOf('_'));
        int sprIndex = int.Parse(match.Groups[6].Value);

        Sprite[] group;
        if (!dict.TryGetValue(name, out group)) {
            dict[name] = Resources.LoadAll<Sprite>(sprPath);
        }

        return dict[name][sprIndex];
    }

    public static int GetIndexFromSprite(string name) {
        const string query = @"((\w|-|_|\s)+/)*((\w|-|_|\s)+)(_)(\d+)$";

        Match match = Regex.Match(name, query);

        string sprName = match.Groups[3].Value;
        string sprPath = name.Remove(name.LastIndexOf('_'));
        int sprIndex = int.Parse(match.Groups[6].Value);

        return sprIndex;
    }
}
