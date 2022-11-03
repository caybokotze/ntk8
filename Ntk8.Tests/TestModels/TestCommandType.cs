using Ntk8.DatabaseServices;
using Ntk8.Models;

namespace Ntk8.Tests.TestModels;

public class TestCommandType : IAccountCommands
{
    public void DeleteRolesForUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public long InsertRefreshToken(RefreshToken refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public int InsertRole(Role role)
    {
        throw new System.NotImplementedException();
    }

    public int InsertUser(IUserEntity userEntity)
    {
        throw new System.NotImplementedException();
    }

    public int InsertUserRole(UserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public void InvalidateRefreshToken(string refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateUser(IUserEntity userEntity)
    {
        throw new System.NotImplementedException();
    }
}