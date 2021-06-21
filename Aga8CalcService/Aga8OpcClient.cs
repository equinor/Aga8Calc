using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Aga8CalcService
{
    public sealed class Aga8OpcClient : IDisposable
    {
        private const int ReconnectPeriod = 10;
        public Session OpcSession { get; set; }
        private SessionReconnectHandler reconnectHandler;
        private readonly string endpointUrl;
        private static bool autoAccept = false;
        private readonly UserIdentity user;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Aga8OpcClient(string endpointUrl, string username, string password)
        {
            this.endpointUrl = endpointUrl;
            if (string.IsNullOrEmpty(username))
            {
                // Empty usename means that we create an Anonymous token
                user = new UserIdentity();
            }
            else
            {
                user = new UserIdentity(username, password);
            }
        }

        public void Dispose()
        {
            if (reconnectHandler != null)
            {
                reconnectHandler.Dispose();
                reconnectHandler = null;
            }
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
                    config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                    if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                    {
                        autoAccept = true;
                    }
                    config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
                }
                else
                {
                    logger.Warn("Missing application certificate, using unsecure connection.");
                }

                logger.Info($"Discover endpoints of { endpointUrl }.");
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointUrl, haveAppCertificate, 15000);

                logger.Info(CultureInfo.InvariantCulture, "Selected endpoint uses: {0}",
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
                logger.Info("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (reconnectHandler == null)
                {
                    logger.Info("Reconnecting");
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

            logger.Info("Reconnected");
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;
                if (autoAccept)
                {
                    logger.Info(CultureInfo.InvariantCulture, "Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    logger.Warn(CultureInfo.InvariantCulture, "Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }
    }
}
