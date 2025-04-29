using Relativity.API;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LTAS_User_Management.Models;
using System.Web;
using System.Linq;

namespace LTAS_User_Management.Handlers
{
    public class MessageHandler
    {
        public class SMTPSetting
        {
            public string Section { get; set; }
            public string Name { get; set; }
        }

        public static class EmailBody
        {
            public static StringBuilder DisabledUsersEmailBody(List<Users> users, string message)
            {
                var htmlBody = new StringBuilder();

                // Start HTML
                htmlBody.AppendLine(@"<!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body { 
                                font-family: Arial, sans-serif; 
                                font-size: 13px; 
                                margin: 20px; 
                            }
                            .header-text { 
                                margin-bottom: 15px; 
                            }
                            .date-info {
                                color: #666;
                                margin-bottom: 15px;
                            }
                            table { 
                                width: auto; 
                                min-width: 600px;
                                max-width: 800px;
                                border-collapse: collapse; 
                                margin: 10px 0; 
                                font-size: inherit;
                            }
                            th { 
                                background-color: transparent;
                                padding: 4px 8px;
                                text-align: left;
                                border-bottom: 1px solid #000;
                                font-weight: normal;
                            }
                            td { 
                                padding: 4px 8px;
                                border-bottom: 1px solid #ddd;
                            }
                            .total-row {
                                margin-top: 15px;
                                font-weight: bold;
                            }
                        </style>
                    </head>
                    <body>");

                // Add header text
                htmlBody.AppendLine($@"<div class='header-text'>
                    <p>{message}</p>
                </div>");

                // Add date information
                htmlBody.AppendLine($@"<div class='date-info'>Users disabled on {DateTime.Now:MMMM dd, yyyy}</div>");

                // Create users table
                htmlBody.AppendLine(@"<table>
                <thead>
                    <tr>
                        <th style='min-width: 100px;'>Artifact ID</th>
                        <th style='min-width: 150px;'>First Name</th>
                        <th style='min-width: 150px;'>Last Name</th>
                        <th style='min-width: 200px;'>Email Address</th>
                    </tr>
                </thead>
                <tbody>");

                // Add user data
                foreach (var user in users)
                {
                    htmlBody.AppendLine($@"<tr>
                        <td>{user.ArtifactId}</td>
                        <td>{HttpUtility.HtmlEncode(user.FirstName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.LastName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.EmailAddress)}</td>
                    </tr>");
                }

                htmlBody.AppendLine("</tbody></table>");

                // Add total count
                htmlBody.AppendLine($@"<div class='total-row'>Total users disabled: {users.Count}</div>");

                // Close HTML
                htmlBody.AppendLine("</body></html>");

                return htmlBody;
            }

            public static StringBuilder LoginValidationEmailBody(List<LoginProfileValidation> users, string message)
            {
                var htmlBody = new StringBuilder();
                // Start HTML
                htmlBody.AppendLine(@"<!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body { 
                                font-family: Arial, sans-serif; 
                                font-size: 13px; 
                                margin: 20px; 
                            }
                            .header-text { 
                                margin-bottom: 15px; 
                            }
                            .date-info {
                                color: #666;
                                margin-bottom: 15px;
                            }
                            table { 
                                width: auto; 
                                min-width: 600px;
                                max-width: 900px;
                                border-collapse: collapse; 
                                margin: 10px 0; 
                                font-size: inherit;
                            }
                            th { 
                                background-color: transparent;
                                padding: 4px 8px;
                                text-align: left;
                                border-bottom: 1px solid #000;
                                font-weight: normal;
                            }
                            td { 
                                padding: 4px 8px;
                                border-bottom: 1px solid #ddd;
                            }
                            .total-row {
                                margin-top: 15px;
                                font-weight: bold;
                            }
                            .validation-message {
                                color: #d32f2f;
                            }
                            .multi-provider {
                                background-color: #fff3e0;
                            }
                        </style>
                    </head>
                    <body>");

                // Add header text
                htmlBody.AppendLine($@"<div class='header-text'>
                    <p>{message}</p>
                </div>");

                // Add date information
                htmlBody.AppendLine($@"<div class='date-info'>Login validation performed on {DateTime.Now:MMMM dd, yyyy}</div>");

                // Create users table
                htmlBody.AppendLine(@"<table>
                    <thead>
                        <tr>
                            <th style='min-width: 100px;'>Artifact ID</th>
                            <th style='min-width: 150px;'>First Name</th>
                            <th style='min-width: 150px;'>Last Name</th>
                            <th style='min-width: 200px;'>Email Address</th>
                            <th style='min-width: 250px;'>Issue</th>
                        </tr>
                    </thead>
                    <tbody>");

                // Add user data
                foreach (var user in users)
                {
                    var rowClass = user.HasMultipleProviders ? "multi-provider" : "";

                    htmlBody.AppendLine($@"<tr class='{rowClass}'>
                        <td>{user.UserArtifactId}</td>
                        <td>{HttpUtility.HtmlEncode(user.FirstName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.LastName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.EmailAddress)}</td>
                        <td class='validation-message'>{HttpUtility.HtmlEncode(user.ValidationMessage)}</td>
                    </tr>");
                }

                htmlBody.AppendLine("</tbody></table>");

                // Add summary counts
                var multiProviderCount = users.Count(u => u.HasMultipleProviders);
                var noValidProviderCount = users.Count(u => u.HasNoValidProvider);

                htmlBody.AppendLine(@"<div class='total-row'>Summary:</div>");
                htmlBody.AppendLine($@"<div>Users with multiple providers: {multiProviderCount}</div>");
                htmlBody.AppendLine($@"<div>Users with no valid provider: {noValidProviderCount}</div>");
                htmlBody.AppendLine($@"<div class='total-row'>Total users with issues: {users.Count}</div>");

                // Close HTML
                htmlBody.AppendLine("</body></html>");
                return htmlBody;
            }

            public static StringBuilder PasswordAuthEmailBody(List<PasswordAuthValidation> users, string message)
            {
                var htmlBody = new StringBuilder();
                // Start HTML
                htmlBody.AppendLine(@"<!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body { 
                                font-family: Arial, sans-serif; 
                                font-size: 13px; 
                                margin: 20px; 
                            }
                            .header-text { 
                                margin-bottom: 15px; 
                            }
                            .date-info {
                                color: #666;
                                margin-bottom: 15px;
                            }
                            table { 
                                width: auto; 
                                min-width: 600px;
                                max-width: 900px;
                                border-collapse: collapse; 
                                margin: 10px 0; 
                                font-size: inherit;
                            }
                            th { 
                                background-color: transparent;
                                padding: 4px 8px;
                                text-align: left;
                                border-bottom: 1px solid #000;
                                font-weight: normal;
                            }
                            td { 
                                padding: 4px 8px;
                                border-bottom: 1px solid #ddd;
                            }
                            .total-row {
                                margin-top: 15px;
                                font-weight: bold;
                            }
                            .validation-message {
                                color: #d32f2f;
                            }
                        </style>
                    </head>
                    <body>");

                // Add header text
                htmlBody.AppendLine($@"<div class='header-text'>
                    <p>{message}</p>
                </div>");

                // Add date information
                htmlBody.AppendLine($@"<div class='date-info'>Password authentication validation performed on {DateTime.Now:MMMM dd, yyyy}</div>");

                // Create users table
                htmlBody.AppendLine(@"<table>
                    <thead>
                        <tr>
                            <th style='min-width: 100px;'>Artifact ID</th>
                            <th style='min-width: 150px;'>First Name</th>
                            <th style='min-width: 150px;'>Last Name</th>
                            <th style='min-width: 200px;'>Email Address</th>
                            <th style='min-width: 250px;'>Issue</th>
                        </tr>
                    </thead>
                    <tbody>");

                // Add user data
                foreach (var user in users)
                {
                    htmlBody.AppendLine($@"<tr>
                        <td>{user.UserArtifactId}</td>
                        <td>{HttpUtility.HtmlEncode(user.FirstName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.LastName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.EmailAddress)}</td>
                        <td class='validation-message'>{HttpUtility.HtmlEncode(user.ValidationMessage)}</td>
                    </tr>");
                }

                htmlBody.AppendLine("</tbody></table>");

                // Add summary count
                htmlBody.AppendLine($@"<div class='total-row'>Total users with password auth without 2FA: {users.Count}</div>");

                // Close HTML
                htmlBody.AppendLine("</body></html>");
                return htmlBody;
            }

            public static StringBuilder ItemListUpdateEmailBody(List<Users> users, string message)
            {
                var htmlBody = new StringBuilder();
                // Start HTML
                htmlBody.AppendLine(@"<!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body { 
                                font-family: Arial, sans-serif; 
                                font-size: 13px; 
                                margin: 20px; 
                            }
                            .header-text { 
                                margin-bottom: 15px; 
                            }
                            .date-info {
                                color: #666;
                                margin-bottom: 15px;
                            }
                            table { 
                                width: auto; 
                                min-width: 600px;
                                max-width: 800px;
                                border-collapse: collapse; 
                                margin: 10px 0; 
                                font-size: inherit;
                            }
                            th { 
                                background-color: transparent;
                                padding: 4px 8px;
                                text-align: left;
                                border-bottom: 1px solid #000;
                                font-weight: normal;
                            }
                            td { 
                                padding: 4px 8px;
                                border-bottom: 1px solid #ddd;
                            }
                            .total-row {
                                margin-top: 15px;
                                font-weight: bold;
                            }
                        </style>
                    </head>
                    <body>");
                // Add header text
                htmlBody.AppendLine($@"<div class='header-text'>
                    <p>{message}</p>
                    </div>");
                // Add date information
                htmlBody.AppendLine($@"<div class='date-info'>Item List Page Length updated on {DateTime.Now:MMMM dd, yyyy}</div>");
                // Create users table
                htmlBody.AppendLine(@"<table>
                    <thead>
                        <tr>
                            <th style='min-width: 100px;'>Artifact ID</th>
                            <th style='min-width: 150px;'>First Name</th>
                            <th style='min-width: 150px;'>Last Name</th>
                            <th style='min-width: 200px;'>Email Address</th>
                        </tr>
                    </thead>
                    <tbody>");
                // Add user data
                foreach (var user in users)
                {
                    htmlBody.AppendLine($@"<tr>
                        <td>{user.ArtifactId}</td>
                        <td>{HttpUtility.HtmlEncode(user.FirstName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.LastName)}</td>
                        <td>{HttpUtility.HtmlEncode(user.EmailAddress)}</td>
                    </tr>");
                }
                htmlBody.AppendLine("</tbody></table>");
                // Add total count
                htmlBody.AppendLine($@"<div class='total-row'>Total users updated: {users.Count}</div>");
                // Close HTML
                htmlBody.AppendLine("</body></html>");
                return htmlBody;
            }
        }

        public class Email
        {
            static string smtpPasswordValue;
            static int smtpPortValue;
            static string smtpUserValue;
            static string smtpServerValue;
            static string smtpEnvironmentValue;
            static string adminEmailAddress;
            static string teamEmailAddresses;
            static string supportEmailAddress;
            private static void GetSMTPValue(string settingName, SMTPSetting smtpInstanceSettingSingle, IInstanceSettingsBundle instanceSettingsBundle)
            {
                switch (settingName)
                {
                    case "SMTPPassword":
                        var singleSettingValuePass = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        smtpPasswordValue = singleSettingValuePass.Result;
                        break;

                    case "SMTPPort":
                        int singleSettingValuePort = Convert.ToInt32(instanceSettingsBundle.GetUIntAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name).Result.Value);
                        smtpPortValue = singleSettingValuePort;
                        break;

                    case "SMTPUserName":
                        var singleSettingValueUser = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        smtpUserValue = singleSettingValueUser.Result;
                        break;

                    case "SMTPServer":
                        var singleSettingValueServer = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        smtpServerValue = singleSettingValueServer.Result;
                        break;
                    case "RelativityInstanceURL":
                        var singleSettingValueEnvironment = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        smtpEnvironmentValue = singleSettingValueEnvironment.Result;
                        break;
                    case "AdminEmailAddress":
                        var singleSettingValueAdmin = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        adminEmailAddress = singleSettingValueAdmin.Result;
                        break;
                    case "TeamEmailAddresses":
                        var singleSettingValueTeam = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        teamEmailAddresses = singleSettingValueTeam.Result;
                        break;
                    case "AnalystCaseTeamUpdates":
                        var singleSettingValueSupport = instanceSettingsBundle.GetStringAsync(smtpInstanceSettingSingle.Section, smtpInstanceSettingSingle.Name);
                        supportEmailAddress = singleSettingValueSupport.Result;
                        break;
                }
            }

            public static async Task SendInternalNotificationAsync(IInstanceSettingsBundle instanceSettingsBundle, string htmlBody, string emailSubject)
            {
                SMTPSetting smtpPassword = new SMTPSetting { Section = "kCura.Notification", Name = "SMTPPassword" };
                SMTPSetting smtpPort = new SMTPSetting { Section = "kCura.Notification", Name = "SMTPPort" };
                SMTPSetting smtpServer = new SMTPSetting { Section = "kCura.Notification", Name = "SMTPServer" };
                SMTPSetting smtpUser = new SMTPSetting { Section = "kCura.Notification", Name = "SMTPUserName" };
                SMTPSetting smtpEnvironment = new SMTPSetting { Section = "Relativity.Core", Name = "RelativityInstanceURL" };
                SMTPSetting adminEmailSetting = new SMTPSetting { Section = "LTAS Billing Management", Name = "AdminEmailAddress" };

                List<SMTPSetting> smtpSettings = new List<SMTPSetting> { smtpPort, smtpServer, smtpUser, smtpPassword, smtpEnvironment, adminEmailSetting };

                foreach (var smtpInstanceSettingSingle in smtpSettings)
                {
                    try
                    {
                        GetSMTPValue(smtpInstanceSettingSingle.Name, smtpInstanceSettingSingle, instanceSettingsBundle);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                var emailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@relativity.one", "LTAS Billing Management"),
                    Subject = $"{smtpEnvironmentValue.Split('-')[1].Split('.')[0].ToUpper()} - {emailSubject}",
                    Body = htmlBody.ToString(),
                    IsBodyHtml = true
                };
                emailMessage.To.Add(adminEmailAddress);
                emailMessage.ReplyToList.Add(new MailAddress(adminEmailAddress));

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtpServerValue;
                    smtpClient.Port = smtpPortValue;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUserValue, smtpPasswordValue);
                    await smtpClient.SendMailAsync(emailMessage);
                }
            }
        }
    }
}
