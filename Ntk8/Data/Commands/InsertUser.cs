using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUser : Command<int>
    {
        public BaseBaseUser BaseBaseUser { get; }

        public InsertUser(BaseBaseUser baseBaseUser)
        {
            BaseBaseUser = baseBaseUser;
            BaseBaseUser.DateModified = DateTime.UtcNow;
            BaseBaseUser.DateCreated = DateTime.UtcNow;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<int>(@"INSERT INTO users (
            first_name, 
            last_name, 
            reference_id, 
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
            verification_date, 
            password_reset_date, 
            reset_token_expires,
                   date_modified,
                   date_created,
                   is_active) 
            VALUES (@FirstName,
                    @LastName,
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
                    @DateCreated,
                    @IsActive); SELECT last_insert_id();", BaseBaseUser);
        }
    }
}