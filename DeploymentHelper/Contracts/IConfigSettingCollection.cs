using System;
namespace DeploymentHelper
{
    public interface IConfigSettingCollection
    {
        IConfigurationDetail Get(string appName, string environmentName);
    }
}
