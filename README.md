# ntk8
AuthNTk8 (auth-en-ti-cate) is a standalone .NET auth service for stateless authentication.

# Main Features
**Allows you to auth a user, whether via session or JWT is up to you.**

## Account Service Interface
```csharp
 public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
        void Register(RegisterRequest model, string origin);
        void VerifyEmail(string token);
        void ForgotPassword(ForgotPasswordRequest model, string origin);
        void ValidateResetToken(ValidateResetTokenRequest model);
        void ResetPassword(ResetPasswordRequest model);
        AccountResponse GetById(int id);
        AccountResponse Create(CreateRequest model);
        AccountResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        void AutoVerifyUser(RegisterRequest model);
    }
```


## Migration Scripts to create valid tables
**These migrations are for mysql, some minor tweaking would be required for other databases.**
> As long as the properties match you will be able to use the package.
```sql

CREATE TABLE users
(
    id                  int PRIMARY KEY AUTO_INCREMENT,
    reference_id        char(36),
    title               varchar(100),
    email               varchar(60),
    name                varchar(100),
    surname             varchar(100),
    tel_number          varchar(15),
    username            varchar(20),
    access_failed_count int,
    lockout_enabled     tinyint(1),
    password_hash       varchar(50),
    concurrency_stamp   varchar(50),
    security_stamp      varchar(50),
    password_salt       varchar(50),
    accepted_terms        tinyint(1),
    reset_token         varchar(100),
    verification_token  varchar(100),
    verification_date   datetime,
    password_reset      datetime,
    reset_token_expires datetime,
    date_created        datetime,
    date_updated        datetime,
    is_live             tinyint(1),
    address_id          int,
    business_branch_id  int
);
```


```sql
CREATE TABLE roles
(
    id int PRIMARY KEY AUTO_INCREMENT,
    role_name varchar(50)
);
```

```sql
CREATE TABLE user_roles
(
    id      int PRIMARY KEY AUTO_INCREMENT,
    user_id int,
    role_id int
);
```

```sql
ALTER TABLE user_roles
    ADD CONSTRAINT user_roles_user_id
        FOREIGN KEY (user_id)
            REFERENCES users (id);

ALTER TABLE user_roles
    ADD CONSTRAINT user_roles_role_id
        FOREIGN KEY (role_id)
            REFERENCES roles (id);
```


## Grab on nuget
