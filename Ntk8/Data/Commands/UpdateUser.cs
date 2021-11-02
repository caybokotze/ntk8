using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class UpdateUser : Command<int>
    {
        public BaseUser BaseUser { get; }

        public UpdateUser(BaseUser baseUserModel)
        {
            BaseUser = baseUserModel;
            BaseUser.DateModified = DateTime.UtcNow;
        }

        public override void Execute()
        {
            Result = Execute(@"UPDATE users SET 
            first_name = @FirstName,
            last_name = @LastName,
            guid = @Guid,
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
            date_verified = @DateVerified,
            date_of_password_reset = @DateOfPasswordReset,
            date_reset_token_expires = @DateResetTokenExpires,
            date_modified = @DateModified,
            date_created = @DateCreated,
            is_active = @IsActive
            WHERE id = @Id;", BaseUser);
        }
    }
}