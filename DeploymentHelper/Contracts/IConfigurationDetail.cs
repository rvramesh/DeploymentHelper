using System;
namespace DeploymentHelper
{
    public interface IConfigurationDetail
    {
        string AppName { get; set; }
        string ConfigLocation { get; set; }
        string EnvironmentName { get; set; }
        System.Collections.Generic.IEnumerable<IConfigurationSetting> Settings { get; }
    }
}
