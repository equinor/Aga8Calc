using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aga8CalcService
{
    public class Aga8OpcClient
    {
        private const int ReconnectPeriod = 10;
        public Session OpcSession { get; set; }
        private SessionReconnectHandler reconnectHandler;
        private readonly string endpointUrl;
        private static bool autoAccept = false;
        private readonly UserIdentity user;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Aga8OpcClient(string endpointUrl, bool autoAccept, string username, string password)
        {
            this.endpointUrl = endpointUrl;
            Aga8OpcClient.autoAccept = autoAccept;

            user = new UserIdentity(username, password);
        }

        public async Task Connect()
        {
            try
            {
                logger.Info("Create an Application Configuration.");

                ApplicationInstance application = new ApplicationInstance
                {
                    ApplicationName = "Aga8 Calc Client",
                    ApplicationType = ApplicationType.Client,
                    ConfigSectionName = "Aga8_Calc_Client"
                };

                // load the application configuration.
                ApplicationConfiguration config = await application.LoadApplicationConfiguration(false);

                // check the application certificate.
                bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
                if (!haveAppCertificate)
                {
                    throw new Exception("Application instance certificate invalid!");
                }

                if (haveAppCertificate)
                {
                    config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                    if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                    {
                        autoAccept = true;
                    }
                    config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
                }
                else
                {
                    Console.WriteLine("    WARN: missing application certificate, using unsecure connection.");
                }

                logger.Info($"Discover endpoints of { endpointUrl }.");
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointUrl, haveAppCertificate, 15000);
               
                logger.Info("Selected endpoint uses: {0}",
                    selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

                logger.Info("Create a session with OPC UA server.");
                var endpointConfiguration = EndpointConfiguration.Create(config);
                var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                OpcSession = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, user, null);

                // register keep alive handler
                OpcSession.KeepAlive += Client_KeepAlive;
            }
            catch (Exception ex)
            {
                Utils.Trace("ServiceResultException:" + ex.Message);
                logger.Fatal(ex, "Failed to connect to Opc server.");
                throw;
            }
        }

        public void DisConnect()
        {
            // stop any reconnect operation.
            if (reconnectHandler != null)
            {
                reconnectHandler.Dispose();
                reconnectHandler = null;
            }

            // disconnect any existing session.
            if (OpcSession != null)
            {
                OpcSession.Close(5000);
                OpcSession.Dispose();
                OpcSession = null;
            }
        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                Console.WriteLine("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (reconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            OpcSession = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            Console.WriteLine("--- RECONNECTED ---");
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;
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
