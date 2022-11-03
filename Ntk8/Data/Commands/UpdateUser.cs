using System;
using System.IO;
using Dapper.CQRS;
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
                throw new InvalidDataException("Can not update a record with a non valid ID, as that is used as the primary key.");
            }
            
            UserEntity = userEntityModel;
            UserEntity.Email = userEntityModel.Email?.ToLowerInvariant();
            UserEntity.DateModified = DateTime.UtcNow;
        }

        public override void Execute()
        {
            Result = Execute(
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
            WHERE id = @Id;",
                UserEntity);
        }
    }
}