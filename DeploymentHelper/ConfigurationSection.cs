using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.IO;

namespace DeploymentHelper
{

    public class ConfigSettingCollection : DeploymentHelper.IConfigSettingCollection
    {
        private List<IConfigurationDetail> _collection = new List<IConfigurationDetail>();

        public void Add(IConfigurationDetail element)
        {
            _collection.Add(element);
        }

        public IConfigurationDetail Get(string appName, string environmentName)
        {
            var result = from item in _collection
                         where item.EnvironmentName.Equals(environmentName,StringComparison.OrdinalIgnoreCase) &&
                         item.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase)
                         select item;

            if(result != null && result.Count()> 0)
            {
                return result.ElementAt(0);
            }
            return null;
        }
    }

    public class ConfigurationLoader : ConfigurationSection
    {

        private ConfigurationLoader()
        {
        }

        public static ConfigSettingCollection ScanAndGetConfig()
        {
            ConfigSettingCollection csc = new ConfigSettingCollection();
            string[] files = Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) , "*.config", SearchOption.TopDirectoryOnly);
            
            foreach (var file in files)
            {

                ConfigurationLoader c = new ConfigurationLoader(); 
                c.Load(file);
                csc.Add(c.Section);
            }
            return csc;
        }

        private void Load(string uri)
        {
            try
            {
                using (XmlReader reader = new XmlTextReader(uri))
                {
                    base.DeserializeElement(reader, false);
                }
            }
            catch
            {
                //TODO: Handle cases where config files not belong to our config section.
            }
        }
        
        [ConfigurationProperty("appConfig")]
        public MyConfigurationSection Section
        {
            get
            {
                return this["appConfig"] as MyConfigurationSection;
            }
            set
            {
                this["appConfig"] = value;
            }
        }
    }

    public class MyConfigurationSection : System.Configuration.ConfigurationElement, DeploymentHelper.IConfigurationDetail
    {
        [ConfigurationProperty("environmentName")]
        public string EnvironmentName
        {
            get
            {
                return this["environmentName"] as string;
            }
            set
            {
                this["environmentName"] = value;
            }
        }

        [ConfigurationProperty("appName")]
        public string AppName
        {
            get
            {
                return this["appName"] as string;
            }
            set
            {
                this["appName"] = value;
            }
        }


        [ConfigurationProperty("appLocation")]
        public string ConfigLocation
        {
            get
            {
                return this["appLocation"] as string;
            }
            set
            {
                this["appLocation"] = value;
            }
        }

        [ConfigurationProperty("settings", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        private MyConfigurationElementCollection SettingsInternal
        {
            get
            {
                return (MyConfigurationElementCollection)base["settings"];
            }
        }

        public IEnumerable<IConfigurationSetting> Settings
        {
            get
            {
                for (int i = 0; i < this.SettingsInternal.Count; i++)
                {
                    yield return this.SettingsInternal[i] as IConfigurationSetting;
                }
            }
        }
    }

    public class MyConfigurationElement : ConfigurationElement, DeploymentHelper.IConfigurationSetting
    {
        [ConfigurationProperty("key", DefaultValue = "")]
        public string Key
        {
            get
            {
                return (string)base["key"];
            }
            set
            {
                base["key"] = value;
            }
        }

        [ConfigurationProperty("attributeName", DefaultValue = "")]
        public string AttributeName
        {
            get
            {
                return (string)base["attributeName"];
            }
            set
            {
                base["attributeName"] = value;
            }
        }

        [ConfigurationProperty("matchExpression", DefaultValue = "")]
        public string MatchExpression
        {
            get
            {
                return (string)base["matchExpression"];
            }
            set
            {
                base["matchExpression"] = value;
            }
        }

        [ConfigurationProperty("replaceValue", DefaultValue = "")]
        public string ReplaceValue
        {
            get
            {
                return (string)base["replaceValue"];
            }
            set
            {
                base["replaceValue"] = value;
            }
        }

        [ConfigurationProperty("id", DefaultValue = "")]
        public string Id
        {
            get
            {
                return (string)base["id"];
            }
            set
            {
                base["id"] = value;
            }
        }
    }

    [ConfigurationCollection(typeof(MyConfigurationElement))]
    public class MyConfigurationElementCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {

            return new MyConfigurationElement();
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "setting";
            }
        }


        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MyConfigurationElement)element).Id;
        }

        public MyConfigurationElement this[int index]
        {
            get
            {
                return (MyConfigurationElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
    }
}
