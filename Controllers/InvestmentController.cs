using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers
{
    public class InvestmentController : Controller
    {
        private readonly InvestmentCalculatorService _calculatorService;
        private readonly IGroqAiService _aiService;

        public InvestmentController(IGroqAiService aiService)
        {
            _aiService = aiService;
            _calculatorService = new InvestmentCalculatorService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new InvestmentInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(InvestmentInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = _calculatorService.Calculate(model);
                ViewBag.Result = result;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao calcular investimento: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Advisor()
        {
            return View(new InvestmentAdvisorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Advisor(InvestmentAdvisorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var request = new PortfolioSuggestionRequest
                {
                    InvestorProfile = model.InvestorProfile,
                    Age = model.Age ?? 0,
                    MonthlyIncome = model.MonthlyIncome ?? 0,
                    AvailableAmount = model.AvailableAmount ?? 0,
                    InvestmentGoal = model.InvestmentGoal,
                    TimeHorizonYears = model.TimeHorizonYears ?? 0,
                    HasEmergencyReserve = model.HasEmergencyReserve
                };

                model.Result = await _aiService.GenerateAsync(request);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao gerar sugestão de carteira: {ex.Message}");
                return View(model);
            }
        }
    }
}