using Microsoft.AspNetCore.Authorization;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Service;

namespace tobeh.Louvre.Server.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public UserTypeEnum Type { get; }
    public RoleRequirement(UserTypeEnum type) => Type = type;
}

public class RoleRequirementHandler(UserRequestContext userContext) : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var user = await userContext.GetUserAsync();
        if (user.UserType > requirement.Type)
        {
            context.Fail(new AuthorizationFailureReason(this, $"User type '{user.UserType}' does not meet the required type '{requirement.Type}'."));
        }

        else context.Succeed(requirement);
    }
}