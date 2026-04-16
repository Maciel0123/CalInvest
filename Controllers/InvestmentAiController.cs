using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers
{
    public class InvestmentAiController : Controller
    {
        private readonly IGroqAiService _aiService;

        public InvestmentAiController(IGroqAiService portfolioAiService)
        {
            _aiService = portfolioAiService;
        }

        [HttpPost]
        public async Task<IActionResult> SuggestPortfolio([FromBody] PortfolioSuggestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _aiService.GenerateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao gerar sugestão de carteira.",
                    detail = ex.Message
                });
            }
        }
    }
}