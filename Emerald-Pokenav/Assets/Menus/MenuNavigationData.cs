using UnityEngine;

public static class MenuNavigationData
{
    // Texto que se mostrar¨¢ en el subtitle de la siguiente escena
    public static string NextSubtitleText;

    // Color del recuadro del subtitle de la siguiente escena
    public static Color NextSubtitleColor;

    // Sirve para saber si realmente hemos enviado datos
    public static bool HasPendingData = false;

    // M¨¦todo auxiliar para guardar la informaci¨®n antes de cambiar de escena
    public static void SetSubtitleData(string subtitleText, Color subtitleColor)
    {
        NextSubtitleText = subtitleText;
        NextSubtitleColor = subtitleColor;
        HasPendingData = true;
    }

    // M¨¦todo auxiliar para limpiar los datos si quieres que no se reutilicen
    public static void Clear()
    {
        NextSubtitleText = string.Empty;
        NextSubtitleColor = Color.white;
        HasPendingData = false;
    }
}