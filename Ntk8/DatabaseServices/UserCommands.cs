using Dapper.CQRS;
using Ntk8.Data.Commands;
using Ntk8.Models;

namespace Ntk8.DatabaseServices;

public interface IUserCommands
{
    void DeleteRolesForUserById(int id);
    void DeleteUserById(int id);
    long InsertRefreshToken(RefreshToken refreshToken);
    int InsertRole(Role role);
    int InsertUser(IUserEntity userEntity);
    int InsertUserRole(UserRole userRole);
    void UpdateRefreshToken(RefreshToken refreshToken);
    void UpdateUser(IUserEntity userEntity);
}

public class UserCommands : IUserCommands
{
    private readonly ICommandExecutor _commandExecutor;

    public UserCommands(ICommandExecutor commandExecutor)
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
        return _commandExecutor.Execute(new InsertRole(role));
    }

    public int InsertUser(IUserEntity userEntity)
    {
        return _commandExecutor.Execute(new InsertUser(userEntity));
    }

    public int InsertUserRole(UserRole userRole)
    {
        return _commandExecutor.Execute(new InsertUserRole(userRole));
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        _commandExecutor.Execute(new UpdateRefreshToken(refreshToken));
    }

    public void UpdateUser(IUserEntity userEntity)
    {
        _commandExecutor.Execute(new UpdateUser(userEntity));
    }
}