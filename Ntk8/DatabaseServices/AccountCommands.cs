using Dapper.CQRS;
using Ntk8.Data.Commands;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.DatabaseServices;

public interface IAccountCommands
{
    void DeleteRolesForUserById(int id);
    void DeleteUserById(int id);
    long InsertRefreshToken(RefreshToken? refreshToken);
    int InsertRole(Role? role);
    int InsertUser(IUserEntity? userEntity);
    int InsertUserRole(UserRole? userRole);
    void UpdateRefreshToken(RefreshToken? refreshToken);
    void UpdateUser(IUserEntity? userEntity);
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

    public long InsertRefreshToken(RefreshToken refreshToken)
    {
        return _commandExecutor.Execute(new InsertRefreshToken(refreshToken));
    }

    public int InsertRole(Role role)
    {
        if (role is null)
        {
            throw new InvalidRoleException();
        }
        
        return _commandExecutor.Execute(new InsertRole(role));
    }

    public int InsertUser(IUserEntity userEntity)
    {
        if (userEntity is null)
        {
            throw new InvalidUserException("Cannot insert a null user");
        }
        
        return _commandExecutor.Execute(new InsertUser(userEntity));
    }

    public int InsertUserRole(UserRole userRole)
    {
        if (userRole is null)
        {
            throw new InvalidRoleException();
        }
        
        return _commandExecutor.Execute(new InsertUserRole(userRole));
    }

    public void UpdateRefreshToken(RefreshToken? refreshToken)
    {
        if (refreshToken is null)
        {
            throw new InvalidRefreshTokenException("Can not update a null refresh token");
        }
        
        _commandExecutor.Execute(new UpdateRefreshToken(refreshToken));
    }

    public void UpdateUser(IUserEntity? userEntity)
    {
        if (userEntity is null)
        {
            throw new InvalidUserException("A null user can not be updated");
        }
        
        _commandExecutor.Execute(new UpdateUser(userEntity));
    }
}