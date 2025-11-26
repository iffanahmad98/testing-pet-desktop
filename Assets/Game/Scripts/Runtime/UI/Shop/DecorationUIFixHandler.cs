using UnityEngine;

public static class DecorationUIFixHandler {
    public static string lastDecorId;
    public static void SetDecorationStats (string value) { // DecorationShopManager
        lastDecorId = value;
    }

    public static string GetDecorationStats () { // DecorationCardUI
        return lastDecorId;
    }
}
