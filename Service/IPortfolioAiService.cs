using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Models;
using Options;

namespace Services
{
    public interface IPortfolioAiService
    {
        Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request);
    }

    public class PortfolioAiService : IPortfolioAiService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAiOptions _options;

        public PortfolioAiService(HttpClient httpClient, IOptions<OpenAiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PortfolioSuggestionResponse> GenerateAsync(PortfolioSuggestionRequest request)
        {
            var prompt = BuildPrompt(request);

            var payload = new
            {
                model = _options.Model,
                input = prompt
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(httpRequest);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro OpenAI: {response.StatusCode} - {raw}");
            }

            using var doc = JsonDocument.Parse(raw);

            var outputText = doc.RootElement
                .GetProperty("output")[0]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(outputText))
                throw new Exception("A IA não retornou conteúdo.");

            var result = JsonSerializer.Deserialize<PortfolioSuggestionResponse>(
                outputText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
                throw new Exception("Não foi possível interpretar a resposta da IA.");

            var total =
                result.FixedIncomePercent +
                result.StocksPercent +
                result.RealEstateFundsPercent +
                result.InternationalPercent +
                result.EmergencyReservePercent;

            if (total != 100)
                throw new Exception($"A carteira retornada pela IA é inválida. Soma = {total}%.");

            return result;
        }

        private static string BuildPrompt(PortfolioSuggestionRequest request)
        {
            var reserva = request.HasEmergencyReserve ? "Sim" : "Não";

            return $@"
        Você é um assistente financeiro educacional.
        Gere uma sugestão de carteira de investimentos em JSON válido, sem markdown.

        Dados do investidor:
        - Perfil: {request.InvestorProfile}
        - Idade: {request.Age}
        - Renda mensal: {request.MonthlyIncome}
        - Valor disponível: {request.AvailableAmount}
        - Objetivo: {request.InvestmentGoal}
        - Horizonte: {request.TimeHorizonYears} anos
        - Possui reserva de emergência: {reserva}

        Regras:
        - Responder somente JSON.
        - Não citar ativos específicos.
        - Focar em classes de ativos.
        - A soma dos percentuais deve ser exatamente 100.
        - Se não houver reserva de emergência, considerar isso de forma prudente.
        - Tom educacional, claro e objetivo.

        Estrutura obrigatória:
        {{
        ""profileSummary"": ""string"",
        ""recommendationTitle"": ""string"",
        ""rationale"": ""string"",
        ""fixedIncomePercent"": 0,
        ""stocksPercent"": 0,
        ""realEstateFundsPercent"": 0,
        ""internationalPercent"": 0,
        ""emergencyReservePercent"": 0,
        ""recommendations"": [""string""],
        ""alerts"": [""string""]
        }}";
        }
            }
        }