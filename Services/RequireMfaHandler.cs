using Microsoft.AspNetCore.Authorization; 
 
namespace AkarangaBank.Web.Services; 
 
public class RequireMfa : IAuthorizationRequirement 
{ 
} 
 
public class RequireMfaHandler : AuthorizationHandler<RequireMfa> 
{ 
    protected override Task HandleRequirementAsync( 
        AuthorizationHandlerContext context, 
        RequireMfa requirement) 
    { 
        if (context.User.Identity?.IsAuthenticated != true) 
            return Task.CompletedTask; 
 
        var amrClaim = context.User.FindFirst(c => c.Type == "amr"); 
        if (amrClaim?.Value == "mfa") 
        { 
            context.Succeed(requirement); 
        } 
 
        return Task.CompletedTask; 
    } 
} 
