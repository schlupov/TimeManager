using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net.Mail;

namespace TimeManager
{
    public class ArgumentParser
    {
        private readonly Command create;
        private readonly Command read;
        private readonly Command update;
        private readonly Command delete;

        public ArgumentParser(Command create, Command read, Command update, Command delete)
        {
            this.create = create;
            this.read = read;
            this.update = update;
            this.delete = delete;
        }

        public Command CreateOptions()
        {
            var usernameOption = new Option<string>("--name", "User name") {IsRequired = true};
            usernameOption.AddAlias("-n");
            usernameOption.AddValidator(x =>
            {
                var name = x.GetValueOrDefault<string>();
                return name is {Length: >= 3} ? null : "Name has to be at least 3 characters long";
            });

            var emailOption = new Option<string>("--email", "User email") {IsRequired = true};
            emailOption.AddAlias("-e");
            emailOption.AddValidator(x =>
            {
                var email = x.GetValueOrDefault<string>();
                return isValid(email) ? null : "Invalid email address";
            });

            var passwordOption = new Option<string>("--password", "User password") {IsRequired = true};
            passwordOption.AddAlias("-p");
            passwordOption.AddValidator(x =>
            {
                var password = x.GetValueOrDefault<string>();
                return password is {Length: >= 3} ? null : "Password has to be at least 3 characters long";
            });

            create.AddOption(usernameOption);
            create.AddOption(emailOption);
            create.AddOption(passwordOption);
            return create;
        }

        public Command ReadOptions()
        {
            var settingsOption = new Option<string>("--settings", "Read user settings using your email");
            settingsOption.AddAlias("-s");
            var tokenOption = new Option<string>("--token", "Read user token using your email");
            tokenOption.AddAlias("-t");
            var settingsConfigEmail = new Option("--settings-config-email",
                "Read user settings using your email from config.json");
            var tokenConfigEmail =
                new Option("--token-config-email", "Read user token using your email from config.json");

            read.AddOption(settingsOption);
            read.AddOption(tokenOption);
            read.AddOption(settingsConfigEmail);
            read.AddOption(tokenConfigEmail);
            return read;
        }

        public Command UpdateOptions()
        {
            var email = new Option<string>("--email", "Email of the user whose account you want to change");
            email.AddAlias("-e");
            var settingsConfigEmail = new Option("--settings-config-email",
                "User whose account you want to change, email is read from config.json");
            var token = new Option("--token", "Generate new token");
            var password = new Option("--password", "Change password");
            
            update.AddOption(email);
            update.AddOption(settingsConfigEmail);
            update.AddOption(token);
            update.AddOption(password);
            return update;
        }
        
        public Command DeleteOptions()
        {
            var email = new Option<string>("--email", "Email of the user whose account you want to delete");
            email.AddAlias("-e");
            var settingsConfigEmail = new Option("--settings-config-email",
                "User whose account you want to delete, email is read from config.json");

            delete.AddOption(email);
            delete.AddOption(settingsConfigEmail);
            return delete;
        }

        private static bool isValid(string address)
        {
            try
            {
                MailAddress email = new MailAddress(address);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}