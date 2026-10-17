namespace OpenArcade.Api.Services;

public static class Ranking
{
    /// <summary>
    /// Lower bound of the Wilson score interval at 95% confidence. A game with 3 likes and no dislikes
    /// ranks below one with 900 likes and 50 dislikes.
    /// </summary>
    public static double WilsonLowerBound(int likes, int dislikes)
    {
        var n = likes + dislikes;
        if (n == 0) return 0;

        const double z = 1.96;
        var p = (double)likes / n;
        var z2 = z * z;
        return (p + z2 / (2 * n) - z * Math.Sqrt((p * (1 - p) + z2 / (4 * n)) / n)) / (1 + z2 / n);
    }

    /// <summary>
    /// Trending this week: recent activity divided by the game's usual weekly activity, so a small game
    /// that suddenly gets popular can rise. Weights are a starting point and easy to tune.
    /// </summary>
    public static double Trending(int plays7d, int likes7d, int comments7d, int favorites7d, double baselineWeekly)
    {
        var activity = plays7d + 3.0 * likes7d + 2.0 * comments7d + 4.0 * favorites7d;
        // The +10 floor keeps brand-new games with a handful of plays from topping the list.
        return activity / Math.Max(baselineWeekly, 10);
    }
}
