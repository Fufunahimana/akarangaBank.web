using AkarangaBank.Web.Data; 
using AkarangaBank.Web.Models; 
using AkarangaBank.Web.Services; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc.Authorization; 
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args); 
 
// --- Base de données (MariaDB / MySqlConnector) --- 
var connectionString = builder.Configuration.GetConnectionString("AkarangaBank");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("La chaîne de connexion 'AkarangaBank' est absente. Configurez ConnectionStrings__AkarangaBank dans Railway.");

builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddDbContext<AkarangaBankContext>(options => 
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); 

// --- Identity (Unique et complet avec rôles et options de sécurité) --- 
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => 
{ 
    // Politique de mot de passe bancaire 
    options.Password.RequiredLength = 8; 
    options.Password.RequireDigit = true; 
    options.Password.RequireUppercase = true; 
    options.Password.RequireLowercase = true; 
    options.Password.RequireNonAlphanumeric = true; 
 
    // Verrouillage après tentatives échouées 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); 
    options.Lockout.MaxFailedAccessAttempts = 3; 
    options.Lockout.AllowedForNewUsers = true; 
 
    // Confirmation email (désactivée pour le cours) 
    options.SignIn.RequireConfirmedEmail = false; 
    options.SignIn.RequireConfirmedAccount = false; 
 
    // Token TOTP pour la 2FA 
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider; 
    options.Tokens.AuthenticatorIssuer = "AkarangaBank"; 
}) 
.AddEntityFrameworkStores<AkarangaBankContext>() 
.AddDefaultTokenProviders(); 
 
// --- Cookie d'authentification (sessions) --- 
builder.Services.ConfigureApplicationCookie(options => 
{ 
    options.LoginPath = "/Identity/Account/Login"; 
    options.LogoutPath = "/Identity/Account/Logout"; 
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; 
 
    // DUREE_SESSION_MIN = 30 minutes (paramètre du script) 
    // On prend une marge : 60 minutes avec expiration glissante. 
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
    options.SlidingExpiration = true; 
 
    options.Cookie.HttpOnly = true; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; 
    options.Cookie.SameSite = SameSiteMode.Lax; 
}); 
 
// --- Services métier --- 
builder.Services.AddScoped<IAgenceService, AgenceService>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>(); 
builder.Services.AddScoped<ICompteService, CompteService>(); 
builder.Services.AddScoped<IClientService, ClientService>(); 
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IConformiteService, ConformiteService>();
builder.Services.AddScoped<ISurveillanceService, SurveillanceService>();
builder.Services.AddScoped<IDeclarationService, DeclarationService>();
builder.Services.AddScoped<IComptabiliteService, ComptabiliteService>();
builder.Services.AddScoped<ICompensationService, CompensationService>();
builder.Services.AddScoped<IVirementInterAgenceService, VirementInterAgenceService>();
builder.Services.AddScoped<ICNRFService, CNRFService>();
builder.Services.AddScoped<ISeuilAMLService, SeuilAMLService>();
builder.Services.AddScoped<IDetectionAvanceeService, DetectionAvanceeService>();
builder.Services.AddScoped<IPretService, PretService>();
builder.Services.AddScoped<ICarteService, CarteService>();
builder.Services.AddHostedService<RapportAmlQuotidienService>();
//builder.Services.AddScoped<IVirementService, VirementService>(); 
//builder.Services.AddScoped<IOperationService, OperationService>(); 
//builder.Services.AddScoped<IAmlService, AmlService>(); 
//builder.Services.AddSingleton<IAuditService, AuditService>(); 
 
// --- Politiques d'autorisation --- 
builder.Services.AddSingleton<IAuthorizationHandler, RequireMfaHandler>(); 
builder.Services.AddAuthorization(options => 
{ 
    options.AddPolicy("RequireMfa", policy => 
        policy.Requirements.Add(new RequireMfa())); 
 
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("admin")); 
 
    options.AddGestionnaireOrAdminPolicy();

    options.AddOperationsWritePolicy();

    options.AddPolicy("GuichetierOnly", policy =>
        policy.RequireRole("guichetier", "gestionnaire", "admin"));

    options.AddPolicy("AuditeurOnly", policy =>
        policy.RequireRole("auditeur", "admin"));
}); 
 
// --- Protection globale : tout exige l'authentification --- 
builder.Services.AddControllersWithViews(options => 
{ 
    var policy = new AuthorizationPolicyBuilder() 
        .RequireAuthenticatedUser() 
        .Build(); 
    options.Filters.Add(new AuthorizeFilter(policy)); 
}); 
 
builder.Services.AddRazorPages(options => 
{ 
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login"); 
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout"); 
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied"); 
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/LoginWith2fa"); 
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/LoginWithRecoveryCode");
    // Ajoutez cette ligne pour autoriser l'accès à l'inscription sans être connecté : 
    options.Conventions.AllowAnonymousToAreaPage(areaName: "Identity", pageName: "/Account/Register");
}); 
 
var app = builder.Build(); 
 
// --- Initialisation de la base (migrations, rôles + admin) --- 
using (var scope = app.Services.CreateScope()) 
{ 
    var dbContext = scope.ServiceProvider.GetRequiredService<AkarangaBankContext>();
    await dbContext.Database.MigrateAsync();
    await AdminInitializer.InitializeAsync(scope.ServiceProvider); 
} 
 
// --- Pipeline HTTP --- 
if (!app.Environment.IsDevelopment()) 
{ 
    app.UseExceptionHandler("/Home/Error"); 
    app.UseHsts(); 
} 
// Redirection automatique de la racine vers la page de connexion Identity
app.MapGet("/", async context =>
{
    context.Response.Redirect(context.User.Identity?.IsAuthenticated == true
        ? (context.User.IsInRole("admin") ? "/Home/Index" : "/Clients/Index")
        : "/Identity/Account/Login");
    await Task.CompletedTask;
});
// Créez un scope pour accéder aux services de l'application
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    var user = await userManager.FindByEmailAsync("fufunahimana@gmail.com");
    if (user != null && !await userManager.IsInRoleAsync(user, "admin"))
    {
        await userManager.AddToRoleAsync(user, "admin");
    }
}
app.UseHttpsRedirection(); 
app.UseStaticFiles(); 
app.UseRouting(); 
 
app.UseAuthentication();   // AVANT UseAuthorization 
app.UseAuthorization(); 
 
app.MapControllerRoute( 
    name: "default", 
    pattern: "{controller=Home}/{action=Index}/{id?}"); 
 
app.MapRazorPages(); 

app.Run();

public partial class Program { }
