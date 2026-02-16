using System.ComponentModel.DataAnnotations;

namespace Aep.Cmo.Sync.Orchestrator.Infrastructure.Messaging;

public static class ServiceBusMessageValidator
{
    public static bool TryValidate<T>(T obj, out List<string> errors)
    {
        var context = new ValidationContext(obj!);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            obj!,
            context,
            results,
            validateAllProperties: true);

        errors = results
            .Select(r => r.ErrorMessage ?? "Unknown validation error")
            .ToList();

        return isValid;
    }
}
