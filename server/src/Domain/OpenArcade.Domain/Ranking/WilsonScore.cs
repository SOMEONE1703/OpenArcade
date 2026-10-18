namespace OpenArcade.Domain.Ranking;

public static class WilsonScore
{
    /// <summary>
    /// Lower bound of the Wilson score interval at 95% confidence. A game with 3 likes and no dislikes
    /// ranks below one with 900 likes and 50 dislikes.
    /// </summary>
    public static double LowerBound(int likes, int dislikes)
    {
        var n = likes + dislikes;
        if (n == 0) return 0;

        const double z = 1.96;
        var p = (double)likes / n;
        var z2 = z * z;
        return (p + z2 / (2 * n) - z * Math.Sqrt((p * (1 - p) + z2 / (4 * n)) / n)) / (1 + z2 / n);
    }

    /// <summary>Like percentage for display, or null when there are no votes.</summary>
    public static int? LikePercent(int likes, int dislikes) =>
        likes + dislikes == 0 ? null : (int)Math.Round(100.0 * likes / (likes + dislikes));
}
