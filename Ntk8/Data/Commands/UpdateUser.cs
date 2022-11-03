using System;
using Dapper.CQRS;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class UpdateUser : Command<int>
    {
        public IUserEntity UserEntity { get; }

        public UpdateUser(IUserEntity userEntityModel)
        {
            if (userEntityModel.Id < 1)
            {
                throw new InvalidUserException("Ïnvalid ID provided for the user");
            }
            
            UserEntity = userEntityModel;
            UserEntity.Email = userEntityModel.Email?.ToLowerInvariant();
            UserEntity.DateModified = DateTime.UtcNow;
        }

        public override void Execute()
        {
            Result = QueryFirst<int>(
                @"UPDATE users SET 
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
            WHERE id = @Id; SELECT last_insert_id();",
                UserEntity);
        }
    }
}