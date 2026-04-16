using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Models;
using Options;

namespace Services
{
    public interface IGroqAiService
    {
        Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request);
    }

    public class GroqAiService : IGroqAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;

        public GroqAiService(HttpClient httpClient, IOptions<GroqOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request)
        {
            ValidateConfiguration();

            var prompt = BuildPrompt(request);

            var payload = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "Você é um assistente financeiro educacional. Responda somente em JSON válido, sem markdown, sem explicações extras."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.4
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(httpRequest);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(BuildErrorMessage(response.StatusCode, raw));
            }

            using var doc = JsonDocument.Parse(raw);

            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("A IA retornou uma resposta vazia.");

            text = CleanJsonResponse(text);

            if (!text.TrimStart().StartsWith("{"))
                throw new Exception("A IA não retornou um JSON válido.");

            var result = JsonSerializer.Deserialize<PortfolioSuggestionResponse>(
                text,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
                throw new Exception("Não foi possível interpretar a resposta da IA.");

            ValidatePortfolio(result);

            return result;
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new Exception("A chave da API do Groq não foi configurada.");

            if (!_options.ApiKey.Trim().StartsWith("gsk_"))
                throw new Exception("A chave da API do Groq parece inválida.");

            if (string.IsNullOrWhiteSpace(_options.Model))
                throw new Exception("O modelo do Groq não foi configurado.");
        }

        private static string CleanJsonResponse(string text)
        {
            return text
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        private static void ValidatePortfolio(PortfolioSuggestionResponse result)
        {
            var total =
                result.FixedIncomePercent +
                result.StocksPercent +
                result.RealEstateFundsPercent +
                result.InternationalPercent +
                result.EmergencyReservePercent;

            if (total != 100)
                throw new Exception($"A carteira retornada pela IA é inválida. Soma = {total}%.");
        }

        private static string BuildErrorMessage(HttpStatusCode statusCode, string raw)
        {
            if (statusCode == HttpStatusCode.Unauthorized)
                return "API key do Groq inválida ou expirada.";

            if ((int)statusCode == 429)
                return "Limite de uso da IA atingido. Tente novamente em instantes.";

            if (statusCode == HttpStatusCode.BadRequest)
                return $"Requisição inválida para o Groq. Detalhes: {raw}";

            return $"Erro Groq ({(int)statusCode}): {raw}";
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
    - Para reserva de emergência, sugerir produtos conservadores e líquidos e tambem uma quantidade de meses ideal do custo de vida.

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