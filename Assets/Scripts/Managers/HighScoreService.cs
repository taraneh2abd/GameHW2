using UnityEngine;

public static class HighScoreService
{
    public const string PrefsKey = "HighScore";

    public static int Load()
    {
        return PlayerPrefs.GetInt(PrefsKey, 0);
    }

    public static bool TrySubmit(int score)
    {
        if (score <= Load())
        {
            return false;
        }

        PlayerPrefs.SetInt(PrefsKey, score);
        PlayerPrefs.Save();
        return true;
    }
}
