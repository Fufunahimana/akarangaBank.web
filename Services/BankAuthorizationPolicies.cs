using Microsoft.AspNetCore.Authorization;

namespace AkarangaBank.Web.Services;

public static class BankAuthorizationPolicies
{
    public const string GestionnaireOrAdmin = "GestionnaireOrAdmin";
    public const string OperationsWrite = "OperationsWrite";

    public static void AddGestionnaireOrAdminPolicy(this AuthorizationOptions options) =>
        options.AddPolicy(GestionnaireOrAdmin, policy =>
            policy.RequireRole("gestionnaire", "admin"));

    public static void AddOperationsWritePolicy(this AuthorizationOptions options) =>
        options.AddPolicy(OperationsWrite, policy =>
            policy.RequireRole("admin", "gestionnaire", "guichetier"));
}
