using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationPattern;
using ConfigurationPattern.Patterns;
using ConfigurationPattern.Patterns.Ini;

namespace TestHubbleCore
{
    [ConfigurationPattern(TPattern.INI, "Test")]
    internal class Config : Configuration
    {
        private bool _TestHubble;
        private bool _TestLucene;
        private int _TestRows;
        private bool _PerformanceTest;
        private String _NewsXmlFilePath;
        private string _QueryString = "北京大学";

        public Config()
            : base("Test.Ini")
        {
        }

        #region Public Propertys

        public bool TestHubble
        {
            get
            {
                return _TestHubble;
            }

            set
            {
                _TestHubble = value;
            }
        }

        public bool TestLucene
        {
            get
            {
                return _TestLucene;
            }

            set
            {
                _TestLucene = value;
            }
        }

        public bool PerformanceTest
        {
            get
            {
                return _PerformanceTest;
            }

            set
            {
                _PerformanceTest = value;
            }
        }

        public int TestRows
        {
            get
            {
                return _TestRows;
            }

            set
            {
                _TestRows = value;
            }
        }

        public String NewsXmlFilePath
        {
            get
            {
                return _NewsXmlFilePath;
            }

            set
            {
                _NewsXmlFilePath = value;
            }
        }

        public String QueryString
        {
            get
            {
                return _QueryString;
            }

            set
            {
                _QueryString = value;
            }
        }

        #endregion
    }
}
