using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUser : Command<int>
    {
        public User User { get; }

        public InsertUser(User user)
        {
            User = user;
        }
        
        public override void Execute()
        {
            Result = Execute(@"INSERT INTO users (
            name, 
            surname, 
            reference_id, 
            title, 
            email, 
            tel_number, 
            user_name, 
            access_failed_count, 
            lockout_enabled, 
            password_hash, 
            password_salt, 
            accepted_terms, 
            reset_token, 
            verification_token, 
            verification_date, 
            password_reset_date, 
            reset_token_expires,
                   date_modified,
                   date_created) 
            VALUES (@Name,
                    @Surname,
                    @ReferenceId,
                    @Title,
                    @Email,
                    @TelNumber,
                    @UserName,
                    @AccessFailedCount,
                    @LockoutEnabled,
                    @PasswordHash,
                    @PasswordSalt,
                    @AcceptedTerms,
                    @ResetToken,
                    @VerificationToken,
                    @VerificationDate,
                    @PasswordResetDate,
                    @ResetTokenExpires,
                    @DateModified,
                    @DateCreated)", User);
        }
    }
}