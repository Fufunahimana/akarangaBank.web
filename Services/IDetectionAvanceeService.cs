using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IDetectionAvanceeService
{
    Task<ResultatSurveillance> DetecterStructuringAsync(int clientId, DateTime date);
    Task<ResultatSurveillance> DetecterSmurfingAsync(int clientId, DateTime date);
    Task<ResultatSurveillance> DetecterLayeringAsync(int clientId, DateTime date);
    Task<ResultatSurveillance> DetecterVelociteAnormaleAsync(int clientId, DateTime date);
}
