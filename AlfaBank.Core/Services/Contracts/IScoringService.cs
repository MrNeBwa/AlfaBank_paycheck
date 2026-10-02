using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Выполняет автоматическую проверку (скоринг) заёмщика.
/// </summary>
public interface IScoringService
{
    /// <summary>
    /// Оценивает платёжеспособность заёмщика и выносит рекомендацию по заявке.
    /// </summary>
    /// <param name="input">Данные заёмщика: возраст, стаж, доход, наличие просрочек.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Балльная оценка, показатель долговой нагрузки и вывод.</returns>
    ScoringResultDto Evaluate(ScoringInput input, decimal monthlyPayment);
}
