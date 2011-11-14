using System;
namespace DeploymentHelper
{
    public interface IConfigurationSetting
    {
        string AttributeName { get; set; }
        string Id { get; set; }
        string Key { get; set; }
        string MatchExpression { get; set; }
        string ReplaceValue { get; set; }
    }
}
