using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class AllowedAttribute : ValidationAttribute
    {
        private object[] allowedValues;

        public AllowedAttribute(params object[] allowedValues)
        {
            this.allowedValues = allowedValues;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (allowedValues.Any(x => x.ToString().ToLower() == value.ToString().ToLower()))
            {
                return ValidationResult.Success;
            }

            string error = $"'{value}' is not allowed, the allowed values are {string.Join(", ", allowedValues)}";

            return new ValidationResult(error);
        }
    }
}
