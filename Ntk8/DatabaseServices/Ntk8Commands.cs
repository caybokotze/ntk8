using Dapper.CQRS;
using Ntk8.Data.Commands;
using Ntk8.Models;

namespace Ntk8.DatabaseServices;

public interface INtk8Commands
{
    void DeleteRolesForUserById(int id);
    void DeleteUserById(int id);
    long InsertRefreshToken(RefreshToken refreshToken);
    int InsertRole(Role role);
    int InsertUser(IBaseUser user);
    int InsertUserRole(UserRole userRole);
    void InvalidateRefreshToken(string refreshToken);
    void UpdateRefreshToken(RefreshToken refreshToken);
    void UpdateUser(IBaseUser user);
}

public class Ntk8Commands : INtk8Commands
{
    private readonly ICommandExecutor _commandExecutor;

    public Ntk8Commands(ICommandExecutor commandExecutor)
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

    public int InsertUser(IBaseUser user)
    {
        return _commandExecutor.Execute(new InsertUser(user));
    }

    public int InsertUserRole(UserRole userRole)
    {
        return _commandExecutor.Execute(new InsertUserRole(userRole));
    }

    public void InvalidateRefreshToken(string refreshToken)
    {
        _commandExecutor.Execute(new InvalidateRefreshToken(refreshToken));
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        _commandExecutor.Execute(new UpdateRefreshToken(refreshToken));
    }

    public void UpdateUser(IBaseUser user)
    {
        _commandExecutor.Execute(new UpdateUser(user));
    }
}