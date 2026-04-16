using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Models;
using Options;

namespace Services
{
    public interface IGeminiAiService
    {
        Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request);
    }

    public class GeminiAiService : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;

        public GeminiAiService(HttpClient httpClient, IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request)
        {
            var prompt = BuildPrompt(request);

            var url = $"https://generativelanguage.googleapis.com/v1/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro Gemini: {raw}");

            using var doc = JsonDocument.Parse(raw);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Resposta vazia da IA.");

            // limpa possíveis ```json
            text = text.Replace("```json", "").Replace("```", "").Trim();

            var result = JsonSerializer.Deserialize<PortfolioSuggestionResponse>(
                text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
                throw new Exception("Erro ao interpretar JSON.");

            return result;
        }

        private static string BuildPrompt(PortfolioSuggestionRequest request)
    {
        return $$"""
    Você deve gerar uma sugestão educacional de carteira de investimentos para um investidor brasileiro.

    Dados do investidor:
    - Perfil: {{request.InvestorProfile}}
    - Idade: {{request.Age}}
    - Renda mensal: {{request.MonthlyIncome}}
    - Valor disponível para investir: {{request.AvailableAmount}}
    - Objetivo: {{request.InvestmentGoal}}
    - Horizonte de investimento: {{request.TimeHorizonYears}} anos
    - Possui reserva de emergência: {{(request.HasEmergencyReserve ? "Sim" : "Não")}}

    Regras obrigatórias:
    - Responder SOMENTE em JSON válido.
    - Não usar markdown.
    - Não escrever texto antes ou depois do JSON.
    - A soma dos percentuais deve ser exatamente 100.
    - Sugerir classes de ativos e também exemplos reais de ativos brasileiros ou veículos conhecidos.
    - Para renda fixa, pode sugerir exemplos como Tesouro Selic, CDB, LCI, LCA.
    - Para ações, sugerir tickers reais da bolsa brasileira.
    - Para fundos imobiliários, sugerir tickers reais de FIIs.
    - Para internacional, sugerir ETFs ou BDRs conhecidos.
    - Para reserva de emergência, sugerir produtos conservadores e líquidos.

    Estrutura obrigatória do JSON:
    {
    "profileSummary": "string",
    "recommendationTitle": "string",
    "rationale": "string",
    "fixedIncomePercent": 0,
    "stocksPercent": 0,
    "realEstateFundsPercent": 0,
    "internationalPercent": 0,
    "emergencyReservePercent": 0,
    "fixedIncomeExamples": ["string"],
    "stockExamples": ["string"],
    "realEstateFundExamples": ["string"],
    "internationalExamples": ["string"],
    "emergencyReserveExamples": ["string"],
    "recommendations": ["string"],
    "alerts": ["string"]
    }
    """;
    }
        }
}