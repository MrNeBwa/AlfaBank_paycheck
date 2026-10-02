namespace AlfaBank.Core.Models.Enums;

/// <summary>
/// Состояние выданного кредита.
/// </summary>
public enum CreditStatus
{
    /// <summary>
    /// Кредит выдан и обслуживается: платежи по графику ещё не завершены.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Кредит полностью погашен.
    /// </summary>
    Closed = 2
}
