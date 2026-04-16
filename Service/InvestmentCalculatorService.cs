using Models;

namespace Services
{
    public class InvestmentCalculatorService
    {
        public InvestmentResultModel Calculate(InvestmentInputModel input)
        {
            if (!input.InitialAmount.HasValue ||
                !input.MonthlyContribution.HasValue ||
                !input.MonthlyInterestRate.HasValue ||
                !input.Months.HasValue)
            {
                throw new ArgumentException("Dados inválidos para cálculo.");
            }

            decimal balance = input.InitialAmount.Value;
            decimal totalInvested = input.InitialAmount.Value;
            decimal monthlyRate = input.MonthlyInterestRate.Value / 100m;

            for (int i = 1; i <= input.Months.Value; i++)
            {
                balance *= (1 + monthlyRate);
                balance += input.MonthlyContribution.Value;
                totalInvested += input.MonthlyContribution.Value;
            }

            return new InvestmentResultModel
            {
                TotalInvested = totalInvested,
                FinalAmount = balance,
                TotalInterest = balance - totalInvested
            };
        }
    }
}