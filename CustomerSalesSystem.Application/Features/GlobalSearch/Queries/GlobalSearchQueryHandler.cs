namespace CustomerSalesSystem.Application.Features
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, List<GlobalSearchResultDto>>
    {
        private readonly IGlobalSearchRepository _repository;

        public GlobalSearchQueryHandler(IGlobalSearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<GlobalSearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            var aiRequest = new AIIntentResult
            {
                Intent = request.Intent,
                Entities = request.Entities
            };

            return await _repository.SearchAsync(aiRequest);
        }
    }

}
