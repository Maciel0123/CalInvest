using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class InvestmentInputModel
    {
        [Display(Name = "Valor inicial")]
        [Range(0, double.MaxValue, ErrorMessage = "Informe um valor válido.")]
        public decimal? InitialAmount { get; set; }

        [Display(Name = "Aporte mensal")]
        [Range(0, double.MaxValue, ErrorMessage = "Informe um valor válido.")]
        public decimal? MonthlyContribution { get; set; }

        [Display(Name = "Taxa de juros mensal (%)")]
        [Range(0, 100, ErrorMessage = "Informe uma taxa válida.")]
        public decimal? MonthlyInterestRate { get; set; }

        [Display(Name = "Prazo (meses)")]
        [Range(1, 1200, ErrorMessage = "Informe um prazo válido.")]
        public int? Months { get; set; }
    }
}