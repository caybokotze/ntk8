using Dapper.CQRS;
using Ntk8.Data.Commands;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using ScopeFunction.Utils;

namespace Ntk8.DatabaseServices;

public interface IAccountCommands
{
    void DeleteRolesForUserById(int id);
    void DeleteUserById(int id);
    RefreshToken InsertRefreshToken(RefreshToken? refreshToken);
    Role InsertOrUpdateRole(Role? role);
    UserAccountResponse InsertUser(IUserEntity? userEntity);
    UserRole InsertOrUpdateUserRole(UserRole? userRole);
    RefreshToken UpdateRefreshToken(RefreshToken? refreshToken);
    UserAccountResponse UpdateUser(IUserEntity? userEntity);
}

public class AccountCommands : IAccountCommands
{
    private readonly ICommandExecutor _commandExecutor;

    public AccountCommands(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }
    
    public void DeleteRolesForUserById(int id)
    {
        _commandExecutor.Execute(new DeleteRolesForUserById(id));
    }

    public void DeleteUserById(int id)
    {
        _commandExecutor.Execute(new DeleteUserById(id));
    }

    public RefreshToken InsertRefreshToken(RefreshToken? refreshToken)
    {
        if (refreshToken is null)
        {
            throw new InvalidRefreshTokenException("Can not insert a null refresh token");
        }
        
        refreshToken.Id = _commandExecutor.Execute(new InsertRefreshToken(refreshToken));
        return refreshToken;
    }

    public Role InsertOrUpdateRole(Role? role)
    {
        if (role is null)
        {
            throw new InvalidRoleException("Can not insert a null role");
        }
        
        role.Id = _commandExecutor.Execute(new InsertOrUpdateRole(role));
        return role;
    }

    public UserAccountResponse InsertUser(IUserEntity? userEntity)
    {
        if (userEntity is null)
        {
            throw new InvalidUserException("Cannot insert a null user");
        }
        
        userEntity.Id = _commandExecutor.Execute(new InsertUser(userEntity));
        return userEntity.MapTo(new UserAccountResponse());
    }

    public UserRole InsertOrUpdateUserRole(UserRole? userRole)
    {
        if (userRole is null)
        {
            throw new InvalidRoleException("Cannot insert a null user role");
        }
        
        userRole.Id = _commandExecutor.Execute(new InsertOrUpdateUserRole(userRole));
        return userRole;
    }

    public RefreshToken UpdateRefreshToken(RefreshToken? refreshToken)
    {
        if (refreshToken is null)
        {
            throw new InvalidRefreshTokenException("Can not update a null refresh token");
        }
        
        refreshToken.Id = _commandExecutor.Execute(new UpdateRefreshToken(refreshToken));
        return refreshToken;
    }

    public UserAccountResponse UpdateUser(IUserEntity? userEntity)
    {
        if (userEntity is null)
        {
            throw new InvalidUserException("A null user can not be updated");
        }
        
        userEntity.Id = _commandExecutor.Execute(new UpdateUser(userEntity));
        return userEntity.MapTo(new UserAccountResponse(userEntity.IsVerified));
    }
}