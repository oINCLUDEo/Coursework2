using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MigrationService.Binders
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            value = value.Replace(',', '.');

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(parsedValue);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Недопустимый формат числа.");
            }

            return Task.CompletedTask;
        }
    }
}

