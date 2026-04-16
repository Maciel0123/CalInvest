using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class PortfolioSuggestionRequest
    {
        [Required]
        public string InvestorProfile { get; set; } = "";

        [Range(18, 100)]
        public int? Age { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MonthlyIncome { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? AvailableAmount { get; set; }

        [Required]
        public string InvestmentGoal { get; set; } = "";

        [Range(1, 60)]
        public int? TimeHorizonYears { get; set; }

        public bool HasEmergencyReserve { get; set; }
    }
}