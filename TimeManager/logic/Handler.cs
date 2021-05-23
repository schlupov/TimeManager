using System;
using System.IO;
using System.Threading.Tasks;

namespace TimeManager.logic
{
    public class Handler
    {
        private readonly UserLogic userLogic = new();

        public async Task HandleCreateAsync(string name, string email, string password)
        {
            await userLogic.CreateUserAsync(name, email, password);
        }

        public async Task HandleReadAsync(string settings, string token, bool settingsConfigEmail,
            bool tokenConfigEmail)
        {
            string email = null;
            string password = null;
            if (settingsConfigEmail || tokenConfigEmail)
            {
                email = Program.Configuration.Email;
                password = Program.Configuration.Password;
            }

            if (!string.IsNullOrEmpty(settings) || !string.IsNullOrEmpty(token))
            {
                email = string.IsNullOrEmpty(settings) ? token : settings;
            }

            bool successToken = await userLogic.CheckTokenAsync(email);
            if (!successToken)
            {
                if (password == null)
                {
                    await Console.Out.WriteAsync(
                        $"Couldn't find token for email {email} in {Program.Configuration.PathToToken}, " +
                        $"please type your password: ");
                    password = Console.ReadLine();
                }

                bool successPassword = await userLogic.CheckPasswordAsync(email, password);
                if (!successPassword)
                {
                    await Console.Error.WriteLineAsync("Bad token and password");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(settings) || settingsConfigEmail)
            {
                await userLogic.ReadUserSettingsAsync(email);
            }

            if (!string.IsNullOrEmpty(token) || tokenConfigEmail)
            {
                await userLogic.ReadUserTokenAsync(email);
            }
        }

        public async Task HandleUpdateAsync(string email, bool settingsConfigEmail, bool token, bool password)
        {
            if (!token && !password)
            {
                await Console.Error.WriteLineAsync("Choose --token or --password");
                return;
            }
            string configEmail = null;
            if (settingsConfigEmail && Program.Configuration.Email != null)
            {
                configEmail = Program.Configuration.Email;
            }
            if (settingsConfigEmail && Program.Configuration.Email == null)
            {
                await Console.Error.WriteLineAsync("You don't have your email set in the config.json file");
                return;
            }

            if (token)
            {
                string newToken;
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(configEmail))
                {
                    var tokenOk = await userLogic.CheckTokenAsync(email);
                    if (!tokenOk)
                    {
                        await Console.Out.WriteAsync(
                            $"Invalid token in the token file, please type your password: ");
                        var passwordFromUser = Console.ReadLine();
                        var passwordOk = await userLogic.CheckPasswordAsync(email, passwordFromUser);
                        if (!passwordOk)
                        {
                            await Console.Error.WriteLineAsync("Bad password!");
                            return;
                        }
                    }
                    newToken = await userLogic.GenerateNewTokenAsync(email);
                    if (newToken == null)
                    {
                        await Console.Error.WriteLineAsync("No such user");
                        return;
                    }
                }
                else
                {
                    await Console.Error.WriteLineAsync("Use valid email address");
                    return;
                }

                await Console.Out.WriteLineAsync($"This is your new token: {newToken}");
                var success = await userLogic.UpdateTokenInFileAsync(newToken, email);
                if (!success)
                {
                    await Console.Error.WriteLineAsync(
                        $"It wasn't possible to update your token in file " +
                        $"{Path.Combine(Program.Configuration.PathToToken, email!)}, please do it manually!");
                }
            }

            if (password)
            {
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(configEmail))
                {
                    string emailToUse = !string.IsNullOrEmpty(email) ? email : configEmail;
                    await userLogic.UpdatePasswordAsync(emailToUse);
                }
                else
                {
                    await Console.Error.WriteLineAsync("Use valid email address");
                }
            }
        }

        public async Task HandleDeleteAsync(string email, bool settingsConfigEmail)
        {
            string configEmail = null;
            if (settingsConfigEmail && Program.Configuration.Email != null)
            {
                configEmail = Program.Configuration.Email;
            }
            if (settingsConfigEmail && Program.Configuration.Email == null)
            {
                await Console.Error.WriteLineAsync("You don't have your email set in the config.json file");
                return;
            }

            if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(configEmail))
            {
                string emailToUse = !string.IsNullOrEmpty(email) ? email : configEmail;
                await userLogic.DeleteUserAsync(emailToUse);
            }
            else
            {
                await Console.Error.WriteLineAsync("Use valid email address");
            }
        }
    }
}