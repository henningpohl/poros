using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpColorPalette {

    // https://flatuicolors.com/palette/nl
    private static List<Color> colors = new List<Color>() {
        FromHex("#FFC312"),
        FromHex("#C4E538"),
        FromHex("#12CBC4"),
        FromHex("#FDA7DF"),
        FromHex("#ED4C67"),
        FromHex("#F79F1F"),
        FromHex("#A3CB38"),
        FromHex("#1289A7"),
        FromHex("#D980FA"),
        FromHex("#B53471"),
        FromHex("#EE5A24"),
        FromHex("#009432"),
        FromHex("#0652DD"),
        FromHex("#9980FA"),
        FromHex("#833471"),
        FromHex("#EA2027"),
        FromHex("#006266"),
        FromHex("#1B1464"),
        FromHex("#5758BB"),
        FromHex("#6F1E51")
    };

    private static System.Random random = new System.Random();

    public static Color GetColor() {
        if(colors.Count == 0) {
            return UnityEngine.Random.ColorHSV();
        }

        int i = random.Next(colors.Count);
        var color = colors[i];
        colors.RemoveAt(i);
        return color;
    }

    public static void ReleaseColor(Color color) {
        colors.Add(color);
    }

    private static Color FromHex(string colorString) {
        int num = Int32.Parse(colorString.Substring(1), System.Globalization.NumberStyles.HexNumber);
        float red = (float)((num >> 16) & 0xFF) / 255.0f;
        float green = (float)((num >> 8) & 0xFF) / 255.0f;
        float blue = (float)((num >> 0) & 0xFF) / 255.0f;

        return new Color(red, green, blue);
    }

}
