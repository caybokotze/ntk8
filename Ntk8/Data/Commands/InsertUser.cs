using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class InsertUser : Command<int>
    {
        public IUserEntity UserEntity { get; }

        public InsertUser(IUserEntity userEntity)
        {
            UserEntity = userEntity;
            userEntity.Guid = Guid.NewGuid();
            UserEntity.Email = userEntity.Email?.ToLowerInvariant();
            UserEntity.DateModified = DateTime.UtcNow;
            UserEntity.DateCreated = DateTime.UtcNow;
        }

        public override void Execute()
        {
            const string sql = @"INSERT INTO users (
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
                    @IsActive); 
            SELECT last_insert_id();";

            Result = QueryFirst<int>(sql,
                UserEntity);
        }
    }
}