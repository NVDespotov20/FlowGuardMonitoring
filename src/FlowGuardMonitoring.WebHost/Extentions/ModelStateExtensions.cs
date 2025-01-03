using System.Resources;
using FlowGuardMonitoring.WebHost.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlowGuardMonitoring.WebHost.Extentions;

public static class ModelStateExtensions
{
    public static void AssignIdentityErrors(
        this ModelStateDictionary modelState,
        IEnumerable<IdentityError> errors)
    {
        foreach (var error in errors)
        {
            var key = string.Empty;
            if (error.Code.StartsWith("Password"))
            {
                key = "Password";
            }

            modelState.AddModelError(key, AuthLocals.ResourceManager.GetStringOrDefault($"{error.Code}ErrorMsg"));
        }
    }

    public static string? GetFirstGlobalError(this ModelStateDictionary modelState)
    {
        return modelState
            .Where(x => string.IsNullOrEmpty(x.Key))
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .FirstOrDefault();
    }

    private static string GetStringOrDefault(this ResourceManager resourceManager, string resourceKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                return string.Empty;
            }

            return resourceManager.GetString(resourceKey) ?? string.Empty;
        }
        catch (Exception)
        {
            return resourceKey;
        }
    }
}