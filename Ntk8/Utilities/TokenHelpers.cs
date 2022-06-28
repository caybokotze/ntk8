using System;
using System.Security.Cryptography;

namespace Ntk8.Utilities;

public static class TokenHelpers
{
    public static string GenerateToken()
    {
        var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        var randomBytes = new byte[40];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }
}