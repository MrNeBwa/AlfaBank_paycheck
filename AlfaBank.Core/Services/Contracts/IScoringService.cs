using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface IScoringService
{
    ScoringResultDto Evaluate(ScoringInput input, decimal monthlyPayment);
}
