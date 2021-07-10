namespace Ntk8.Helpers
{
    public static class EmailHelpers
    {
        public static void SendVerificationEmail(
            IEmailService emailService,
            UserModel userModel,
            string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/user/verify-email?token={userModel.VerificationToken}";
                message = $@"<>Please click the link below to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the token below to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{userModel.VerificationToken}</code></p>";
            }
            
            emailService.Send(
                to: userModel.Email,
                subject: "Sign-Up Verification API = Verify Email",
                html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}");
        }
        
        public static void SendEmailAlreadyExistsEmail(
            IEmailService emailService,
            string email,
            string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
            }
            else
            {
                message = "<p>If you don't know your password you can reset it via the <code>/accounts/forget-password</code> api route.</p>";
            }

            emailService.Send(
                to: email,
                subject: "Sign Up Verification API - Email Already Registered",
                html: $@"<h4>Email already registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}");
        }
        
        public static void SendPasswordResetEmail(
            IEmailService emailService,
            UserModel userModel, 
            string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={userModel.ResetToken}";
                message =
                    $@"<p>Please click the link below to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{userModel.ResetToken}</code></p>";
            }
            emailService.Send(
                to: userModel.Email,
                subject: "Sign Up Verification API - Reset Password",
                html: $"<h4>Reset Password Email</h4>" +
                      $"{message}");
            
        }
    }
}