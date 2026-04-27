using Models;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    public class FinanciamentoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new FinanciamentoViewModel());
        }

        [HttpPost]
        public IActionResult Index(FinanciamentoViewModel model)
        {
            if (model.ValorBem <= 0 || model.PrazoMeses <= 0)
                return View(model);

            decimal subsidio = 0m;

            decimal valorFinanciado = model.ValorBem - model.Entrada;

            if (model.UsaFgts && model.ValorFgts > 0)
                valorFinanciado -= model.ValorFgts;

            if (model.MinhaCasaMinhaVida)
            {
                subsidio = CalcularSubsidioMinhaCasaMinhaVida(model.RendaMensal);
                valorFinanciado -= subsidio;
            }

            if (valorFinanciado < 0)
                valorFinanciado = 0;

            model.ValorFinanciado = valorFinanciado;

            decimal taxaAnualPercentual = ObterTaxaJurosAnual(model.RendaMensal, model.MinhaCasaMinhaVida);
            decimal taxaMensal = ConverterTaxaAnualParaMensal(taxaAnualPercentual);

            if (model.Sistema == "SAC")
                CalcularSac(model, taxaMensal);
            else
                CalcularPrice(model, taxaMensal);

            decimal limiteParcela = model.RendaMensal * 0.30m;
            decimal comprometimentoRenda = model.RendaMensal > 0
                ? (model.Parcela / model.RendaMensal) * 100
                : 0;

            ViewBag.Subsidio = subsidio;
            ViewBag.TaxaJurosAnual = taxaAnualPercentual;
            ViewBag.TaxaJurosMensal = taxaMensal * 100;
            ViewBag.LimiteParcela = limiteParcela;
            ViewBag.ParcelaAprovada = model.Parcela <= limiteParcela;
            ViewBag.ComprometimentoRenda = comprometimentoRenda;

            return View(model);
        }

        private void CalcularPrice(FinanciamentoViewModel model, decimal taxa)
        {
            model.Parcelas.Clear();

            if (model.ValorFinanciado <= 0 || model.PrazoMeses <= 0)
                return;

            decimal parcela;

            if (taxa <= 0)
            {
                parcela = model.ValorFinanciado / model.PrazoMeses;
            }
            else
            {
                double i = (double)taxa;
                double pv = (double)model.ValorFinanciado;
                int n = model.PrazoMeses;

                parcela = (decimal)(pv * (i * Math.Pow(1 + i, n)) / (Math.Pow(1 + i, n) - 1));
            }

            decimal saldo = model.ValorFinanciado;
            decimal totalPago = 0;

            for (int mes = 1; mes <= model.PrazoMeses; mes++)
            {
                decimal juros = saldo * taxa;
                decimal amortizacao = parcela - juros;

                saldo -= amortizacao;

                if (saldo < 0)
                    saldo = 0;

                model.Parcelas.Add(new ParcelaFinanciamentoVm
                {
                    Numero = mes,
                    ValorParcela = parcela,
                    Juros = juros,
                    Amortizacao = amortizacao,
                    SaldoDevedor = saldo
                });

                totalPago += parcela;
            }

            model.Parcela = parcela;
            model.TotalPago = totalPago;
            model.TotalJuros = totalPago - model.ValorFinanciado;
        }

        private void CalcularSac(FinanciamentoViewModel model, decimal taxa)
        {
            model.Parcelas.Clear();

            if (model.ValorFinanciado <= 0 || model.PrazoMeses <= 0)
                return;

            decimal saldo = model.ValorFinanciado;
            decimal amortizacao = model.ValorFinanciado / model.PrazoMeses;
            decimal totalPago = 0;

            for (int mes = 1; mes <= model.PrazoMeses; mes++)
            {
                decimal juros = saldo * taxa;
                decimal parcela = amortizacao + juros;

                saldo -= amortizacao;

                if (saldo < 0)
                    saldo = 0;

                model.Parcelas.Add(new ParcelaFinanciamentoVm
                {
                    Numero = mes,
                    ValorParcela = parcela,
                    Juros = juros,
                    Amortizacao = amortizacao,
                    SaldoDevedor = saldo
                });

                totalPago += parcela;
            }

            model.Parcela = model.Parcelas.First().ValorParcela;
            model.TotalPago = totalPago;
            model.TotalJuros = totalPago - model.ValorFinanciado;
        }

        private decimal ObterTaxaJurosAnual(decimal rendaMensal, bool mcmv)
        {
            if (mcmv)
            {
                if (rendaMensal <= 2640)
                    return 4.50m;

                if (rendaMensal <= 4400)
                    return 4.75m;

                if (rendaMensal <= 8000)
                    return 5.50m;

                return 7.00m;
            }

            if (rendaMensal <= 3000)
                return 10.50m;

            if (rendaMensal <= 6000)
                return 9.50m;

            if (rendaMensal <= 10000)
                return 8.80m;

            return 8.20m;
        }

        private decimal ConverterTaxaAnualParaMensal(decimal taxaAnualPercentual)
        {
            double taxaAnual = (double)(taxaAnualPercentual / 100);
            double taxaMensal = Math.Pow(1 + taxaAnual, 1.0 / 12.0) - 1;

            return (decimal)taxaMensal;
        }

        private decimal CalcularSubsidioMinhaCasaMinhaVida(decimal rendaMensal)
        {
            if (rendaMensal <= 2640)
                return 55000;

            if (rendaMensal <= 4400)
                return 40000;

            if (rendaMensal <= 8000)
                return 20000;

            return 0;
        }
    }
}