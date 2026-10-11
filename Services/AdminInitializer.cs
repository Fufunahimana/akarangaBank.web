using Microsoft.AspNetCore.Identity; 
using AkarangaBank.Web.Models; 
using AkarangaBank.Web.Data; 
using Microsoft.EntityFrameworkCore; 
 
namespace AkarangaBank.Web.Services; 
 
public static class AdminInitializer 
{ 
    public static async Task InitializeAsync(IServiceProvider services) 
 { 
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); 
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>(); 
        var context = services.GetRequiredService<AkarangaBankContext>(); 
 
        // Créer les rôles 
        string[] roles = { "admin", "gestionnaire", "auditeur", "guichetier","client" }; 
        foreach (var role in roles) 
        { 
            if (!await roleManager.RoleExistsAsync(role)) 
                await roleManager.CreateAsync(new ApplicationRole { Name =role }); 
        } 

        // Ensure the account types used by the account-opening form exist.
        var accountTypes = await context.TypesCompte.AsNoTracking()
            .Select(type => new { type.Code, type.Libelle })
            .ToListAsync();
        var requiredTypes = new[]
        {
            (Code: "EPARGNE", Libelle: "Épargne"),
            (Code: "COURANT", Libelle: "Courant")
        };
        var missingTypes = requiredTypes
            .Where(required => !accountTypes.Any(existing =>
                string.Equals(existing.Code, required.Code, StringComparison.OrdinalIgnoreCase)
                || string.Equals(existing.Libelle, required.Libelle, StringComparison.OrdinalIgnoreCase)))
            .Select(required => new TypeCompte
            {
                Code = required.Code,
                Libelle = required.Libelle,
                TauxInteret = 0m,
                SoldeMinimum = 0m,
                AutoriseDecouvert = false,
                DateCreation = DateTime.UtcNow
            })
            .ToList();

        if (missingTypes.Count > 0)
        {
            context.TypesCompte.AddRange(missingTypes);
            await context.SaveChangesAsync();
        }

        // Créer le premier admin 
        const string adminEmail = "mettez_votre_email_ici"; 
        var admin = await userManager.FindByEmailAsync(adminEmail); 
 
        if (admin is null) 
        { 
            var siege = await context.Agences.FirstOrDefaultAsync(a => a.EstSiege); 
            if (siege is null) return; 
 
            admin = new ApplicationUser 
            { 
                UserName = adminEmail, 
                Email = adminEmail, 
                EmailConfirmed = true, 
                NomComplet = "Administrateur Système", 
                Role = "admin", 
                AgenceId = siege.Id, 
                EstActif = true 
            }; 
 
            var result = await userManager.CreateAsync(admin, "Administrateur@2025!"); 
            if (result.Succeeded) 
                await userManager.AddToRoleAsync(admin, "admin"); 
        } 
    } 
} 
 
