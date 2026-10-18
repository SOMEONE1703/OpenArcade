namespace OpenArcade.Domain.Ranking;

public static class TrendingScore
{
    /// <summary>
    /// Trending this week: recent activity divided by the game's usual weekly activity, so a small game
    /// that suddenly gets popular can rise. Weights are a starting point and easy to tune.
    /// </summary>
    public static double Compute(int plays7d, int likes7d, int comments7d, int favorites7d, double baselineWeekly)
    {
        var activity = plays7d + 3.0 * likes7d + 2.0 * comments7d + 4.0 * favorites7d;
        // The floor of 10 keeps brand-new games with a handful of plays from topping the list.
        return activity / Math.Max(baselineWeekly, 10);
    }
}
