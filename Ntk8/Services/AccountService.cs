using System;
using System.Collections.Generic;
using Dapper.CQRS;
using Dispatch.K8;
using HigLabo.Core;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Model;
using Ntk8.Models;
using BC = BCrypt.Net.BCrypt;

namespace Ntk8.Services
{
    public class AccountService : IAccountService
    {
        public IQueryExecutor QueryExecutor { get; }
        public ICommandExecutor CommandExecutor { get; }
        public AuthenticationConfiguration Configuration { get; }
        public AuthenticationContextService AuthenticationContextService { get; }

        public AccountService(
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            AuthenticationContextService contextService,
            AuthenticationConfiguration configuration)
        {
            QueryExecutor = queryExecutor;
            CommandExecutor = commandExecutor;
            AuthenticationContextService = contextService;
            Configuration = configuration;
        }
        
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var user = QueryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));

            if (user is null)
            {
                throw new UserNotFoundException("The user does not exist");
            }

            if (!user.IsVerified || 
                !BC.Verify(model.Password, user.PasswordHash))
            {
                throw new InvalidPasswordException("User is not verified or password is incorrect");
            }
            
            var jwtToken = AccountServiceHelpers
                .GenerateJwtToken(Configuration, user);
            var refreshToken = AccountServiceHelpers
                .GenerateRefreshToken(ipAddress);
            user.RefreshTokens = new List<RefreshToken>
            {
                refreshToken
            };
            AccountServiceHelpers
                .RemoveOldRefreshTokens(Configuration, user);
            CommandExecutor.Execute(new UpdateUser(user));

            var response = user.Map(new AuthenticateResponse());

            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var (refreshToken, user) = AccountServiceHelpers
                .GetRefreshToken(QueryExecutor, token);
            var newRefreshToken = AccountServiceHelpers
                .GenerateRefreshToken(ipAddress);
            
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);

            AccountServiceHelpers
                .RemoveOldRefreshTokens(Configuration, user);

            CommandExecutor.Execute(new UpdateUser(user));
            
            var jwtToken = AccountServiceHelpers
                .GenerateJwtToken(Configuration, user);

            var response = user.Map(new AuthenticateResponse());
            
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;
            return response;
        }

        public void RevokeToken(string token, string ipAddress)
        {
            var (refreshToken, user) = AccountServiceHelpers
                .GetRefreshToken(QueryExecutor, token);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            CommandExecutor.Execute(new UpdateUser(user));
        }

        public void Register(RegisterRequest model, string origin)
        {
            var dbUserModel = QueryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));
            
            if (dbUserModel != null)
            {
                if (dbUserModel.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }

                dbUserModel.VerificationToken = AccountServiceHelpers.RandomTokenString();
                dbUserModel.DateModified = DateTime.Now;
                CommandExecutor.Execute(new UpdateUser(dbUserModel));
                return;
            }

            var user = model.Map(new User());
            var role = AssignInitialUserRole();
            user.VerificationToken = AccountServiceHelpers.RandomTokenString();
            user.PasswordHash = BC.HashPassword(model.Password);
            
            CommandExecutor
                .Execute(new InsertUserAndRole(user, role));
        }

        public void AutoVerifyUser(RegisterRequest model)
        {
            var user = QueryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));

            if (user.IsVerified)
            {
                return;
            }
            
            user.VerificationDate = DateTime.UtcNow;
            user.VerificationToken = null;

            CommandExecutor.Execute(new UpdateUser(user));
        }

        private UserRole AssignInitialUserRole()
        {
            var isFirstAccount = !QueryExecutor.Execute(new DoUsersExist());
            var role = isFirstAccount 
                ? UserRoles.GlobalAdmin
                : UserRoles.Unassigned;
            
            return new UserRole
            {
                RoleId = (int) role
            };
        }

        public void VerifyEmail(string token)
        {
            //todo: Make sure that the verification token hasn't' expired...
            
            var user = QueryExecutor
                .Execute(new FetchUserByVerificationToken(token));

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }

            if (user is null)
            {
                throw new NullReferenceException("Verification failed. " +
                                                 "This record either no longer exists or the account has already been verified.");
            }

            user.VerificationDate = DateTime.UtcNow;
            user.VerificationToken = null;
            user.IsLive = true;

            CommandExecutor.Execute(new UpdateUser(user));
        }

        public void ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var user = QueryExecutor.Execute(new FetchUserByEmailAddress(model.Email));

            // always return ok response to prevent email enumeration
            if (user == null) return;

            // create reset token that expires after 1 day
            user.ResetToken = AccountServiceHelpers.RandomTokenString();
            user.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

            CommandExecutor.Execute(new UpdateUser(user));
            
            EmailHelpers.SendPasswordResetEmail(
                EmailService, 
                user,
                origin);
        }

        public void ValidateResetToken(ValidateResetTokenRequest model)
        {
            var user = QueryExecutor.Execute(new FetchUserByResetToken(model.Token));

            // todo: return positive condition
            if (user == null)
            {
                throw new AppException(AuthenticationConstants.InvalidAuthenticationMessage);
            }
        }

        public void ResetPassword(ResetPasswordRequest model)
        {
            var user = QueryExecutor.Execute(new FetchUserByResetToken(model.Token));

            if (user is null)
            {
                throw new AppException(AuthenticationConstants.InvalidAuthenticationMessage);
            }

            // update password and remove reset token
            user.PasswordHash = BC.HashPassword(model.Password);
            user.PasswordResetDate = DateTime.UtcNow;
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            CommandExecutor.Execute(new UpdateUser(user));
        }

        public IEnumerable<AccountResponse> GetAll()
        {
            var users = QueryExecutor.Execute(new DoUsersExist());
            return Mapper.Map<IList<AccountResponse>>(users);
        }

        public AccountResponse GetById(int id)
        {
            var user = QueryExecutor.Execute(new FetchUserById(id));
            return Mapper.Map<AccountResponse>(user);
        }

        public AccountResponse Create(CreateRequest model)
        {
            if (QueryExecutor.Execute(new FetchUserByEmailAddress(model.Email)).Email is not null)
            {
                throw new AppException($"Email: {model.Email} is already registered");
            }
            
            var user = Mapper.Map<UserModel>(model);
            user.DateCreated = DateTime.UtcNow;
            user.VerificationDate = DateTime.UtcNow;

            user.PasswordHash = BC.HashPassword(model.Password);

            CommandExecutor.Execute(new InsertUser(user));

            return Mapper.Map<AccountResponse>(user);
        }

        public AccountResponse Update(int id, UpdateRequest model)
        {
            var user = AccountServiceHelpers.GetAccount(QueryExecutor, id);
            var userByEmail = QueryExecutor.Execute(new FetchUserByEmailAddress(model.Email));

            if (user.Email != model.Email && userByEmail.Email == model.Email)
            {
                throw new AppException($"Email '{model.Email}' is already taken");
            }

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BC.HashPassword(model.Password);
            }

            // copy model to account and save
            Mapper.Map(model, user);
            user.DateUpdated = DateTime.UtcNow;

            CommandExecutor.Execute(new UpdateUser(user));

            return Mapper.Map<AccountResponse>(user);
        }

        public void Delete(int id)
        {
            var user = AccountServiceHelpers.GetAccount(QueryExecutor, id);
            CommandExecutor.Execute(new DeleteUserById(id));
        }
    }
}