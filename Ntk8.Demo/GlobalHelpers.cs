using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Demo
{
    public static class GlobalHelpers
    {
        public static void ValidateModel(object model)
        {
            var vc = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, vc, results, true);

            if (!isValid)
            {
                throw new ValidationException(results[0].ErrorMessage);
            }
        }
    }
}