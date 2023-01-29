/*using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dal;
using Microsoft.EntityFrameworkCore;
using Queries;
using Queries.Base;

namespace AuthorizationService
{
    /// <summary>
    /// .
    /// </summary>
    public class AuthenticationQuery : AsyncQueryBase<AuthorizationModel, AuthenticationQueryResult>
    {
        private readonly Regex usernameNormalizer = new Regex(
            pattern: @"[\wа-яА-ЯёЁ]",
            options: RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
        /// </summary>
        /// <param name="contextFactory">Context factory.</param>
        public AuthenticationQuery(IDbContextFactory<Context> contextFactory, HashMachine hashMachine)
            : base(contextFactory)
        {
            HashMachine = hashMachine;
        }

        private HashMachine HashMachine { get; }

        /// <inheritdoc/>
        protected override async Task<QueryResult<AuthenticationQueryResult>> ExecuteCoreAsync(AuthorizationModel model)
        {
            string username = GetNormalizedUsername(model.Username);
            if (username == string.Empty)
            {
                return GetFailedResult("Неверное имя пользователя");
            }

            var identifiedUsers = Context.Users.Where(user => user.Login == username);
            if (identifiedUsers.Count() != 1)
            {
                return GetFailedResult("Неверное имя пользователя");
            }

            var identifiedUser = await identifiedUsers.FirstOrDefaultAsync();
            string passwordSalt = identifiedUser.PasswordSalt;
            string passwordHash = HashMachine.GeneratePasswordHash(model.Password, passwordSalt);
            var authenticatedUsers = identifiedUsers.Where(user => user.PasswordHash == passwordHash);
            if (!authenticatedUsers.Any())
            {
                return GetFailedResult("Неверный пароль пользователя");
            }

            var authenticatedUser = await authenticatedUsers.FirstOrDefaultAsync();

            return GetSuccessfulResult(new AuthenticationQueryResult("Аутентификация успешна"));
        }

        private string GetNormalizedUsername(string username)
        {
            /*var match = usernameNormalizer.Match($"{username}@");
            if (!match.Success && match.Groups.Count != 2)
            {
                return string.Empty;
            }

            string normalizedUsername = match.Groups[1].Value;#1#

            var match = usernameNormalizer.Match(username);
            if (!match.Success)
            {
                return string.Empty;
            }

            string normalizedUsername = username.Trim();

            return normalizedUsername;
        }
    }
}*/