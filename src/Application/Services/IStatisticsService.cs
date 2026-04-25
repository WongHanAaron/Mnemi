using System;
using System.Threading.Tasks;

namespace Mnemi.Application.Services;

public interface IStatisticsService
{
    Task<QuickStats> GetQuickStatsAsync(Guid userId);
}

public record QuickStats(int CardsStudied, int DayStreak, string MotivationMessage);
