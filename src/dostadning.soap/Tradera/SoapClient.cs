using dostadning.domain.result;
using dostadning.soap.tradera.publicservice;

namespace dostadning.soap.tradera
{
    public abstract class SoapClient<T>
    {
        protected SoapClient(
            T c,
            AppIdentity app)
        { Client = c; App = app; }

        protected T Client { get; }
        protected AppIdentity App { get; }

        protected AuthenticationHeader Auth => new AuthenticationHeader
        {
            AppId = App.Id,
            AppKey = App.Key
        };

        protected static ConfigurationHeader Conf => new ConfigurationHeader();
    }
}