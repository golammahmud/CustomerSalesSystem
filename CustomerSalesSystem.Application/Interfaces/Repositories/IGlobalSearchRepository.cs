namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface IGlobalSearchRepository
    {
        Task<List<GlobalSearchResultDto>> SearchAsync(AIIntentResult request);
    }

}
