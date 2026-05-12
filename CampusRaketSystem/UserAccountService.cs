using System.Data;
using MySql.Data.MySqlClient;

namespace CampusRaketSystem;

public sealed record AuthenticatedUser(int AccountID, string Username, string FullName, string Email);

public sealed record AuthenticationResult(bool Succeeded, string Message, AuthenticatedUser? User);

public sealed record SecurityQuestionResult(bool Succeeded, string Message, string? Email, string? Question);

public static class UserAccountService
{
    public static void EnsureUserAccountTable()
    {
        DbHelper.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS user_accounts (
                AccountID INT NOT NULL AUTO_INCREMENT,
                Username VARCHAR(50) NOT NULL,
                FullName VARCHAR(100) NOT NULL,
                Email VARCHAR(120) NOT NULL,
                PasswordHash VARCHAR(100) NOT NULL,
                PasswordSalt VARCHAR(100) NOT NULL,
                SecurityQuestion VARCHAR(200) NULL,
                SecurityAnswerHash VARCHAR(100) NULL,
                SecurityAnswerSalt VARCHAR(100) NULL,
                IsActive TINYINT(1) NOT NULL DEFAULT 1,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                LastLoginAt DATETIME NULL,
                PRIMARY KEY (AccountID),
                UNIQUE KEY UX_UserAccounts_Username (Username),
                UNIQUE KEY UX_UserAccounts_Email (Email)
            );
            """);

        EnsureColumnExists("SecurityQuestion", "VARCHAR(200) NULL");
        EnsureColumnExists("SecurityAnswerHash", "VARCHAR(100) NULL");
        EnsureColumnExists("SecurityAnswerSalt", "VARCHAR(100) NULL");

        int accountCount = Convert.ToInt32(DbHelper.ExecuteScalar("SELECT COUNT(*) FROM user_accounts"));
        if (accountCount == 0)
        {
            (string passwordHash, string passwordSalt) = PasswordHasher.CreateHash("1234");
            (string answerHash, string answerSalt) = CreateSecurityAnswerHash("campusraket");
            DbHelper.ExecuteNonQuery(
                """
                INSERT INTO user_accounts
                    (Username, FullName, Email, PasswordHash, PasswordSalt, SecurityQuestion, SecurityAnswerHash, SecurityAnswerSalt, IsActive)
                VALUES
                    ('admin', 'System Administrator', 'admin@campusraket.local', @passwordHash, @passwordSalt, @securityQuestion, @securityAnswerHash, @securityAnswerSalt, 1)
                """,
                new MySqlParameter("@passwordHash", passwordHash),
                new MySqlParameter("@passwordSalt", passwordSalt),
                new MySqlParameter("@securityQuestion", "What is the default CampusRaket recovery answer?"),
                new MySqlParameter("@securityAnswerHash", answerHash),
                new MySqlParameter("@securityAnswerSalt", answerSalt));
        }

        EnsureDefaultAdminSecurityQuestion();
    }

    public static AuthenticationResult Authenticate(string username, string password)
    {
        EnsureUserAccountTable();

        DataTable table = DbHelper.GetDataTable(
            """
            SELECT AccountID, Username, FullName, Email, PasswordHash, PasswordSalt, IsActive
            FROM user_accounts
            WHERE Username = @username
            LIMIT 1
            """,
            new MySqlParameter("@username", username));

        if (table.Rows.Count == 0)
        {
            return new AuthenticationResult(false, "Invalid username or password.", null);
        }

        DataRow row = table.Rows[0];
        bool isActive = Convert.ToInt32(row["IsActive"]) == 1;

        if (!isActive)
        {
            return new AuthenticationResult(false, "This account is inactive. Contact an administrator.", null);
        }

        bool validPassword = PasswordHasher.Verify(
            password,
            row["PasswordHash"].ToString() ?? "",
            row["PasswordSalt"].ToString() ?? "");

        if (!validPassword)
        {
            return new AuthenticationResult(false, "Invalid username or password.", null);
        }

        int accountId = Convert.ToInt32(row["AccountID"]);
        DbHelper.ExecuteNonQuery(
            "UPDATE user_accounts SET LastLoginAt = NOW() WHERE AccountID = @accountId",
            new MySqlParameter("@accountId", accountId));

        AuthenticatedUser user = new(
            accountId,
            row["Username"].ToString() ?? "",
            row["FullName"].ToString() ?? "",
            row["Email"].ToString() ?? "");

        return new AuthenticationResult(true, "Login successful.", user);
    }

    public static DataTable GetAccounts(string searchText)
    {
        EnsureUserAccountTable();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return DbHelper.GetDataTable(
                """
                SELECT
                    AccountID,
                    Username,
                    FullName,
                    Email,
                    SecurityQuestion,
                    IsActive,
                    CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS AccountStatus,
                    CreatedAt,
                    LastLoginAt
                FROM user_accounts
                ORDER BY AccountID DESC
                """);
        }

        string search = $"%{searchText.Trim()}%";
        return DbHelper.GetDataTable(
            """
            SELECT
                AccountID,
                Username,
                FullName,
                Email,
                SecurityQuestion,
                IsActive,
                CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS AccountStatus,
                CreatedAt,
                LastLoginAt
            FROM user_accounts
            WHERE Username LIKE @search OR FullName LIKE @search OR Email LIKE @search
            ORDER BY AccountID DESC
            """,
            new MySqlParameter("@search", search));
    }

    public static int AddAccount(
        string username,
        string fullName,
        string email,
        string password,
        bool isActive,
        string securityQuestion,
        string securityAnswer)
    {
        EnsureNoDuplicateAccount(username, email, null);
        EnsureSecurityQuestionIsValid(securityQuestion, securityAnswer);

        (string passwordHash, string passwordSalt) = PasswordHasher.CreateHash(password);
        (string answerHash, string answerSalt) = CreateSecurityAnswerHash(securityAnswer);

        DbHelper.ExecuteNonQuery(
            """
            INSERT INTO user_accounts
                (Username, FullName, Email, PasswordHash, PasswordSalt, SecurityQuestion, SecurityAnswerHash, SecurityAnswerSalt, IsActive)
            VALUES
                (@username, @fullName, @email, @passwordHash, @passwordSalt, @securityQuestion, @securityAnswerHash, @securityAnswerSalt, @isActive)
            """,
            new MySqlParameter("@username", username),
            new MySqlParameter("@fullName", fullName),
            new MySqlParameter("@email", email),
            new MySqlParameter("@passwordHash", passwordHash),
            new MySqlParameter("@passwordSalt", passwordSalt),
            new MySqlParameter("@securityQuestion", securityQuestion),
            new MySqlParameter("@securityAnswerHash", answerHash),
            new MySqlParameter("@securityAnswerSalt", answerSalt),
            new MySqlParameter("@isActive", isActive ? 1 : 0));

        return Convert.ToInt32(DbHelper.ExecuteScalar(
            "SELECT AccountID FROM user_accounts WHERE Username = @username",
            new MySqlParameter("@username", username)));
    }

    public static int CreateUserAccount(string username, string fullName, string email, string password, string securityQuestion, string securityAnswer)
    {
        return AddAccount(username, fullName, email, password, isActive: true, securityQuestion, securityAnswer);
    }

    public static void UpdateAccountProfile(
        int accountId,
        string username,
        string fullName,
        string email,
        string? newPassword,
        bool isActive,
        string securityQuestion,
        string? securityAnswer)
    {
        EnsureNoDuplicateAccount(username, email, accountId);
        EnsureSecurityQuestionIsValid(securityQuestion, securityAnswer, allowBlankAnswer: true);

        bool updatePassword = !string.IsNullOrWhiteSpace(newPassword);
        bool updateSecurityAnswer = !string.IsNullOrWhiteSpace(securityAnswer);

        if (!updatePassword && !updateSecurityAnswer)
        {
            DbHelper.ExecuteNonQuery(
                """
                UPDATE user_accounts
                SET Username = @username,
                    FullName = @fullName,
                    Email = @email,
                    SecurityQuestion = @securityQuestion,
                    IsActive = @isActive
                WHERE AccountID = @accountId
                """,
                new MySqlParameter("@username", username),
                new MySqlParameter("@fullName", fullName),
                new MySqlParameter("@email", email),
                new MySqlParameter("@securityQuestion", securityQuestion),
                new MySqlParameter("@isActive", isActive ? 1 : 0),
                new MySqlParameter("@accountId", accountId));
            return;
        }

        List<string> setClauses =
        [
            "Username = @username",
            "FullName = @fullName",
            "Email = @email",
            "SecurityQuestion = @securityQuestion",
            "IsActive = @isActive"
        ];

        List<MySqlParameter> parameters =
        [
            new("@username", username),
            new("@fullName", fullName),
            new("@email", email),
            new("@securityQuestion", securityQuestion),
            new("@isActive", isActive ? 1 : 0),
            new("@accountId", accountId)
        ];

        if (updatePassword)
        {
            (string passwordHash, string passwordSalt) = PasswordHasher.CreateHash(newPassword!);
            setClauses.Add("PasswordHash = @passwordHash");
            setClauses.Add("PasswordSalt = @passwordSalt");
            parameters.Add(new MySqlParameter("@passwordHash", passwordHash));
            parameters.Add(new MySqlParameter("@passwordSalt", passwordSalt));
        }

        if (updateSecurityAnswer)
        {
            (string answerHash, string answerSalt) = CreateSecurityAnswerHash(securityAnswer!);
            setClauses.Add("SecurityAnswerHash = @securityAnswerHash");
            setClauses.Add("SecurityAnswerSalt = @securityAnswerSalt");
            parameters.Add(new MySqlParameter("@securityAnswerHash", answerHash));
            parameters.Add(new MySqlParameter("@securityAnswerSalt", answerSalt));
        }

        string query = $"UPDATE user_accounts SET {string.Join(", ", setClauses)} WHERE AccountID = @accountId";
        DbHelper.ExecuteNonQuery(query, parameters.ToArray());
    }

    public static void SetAccountActive(int accountId, bool isActive)
    {
        EnsureUserAccountTable();

        DbHelper.ExecuteNonQuery(
            "UPDATE user_accounts SET IsActive = @isActive WHERE AccountID = @accountId",
            new MySqlParameter("@isActive", isActive ? 1 : 0),
            new MySqlParameter("@accountId", accountId));
    }

    public static bool TryFindActiveAccountByEmail(string email)
    {
        EnsureUserAccountTable();

        int count = Convert.ToInt32(DbHelper.ExecuteScalar(
            "SELECT COUNT(*) FROM user_accounts WHERE Email = @email AND IsActive = 1",
            new MySqlParameter("@email", email)));

        return count > 0;
    }

    public static SecurityQuestionResult GetSecurityQuestionByEmail(string email)
    {
        EnsureUserAccountTable();

        DataTable table = DbHelper.GetDataTable(
            """
            SELECT Email, SecurityQuestion, SecurityAnswerHash, SecurityAnswerSalt
            FROM user_accounts
            WHERE Email = @email AND IsActive = 1
            LIMIT 1
            """,
            new MySqlParameter("@email", email));

        if (table.Rows.Count == 0)
        {
            return new SecurityQuestionResult(false, "No active account was found for that email.", null, null);
        }

        DataRow row = table.Rows[0];
        string question = row["SecurityQuestion"].ToString() ?? "";
        string answerHash = row["SecurityAnswerHash"].ToString() ?? "";
        string answerSalt = row["SecurityAnswerSalt"].ToString() ?? "";

        if (string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(answerHash) ||
            string.IsNullOrWhiteSpace(answerSalt))
        {
            return new SecurityQuestionResult(false, "This account does not have a security question set.", null, null);
        }

        return new SecurityQuestionResult(true, "Security question loaded.", row["Email"].ToString(), question);
    }

    public static bool VerifySecurityAnswer(string email, string securityAnswer)
    {
        EnsureUserAccountTable();

        DataTable table = DbHelper.GetDataTable(
            """
            SELECT SecurityAnswerHash, SecurityAnswerSalt
            FROM user_accounts
            WHERE Email = @email AND IsActive = 1
            LIMIT 1
            """,
            new MySqlParameter("@email", email));

        if (table.Rows.Count == 0)
        {
            return false;
        }

        DataRow row = table.Rows[0];
        string answerHash = row["SecurityAnswerHash"].ToString() ?? "";
        string answerSalt = row["SecurityAnswerSalt"].ToString() ?? "";

        if (string.IsNullOrWhiteSpace(answerHash) || string.IsNullOrWhiteSpace(answerSalt))
        {
            return false;
        }

        return PasswordHasher.Verify(NormalizeSecurityAnswer(securityAnswer), answerHash, answerSalt);
    }

    public static void ResetPasswordByEmail(string email, string newPassword)
    {
        EnsureUserAccountTable();

        (string hash, string salt) = PasswordHasher.CreateHash(newPassword);
        DbHelper.ExecuteNonQuery(
            """
            UPDATE user_accounts
            SET PasswordHash = @passwordHash,
                PasswordSalt = @passwordSalt
            WHERE Email = @email AND IsActive = 1
            """,
            new MySqlParameter("@passwordHash", hash),
            new MySqlParameter("@passwordSalt", salt),
            new MySqlParameter("@email", email));
    }

    private static void EnsureNoDuplicateAccount(string username, string email, int? excludedAccountId)
    {
        EnsureUserAccountTable();

        string query = excludedAccountId.HasValue
            ? """
              SELECT COUNT(*)
              FROM user_accounts
              WHERE (Username = @username OR Email = @email) AND AccountID <> @accountId
              """
            : """
              SELECT COUNT(*)
              FROM user_accounts
              WHERE Username = @username OR Email = @email
              """;

        List<MySqlParameter> parameters =
        [
            new("@username", username),
            new("@email", email)
        ];

        if (excludedAccountId.HasValue)
        {
            parameters.Add(new MySqlParameter("@accountId", excludedAccountId.Value));
        }

        int duplicateCount = Convert.ToInt32(DbHelper.ExecuteScalar(query, parameters.ToArray()));
        if (duplicateCount > 0)
        {
            throw new InvalidOperationException("Username or email already exists.");
        }
    }

    private static void EnsureColumnExists(string columnName, string columnDefinition)
    {
        int columnCount = Convert.ToInt32(DbHelper.ExecuteScalar(
            """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'user_accounts'
              AND COLUMN_NAME = @columnName
            """,
            new MySqlParameter("@columnName", columnName)));

        if (columnCount == 0)
        {
            DbHelper.ExecuteNonQuery($"ALTER TABLE user_accounts ADD COLUMN {columnName} {columnDefinition}");
        }
    }

    private static void EnsureDefaultAdminSecurityQuestion()
    {
        int missingAdminQuestionCount = Convert.ToInt32(DbHelper.ExecuteScalar(
            """
            SELECT COUNT(*)
            FROM user_accounts
            WHERE Username = 'admin'
              AND (SecurityQuestion IS NULL OR SecurityQuestion = ''
                   OR SecurityAnswerHash IS NULL OR SecurityAnswerHash = ''
                   OR SecurityAnswerSalt IS NULL OR SecurityAnswerSalt = '')
            """));

        if (missingAdminQuestionCount == 0)
        {
            return;
        }

        (string answerHash, string answerSalt) = CreateSecurityAnswerHash("campusraket");
        DbHelper.ExecuteNonQuery(
            """
            UPDATE user_accounts
            SET SecurityQuestion = @securityQuestion,
                SecurityAnswerHash = @securityAnswerHash,
                SecurityAnswerSalt = @securityAnswerSalt
            WHERE Username = 'admin'
            """,
            new MySqlParameter("@securityQuestion", "What is the default CampusRaket recovery answer?"),
            new MySqlParameter("@securityAnswerHash", answerHash),
            new MySqlParameter("@securityAnswerSalt", answerSalt));
    }

    private static void EnsureSecurityQuestionIsValid(string securityQuestion, string? securityAnswer, bool allowBlankAnswer = false)
    {
        if (string.IsNullOrWhiteSpace(securityQuestion))
        {
            throw new InvalidOperationException("Security question is required.");
        }

        if (!allowBlankAnswer && string.IsNullOrWhiteSpace(securityAnswer))
        {
            throw new InvalidOperationException("Security answer is required.");
        }
    }

    private static (string Hash, string Salt) CreateSecurityAnswerHash(string answer)
    {
        return PasswordHasher.CreateHash(NormalizeSecurityAnswer(answer));
    }

    private static string NormalizeSecurityAnswer(string answer)
    {
        return answer.Trim().ToLowerInvariant();
    }
}
