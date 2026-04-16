namespace Models
{
    public class PortfolioSuggestionResponse
    {
        public string ProfileSummary { get; set; } = "";
        public string RecommendationTitle { get; set; } = "";
        public string Rationale { get; set; } = "";

        public decimal FixedIncomePercent { get; set; }
        public decimal StocksPercent { get; set; }
        public decimal RealEstateFundsPercent { get; set; }
        public decimal InternationalPercent { get; set; }
        public decimal EmergencyReservePercent { get; set; }

        public List<string> Recommendations { get; set; } = new();
        public List<string> Alerts { get; set; } = new();

        public List<string> FixedIncomeExamples { get; set; } = new();
        public List<string> StockExamples { get; set; } = new();
        public List<string> RealEstateFundExamples { get; set; } = new();
        public List<string> InternationalExamples { get; set; } = new();
        public List<string> EmergencyReserveExamples { get; set; } = new();
    }
}