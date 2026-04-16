using System.ComponentModel.DataAnnotations;
using Models;

namespace Models
{
    public class InvestmentAdvisorViewModel
    {
        [Display(Name = "Perfil do investidor")]
        [Required(ErrorMessage = "Informe o perfil do investidor.")]
        public string InvestorProfile { get; set; } = "";

        [Display(Name = "Idade")]
        [Range(18, 100, ErrorMessage = "Informe uma idade válida.")]
        public int? Age { get; set; }

        [Display(Name = "Renda mensal")]
        [Range(0, double.MaxValue, ErrorMessage = "Informe um valor válido.")]
        public decimal? MonthlyIncome { get; set; }

        [Display(Name = "Valor disponível para investir")]
        [Range(0, double.MaxValue, ErrorMessage = "Informe um valor válido.")]
        public decimal? AvailableAmount { get; set; }

        [Display(Name = "Objetivo do investimento")]
        [Required(ErrorMessage = "Informe o objetivo.")]
        public string InvestmentGoal { get; set; } = "";

        [Display(Name = "Horizonte de tempo (anos)")]
        [Range(1, 60, ErrorMessage = "Informe um prazo válido.")]
        public int? TimeHorizonYears { get; set; }

        [Display(Name = "Já possui reserva de emergência?")]
        public bool HasEmergencyReserve { get; set; }

        public PortfolioSuggestionResponse? Result { get; set; }
    }
}