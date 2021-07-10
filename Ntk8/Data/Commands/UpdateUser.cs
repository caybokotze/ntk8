using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class UpdateUser : Command<int>
    {
        private User User { get; }

        public UpdateUser(User userModel)
        {
            User = userModel;
        }

        public override void Execute()
        {
            Result = Execute(@"UPDATE users SET 
            name = @Name,
            surname = @Surname,
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
            reset_token_expires = @ResetTokenExpires
            WHERE id = @Id;", User);
        }
    }
}