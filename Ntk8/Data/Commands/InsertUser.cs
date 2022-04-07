using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUser : Command<int>
    {
        private IBaseUser BaseUser { get; }

        public InsertUser(IBaseUser baseUser)
        {
            BaseUser = baseUser;
            baseUser.Guid ??= Guid.NewGuid();
            BaseUser.Email = baseUser.Email?.ToLowerInvariant() ?? string.Empty;
            BaseUser.DateModified = DateTime.UtcNow;
            BaseUser.DateCreated = DateTime.UtcNow;
        }

        public override void Execute()
        {
            Result = QueryFirst<int>(@"
            INSERT INTO users (
            first_name, 
            last_name, 
            guid, 
            title, 
            email, 
            tel_number, 
            username, 
            access_failed_count, 
            lockout_enabled, 
            password_hash, 
            password_salt, 
            accepted_terms, 
            reset_token, 
            verification_token, 
            date_verified, 
            date_of_password_reset, 
            date_reset_token_expires,
                   date_modified,
                   date_created,
                   is_active) 
            VALUES (@FirstName,
                    @LastName,
                    @Guid,
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
                    @DateVerified,
                    @DateOfPasswordReset,
                    @DateResetTokenExpires,
                    @DateModified,
                    @DateCreated,
                    @IsActive); SELECT last_insert_id();", 
                BaseUser);
        }
    }
}