using Models;

namespace Services
{
    public interface IInvestmentAiService
    {
        Task<PortfolioSuggestionResponse> GeneratePortfolioSuggestionAsync(PortfolioSuggestionRequest request);
    }
}