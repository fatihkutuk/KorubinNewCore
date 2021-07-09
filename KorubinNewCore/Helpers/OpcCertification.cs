using Opc.Ua;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Helpers
{
    public class OpcCertification
    {
        private bool autoAccept = false;

        public OpcCertification()
        {

        }
        public ApplicationConfiguration GetConfiguration()
        {
            try
            {
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationName = "ConsoleClient";
                application.ApplicationType = ApplicationType.Client;
                application.ConfigSectionName = "ClientConfiguration";
                ApplicationConfiguration config = application.LoadApplicationConfiguration(false).Result;

                Console.WriteLine("configuration loaded");

                bool certOk = application.CheckApplicationInstanceCertificate(false, 0).Result;
                if (!certOk)
                {
                    Console.WriteLine("Application instance certificate invalid!");

                    throw new Exception("Application instance certificate invalid!");
                }

                if (!config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
                }
                return config;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CertificateValidator_CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;///???
                if (autoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }
    }
}
