namespace Models
{
    public class FinanciamentoViewModel
    {
        public decimal ValorBem { get; set; }
        public decimal Entrada { get; set; }
        public int PrazoMeses { get; set; }
        public string Sistema { get; set; } = "SAC";

        public decimal ValorFinanciado { get; set; }
        public decimal Parcela { get; set; }
        public decimal TotalPago { get; set; }
        public decimal TotalJuros { get; set; }
        public decimal RendaMensal { get; set; }

        public bool UsaFgts { get; set; }
        public decimal ValorFgts { get; set; }

        public bool MinhaCasaMinhaVida { get; set; }

        public List<ParcelaFinanciamentoVm> Parcelas { get; set; } = new();
    }

    public class ParcelaFinanciamentoVm
    {
        public int Numero { get; set; }
        public decimal ValorParcela { get; set; }
        public decimal Juros { get; set; }
        public decimal Amortizacao { get; set; }
        public decimal SaldoDevedor { get; set; }
    }
}