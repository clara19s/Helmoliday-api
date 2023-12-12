using HELMoliday.Helpers;
using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Validations
{
    public class DateValidation : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateValidation(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (value != null && comparisonValue != null && DateConverter.ConvertStringToDate(value as string) < DateConverter.ConvertStringToDate(comparisonValue as string))
            {
                return new ValidationResult(
                    $"{validationContext.DisplayName} doit être ultérieur à {_comparisonProperty}");
            }

            return ValidationResult.Success;
        }
    }
}
