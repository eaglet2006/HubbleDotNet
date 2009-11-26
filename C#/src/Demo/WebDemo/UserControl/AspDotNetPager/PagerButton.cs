using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class PagerButton
    {
        private string _Url = "";
        private string _TextFormat = "";
        private string _Text = "";
        private string _CustomerText = "";
        private string _CssClass = "";
        private CssStyleCollection _Style = new CssStyleCollection(new System.Web.UI.StateBag());
        private bool _Visible = true;

        /// <summary>
        /// Get or set the css class
        /// </summary>
        public string CssClass
        {
            get
            {
                return _CssClass;
            }

            set
            {
                _CssClass = value;
            }
        }

        /// <summary>
        /// Get or set the style
        /// </summary>
        public CssStyleCollection Style 
        {
            get
            {
                return _Style;
            }
        }

        /// <summary>
        /// Get or set the URL
        /// </summary>
        [NotifyParentProperty(true)]
        public string Url
        {
            get
            {
                return _Url;
            }

            set
            {
                _Url = value;
            }
        }

        /// <summary>
        /// Get or set the Text
        /// </summary>
        [NotifyParentProperty(true)]
        public string Text
        {
            get
            {
                return _Text;
            }

            set
            {
                _Text = value;
            }
        }

        /// <summary>
        /// Get or set the Text format
        /// The format string like "[{0}]" using to format the text
        /// </summary>
        [NotifyParentProperty(true)]
        public string TextFormat
        {
            get
            {
                return _TextFormat;
            }

            set
            {
                _TextFormat = value;
            }
        }

        /// <summary>
        /// Get or set the CustomerText.
        /// We can use this property to show the customer text 
        /// replace Text property
        /// </summary>
        [NotifyParentProperty(true)]
        public string CustomerText
        {
            get
            {
                return _CustomerText;
            }

            set
            {
                _CustomerText = value;
            }
        }

        /// <summary>
        /// Get the inner text
        /// </summary>
        public string InnerText
        {
            get
            {
                string text = string.IsNullOrEmpty(CustomerText) ? GetText() : CustomerText;

                if (!string.IsNullOrEmpty(TextFormat))
                {
                    text = string.Format(TextFormat, text);
                }

                return text;
            }
        }

        /// <summary>
        /// Get the value
        /// </summary>
        public string Value
        {
            get
            {
                if (!Visible)
                {
                    return "";
                }

                if (string.IsNullOrEmpty(GetUrl()))
                {
                    return string.Format("<span class=\"{0}\" style=\"{1}\">{2}</span>",
                        CssClass, Style.Value, InnerText);
                }
                else
                {
                    return string.Format("<span class=\"{0}\" style=\"{1}\"><a href=\"{2}\">{3}</a></span>",
                        CssClass, Style.Value, GetUrl(), InnerText);
                }
            }
        }

        /// <summary>
        /// Get or set Visible
        /// </summary>
        [NotifyParentProperty(true)]
        public bool Visible
        {
            get
            {
                return _Visible;
            }

            set
            {
                _Visible = value;
            }
        }


        public virtual string GetText()
        {
            return _Text;
        }

        public virtual string GetUrl()
        {
            return _Url;
        }
    }
}
