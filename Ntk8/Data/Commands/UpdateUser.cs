using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class UpdateUser : Command<int>
    {
        private BaseBaseUser BaseBaseUser { get; }

        public UpdateUser(BaseBaseUser baseBaseUserModel)
        {
            BaseBaseUser = baseBaseUserModel;
            BaseBaseUser.DateModified = DateTime.UtcNow;
        }

        public override void Execute()
        {
            Result = Execute(@"UPDATE users SET 
            first_name = @FirstName,
            last_name = @LastName,
            reference_id = @ReferenceId,
            title = @Title,
            email = @Email,
            tel_number = @TelNumber,
            username = @Username,
            access_failed_count = @AccessFailedCount,
            lockout_enabled = @LockoutEnabled,
            password_hash = @PasswordHash,
            password_salt = @PasswordSalt,
            accepted_terms = @AcceptedTerms,
            reset_token = @ResetToken,
            verification_token = @VerificationToken,
            verification_date = @VerificationDate,
            password_reset_date = @PasswordResetDate,
            reset_token_expires = @ResetTokenExpires,
            date_modified = @DateModified,
            date_created = @DateCreated,
            is_active = @IsActive
            WHERE id = @Id;", BaseBaseUser);
        }
    }
}