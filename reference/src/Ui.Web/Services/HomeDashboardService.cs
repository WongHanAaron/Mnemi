using Mnemi.Application.Home;

namespace Mnemi.Ui.Web.Services;

public sealed class HomeDashboardService(HomeDashboardStubDataProvider stubDataProvider) : IHomeDashboardService
{
    public Task<HomeDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(stubDataProvider.BuildDashboard());
    }
}
