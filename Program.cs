using Amazon.CognitoIdentity.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace DisparadorEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lendo arquivos...");
            //...

            IEnumerable<string> subDirs = ReadFoldersAndFiles();

            Console.WriteLine($"Encontrado {subDirs.Count()} pastas.");
            //Console.WriteLine("Deseja Enviar os emails?");

            ReadFoldersAndFiles(true);


        }

        private static IEnumerable<string> ReadFoldersAndFiles(bool sendEmail = false)
        {
            var baseDir = ConfigurationManager.AppSettings["Directory"];//AppDomain.CurrentDomain.BaseDirectory; //Read from config...
            IEnumerable<string> subDirs = Directory.GetDirectories(baseDir).ToList();

            foreach (string subdir in subDirs)
            {
                var clientEmail = Path.GetFileName(subdir);
                Console.WriteLine($"Lendo arquivos da pasta: { clientEmail }");
                List<string> filesFromDir = Directory.GetFiles(subdir).ToList();

                //foreach (var file in filesFromDir)
                //{
                //    Console.WriteLine($"{clientEmail} - Arquivo: { Path.GetFileName(file) }");
                //}

                if (sendEmail && filesFromDir.Count > 0)
                {
                    bool resultEmail = SendEmail(clientEmail, filesFromDir);
                    if(resultEmail)
                    {
                        foreach (var item in filesFromDir)
                        {
                            File.Delete(item);
                        }
                    }
                }
            }

            //Console.WriteLine("5 emails enviados!");
            //Console.ReadKey();

            return subDirs;
        }

        private static bool SendEmail(string to, List<string> filePaths)
        {
            MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["FromEmail"], to);


            SmtpClient smtp = new SmtpClient();
            smtp.Host = ConfigurationManager.AppSettings["Host"];
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            Console.WriteLine("Enviando Email......");
            mail.To.Add(to);
            mail.Subject = "Relatorio RCA";
            mail.Body = ConfigurationManager.AppSettings["Body"];
            foreach (var filePath in filePaths)
            {
                mail.Attachments.Add(new Attachment(filePath));
            }
            
            smtp.Send(mail);
            mail.Dispose();


            System.Threading.Thread.Sleep(3000);
            Console.WriteLine("Email enviado.");
           

            return true;

        }
    }
}
