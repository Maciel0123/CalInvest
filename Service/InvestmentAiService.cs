using System.Text;
using System.Text.Json;
using Models;

namespace Services
{
    public class InvestmentAiService : IInvestmentAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public InvestmentAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PortfolioSuggestionResponse> GeneratePortfolioSuggestionAsync(PortfolioSuggestionRequest request)
        {
            var prompt = BuildPrompt(request);

            var apiKey = _configuration["Ai:ApiKey"];
            var endpoint = _configuration["Ai:Endpoint"];

            var payload = new
            {
                prompt = prompt
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync();

            // Aqui assumimos que a API já devolve JSON compatível com PortfolioSuggestionResponse
            var result = JsonSerializer.Deserialize<PortfolioSuggestionResponse>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
                throw new Exception("Não foi possível gerar a sugestão de carteira.");

            return result;
        }

        private static string BuildPrompt(PortfolioSuggestionRequest request)
        {
            return $@"
Você é um especialista em alocação de carteira para fins educacionais.
Com base nos dados abaixo, gere uma sugestão de carteira de investimentos.

Dados do investidor:
- Perfil declarado: {request.InvestorProfile}
- Idade: {request.Age}
- Renda mensal: {request.MonthlyIncome}
- Valor disponível para investir: {request.AvailableAmount}
- Objetivo: {request.InvestmentGoal}
- Horizonte de tempo em anos: {request.TimeHorizonYears}
- Possui reserva de emergência: {(request.HasEmergencyReserve ? "Sim" : "Não")}

Regras:
- Responda SOMENTE em JSON válido.
- Não use markdown.
- A soma dos percentuais deve ser 100.
- Considere prudência se o usuário não tiver reserva de emergência.
- A resposta deve seguir exatamente esta estrutura:

{{
  ""profileSummary"": ""string"",
  ""recommendationTitle"": ""string"",
  ""rationale"": ""string"",
  ""emergencyReservePercent"": 0,
  ""fixedIncomePercent"": 0,
  ""realEstateFundsPercent"": 0,
  ""stocksPercent"": 0,
  ""internationalPercent"": 0,
  ""recommendations"": [""string""],
  ""alerts"": [""string""]
}}

Importante:
- Não cite ativos específicos.
- Foque em classes de ativos.
- O conteúdo é educacional e não substitui recomendação profissional.
";
        }
    }
}