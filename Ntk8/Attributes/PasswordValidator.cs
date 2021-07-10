using System;

namespace Ntk8.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PasswordValidator : Attribute
    {
        public PasswordValidator()
        {
            
        }

        public bool MeetsStrengthRequirement()
        {
            return false;
        }
    }
}