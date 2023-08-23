using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace DevExpressWebcam.Control.WPF
{
    [XmlInclude(typeof(ImageVisualAidItem))]
    public class VisualAidItemBase : UserControl, IXmlSerializable
    {
        public VisualAidItemBase()
        {
            LayoutTransform = new RotateTransform();
            UseHighlightOutline = true;

            DeltaX = 1.0;
            DeltaY = 1.0;

            RenderTransform = new TransformGroup();
        }

        public bool IsPrintMode
        {
            get;
            set;
        }

        public virtual bool IsLineItem
        {
            get { return false; }
        }
        public virtual bool IsArrowItem
        {
            get { return false; }
        }

        [Flags]
        public enum AlignDirection
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Both = Horizontal | Vertical,
        }

        public virtual bool IsManipulationMode
        {
            get;
            set;
        }

        [XmlIgnore]
        internal int DesignerItemIndex
        {
            get;
            set;
        }

        public virtual PathGeometry GetLineGeometry(out Brush stroke, out double strokeThickness)
        {
            stroke = null;
            strokeThickness = 0.0;
            return null;
        }

        public bool UseHighlightOutline
        {
            get;
            set;
        }

        public virtual double ZoomLevel
        {
            get;
            set;
        }

        public double DeltaX
        {
            get;
            set;
        }

        public double DeltaY
        {
            get;
            set;
        }

        public void Rotate()
        {
            Rotate(new Point(.5, .5));
        }

        public void Rotate(Point rotateOrigin)
        {
            Rotate(-90.0, rotateOrigin);
        }

        public void Rotate(double angle, bool forceKeepPositive = false)
        {
            Rotate(angle, new Point(.5, .5), forceKeepPositive);
        }

        public virtual void Rotate(double angle, Point rotateOrigin, bool forceKeepPositive = false)
        {
            RenderTransformOrigin = rotateOrigin;
            if (angle > 0 && !forceKeepPositive)
                angle *= -1;

            if (!(LayoutTransform is RotateTransform))
            {
                LayoutTransform = new RotateTransform(angle % 360);
            }
            else
            {
                (LayoutTransform as RotateTransform).Angle += angle % 360;
            }
        }

        public virtual double RotationAngle
        {
            get
            {
                return !(LayoutTransform is RotateTransform) ? 0.0 : (LayoutTransform as RotateTransform).Angle;
            }
        }

        #region IXmlSerializable Members

        private const string CURRENTVERSION = "15";

        private string version = CURRENTVERSION;

        /// <summary>
        /// Serialization version for future upgradeability
        /// </summary>
        public string Version
        {
            get { return version; }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            if ((reader.LocalName.Equals("BOMLinkSyncHint") || reader.ReadToFollowing("BOMLinkSyncHint")) && !reader.IsEmptyElement)
            {
                string syncVersion = null;
                string s = reader.ReadInnerXml();

                if (!string.IsNullOrEmpty(s))
                {
                    //we need to read the version from the sync hint not the parent element
                    //this is to support backwards compatibility
                    System.Xml.Linq.XElement syncElement = System.Xml.Linq.XElement.Parse(s);

                    if (syncElement != null)
                    {
                        System.Xml.Linq.XAttribute syncVersionAttribute = syncElement.Attribute("Version");

                        if (syncVersionAttribute != null)
                            syncVersion = syncVersionAttribute.Value;
                    }                   
                }
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("BOMLinkSyncHint");

            writer.WriteEndElement();
        }

        public virtual void ReadBaseXmlAttributes(System.Xml.XmlReader reader)
        {
            version = reader.GetAttribute("Version");
            this.UseHighlightOutline = Convert.ToBoolean(reader.GetAttribute("UseHighlightOutline"));

            switch (version)
            {
                case "1":
                case "2":
                    this.Rotate(Convert.ToInt16(reader.GetAttribute("Rotation"), System.Globalization.CultureInfo.InvariantCulture));
                    this.RenderTransform = new ScaleTransform(Convert.ToDouble(reader.GetAttribute("ScaleX"), System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(reader.GetAttribute("ScaleY"), System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                    this.Rotate(Convert.ToInt16(reader.GetAttribute("Rotation"), System.Globalization.CultureInfo.InvariantCulture));
                    this.RenderTransform = new ScaleTransform(Convert.ToDouble(reader.GetAttribute("ScaleX"), System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(reader.GetAttribute("ScaleY"), System.Globalization.CultureInfo.InvariantCulture));
                    break;
                default:
                    this.Rotate(Convert.ToInt16(reader.GetAttribute("Rotation"), System.Globalization.CultureInfo.InvariantCulture));
                    this.RenderTransform = new ScaleTransform(Convert.ToDouble(reader.GetAttribute("ScaleX"), System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(reader.GetAttribute("ScaleY"), System.Globalization.CultureInfo.InvariantCulture));
                    this.Opacity = Convert.ToDouble(reader.GetAttribute("Opacity"), System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }

            if (Math.Abs((this.RenderTransform as ScaleTransform).ScaleX) < .1 || Math.Abs((this.RenderTransform as ScaleTransform).ScaleY) < .1)
            {
                this.RenderTransform = new ScaleTransform(1, 1);
            }
        }

        public virtual void WriteBaseXmlAttribues(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Version", CURRENTVERSION);
            writer.WriteAttributeString("UseHighlightOutline", Convert.ToString(this.UseHighlightOutline));
            writer.WriteAttributeString("Rotation", RotationAngle.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Opacity", this.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (RenderTransform is ScaleTransform)
            {
                writer.WriteAttributeString("ScaleX", (RenderTransform as ScaleTransform).ScaleX.ToString(System.Globalization.CultureInfo.InvariantCulture));
                writer.WriteAttributeString("ScaleY", (RenderTransform as ScaleTransform).ScaleY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttributeString("ScaleX", Convert.ToString(1.0, System.Globalization.CultureInfo.InvariantCulture));
                writer.WriteAttributeString("ScaleY", Convert.ToString(1.0, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region IXmlSerializable Helpers

        protected static Transform ReadTransformXml(string version, XmlReader reader)
        {
            Transform resultTransform = null;

            if (reader.LocalName == "Transform")
            {
                string strType = reader.GetAttribute("TransformType");
                if (strType == "TransformGroup")
                    resultTransform = new TransformGroup();
                else if (strType == "RotateTransform")
                    resultTransform = new RotateTransform();
                else if (strType == "TranslateTransform")
                    resultTransform = new TranslateTransform();
                else if (strType == "SkewTransform")
                    resultTransform = new SkewTransform();
                else if (strType == "ScaleTransform")
                    resultTransform = new ScaleTransform();

                if (resultTransform != null)
                {
                    if (resultTransform is TransformGroup)
                    {
                        if (reader.ReadToDescendant("Children") && !reader.IsEmptyElement &&
                            reader.ReadToDescendant("Transform"))
                        {
                            do
                            {
                                Transform t = ReadTransformXml(version, reader);
                                if (t != null)
                                    ((TransformGroup)resultTransform).Children.Add(t);

                            } while (reader.ReadToNextSibling("Transform"));

                            reader.Read();
                        }
                    }
                    else
                    {
                        if (resultTransform is RotateTransform)
                        {
                            RotateTransform rt = resultTransform as RotateTransform;
                            switch (version)
                            {
                                case "1":
                                case "2":
                                    rt.Angle = Convert.ToDouble(reader.GetAttribute("Angle"));
                                    rt.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"));
                                    rt.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"));
                                    break;
                                default:
                                    rt.Angle = Convert.ToDouble(reader.GetAttribute("Angle"), System.Globalization.CultureInfo.InvariantCulture);
                                    rt.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"), System.Globalization.CultureInfo.InvariantCulture);
                                    rt.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"), System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                        else if (resultTransform is TranslateTransform)
                        {
                            TranslateTransform tt = resultTransform as TranslateTransform;
                            switch (version)
                            {
                                case "1":
                                case "2":
                                    tt.X = Convert.ToDouble(reader.GetAttribute("X"));
                                    tt.Y = Convert.ToDouble(reader.GetAttribute("Y"));
                                    break;
                                default:
                                    tt.X = Convert.ToDouble(reader.GetAttribute("X"), System.Globalization.CultureInfo.InvariantCulture);
                                    tt.Y = Convert.ToDouble(reader.GetAttribute("Y"), System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                        else if (resultTransform is SkewTransform)
                        {
                            SkewTransform st = resultTransform as SkewTransform;
                            switch (version)
                            {
                                case "1":
                                case "2":
                                    st.AngleX = Convert.ToDouble(reader.GetAttribute("AngleX"));
                                    st.AngleY = Convert.ToDouble(reader.GetAttribute("AngleY"));
                                    st.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"));
                                    st.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"));
                                    break;
                                default:
                                    st.AngleX = Convert.ToDouble(reader.GetAttribute("AngleX"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.AngleY = Convert.ToDouble(reader.GetAttribute("AngleY"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"), System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                        else if (resultTransform is ScaleTransform)
                        {
                            ScaleTransform st = resultTransform as ScaleTransform;
                            switch (version)
                            {
                                case "1":
                                case "2":
                                    st.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"));
                                    st.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"));
                                    st.ScaleX = Convert.ToDouble(reader.GetAttribute("ScaleX"));
                                    st.ScaleY = Convert.ToDouble(reader.GetAttribute("ScaleY"));
                                    break;
                                default:
                                    st.CenterX = Convert.ToDouble(reader.GetAttribute("CenterX"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.CenterY = Convert.ToDouble(reader.GetAttribute("CenterY"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.ScaleX = Convert.ToDouble(reader.GetAttribute("ScaleX"), System.Globalization.CultureInfo.InvariantCulture);
                                    st.ScaleY = Convert.ToDouble(reader.GetAttribute("ScaleY"), System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                    }
                }
            }

            return resultTransform;
        }

        protected static void WriteTransformXml(string version, Transform transform, XmlWriter writer)
        {
            writer.WriteStartElement("Transform");

            if (transform is TransformGroup)
            {
                writer.WriteAttributeString("TransformType", "TransformGroup");
                writer.WriteStartElement("Children");
                foreach (Transform t in ((TransformGroup)transform).Children)
                    WriteTransformXml(version, t, writer);

                writer.WriteEndElement();
            }
            else
            {
                if (transform is RotateTransform)
                {
                    writer.WriteAttributeString("TransformType", "RotateTransform");

                    RotateTransform rt = transform as RotateTransform;
                    writer.WriteAttributeString("Angle", Convert.ToString(rt.Angle, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("CenterX", Convert.ToString(rt.CenterX, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("CenterY", Convert.ToString(rt.CenterY, System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (transform is TranslateTransform)
                {
                    writer.WriteAttributeString("TransformType", "TranslateTransform");

                    TranslateTransform tt = transform as TranslateTransform;
                    writer.WriteAttributeString("X", Convert.ToString(tt.X, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("Y", Convert.ToString(tt.Y, System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (transform is SkewTransform)
                {
                    writer.WriteAttributeString("TransformType", "SkewTransform");

                    SkewTransform st = transform as SkewTransform;
                    writer.WriteAttributeString("AngleX", Convert.ToString(st.AngleX, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("AngleY", Convert.ToString(st.AngleY, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("CenterX", Convert.ToString(st.CenterX, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("CenterY", Convert.ToString(st.CenterY, System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (transform is ScaleTransform)
                {
                    writer.WriteAttributeString("TransformType", "ScaleTransform");

                    ScaleTransform st = transform as ScaleTransform;
                    writer.WriteAttributeString("CenterX", Convert.ToString(st.CenterX, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("CenterY", Convert.ToString(st.CenterY, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("ScaleX", Convert.ToString(st.ScaleX, System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("ScaleY", Convert.ToString(st.ScaleY, System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            writer.WriteEndElement();
        }

        protected static Color ReadColorXmlAttribute(string version, XmlReader reader)
        {
            Color c = Colors.Transparent;

            if (reader.HasAttributes)
            {
                string colorString = reader.GetAttribute("Color");
                if (!string.IsNullOrEmpty(colorString))
                {
                    c = (Color)ColorConverter.ConvertFromString(colorString);
                }
            }

            return c;
        }

        protected static void WriteColorXmlAttribute(string version, Color color, XmlWriter writer)
        {
            writer.WriteAttributeString("Color", color.ToString());
        }

        #endregion

        private SolidColorBrush fillBrush = null;
        protected void ClearTransparency()
        {
            Shape shape = this.Content as Shape;
            if (shape != null)
            {
                fillBrush = shape.Fill as SolidColorBrush;
                if (fillBrush != null && fillBrush.Color.A == 0)
                    shape.Fill = null;
                else
                    fillBrush = null;

            }
        }

        protected void RestoreTransparency()
        {
            Shape shape = this.Content as Shape;
            if (shape != null && fillBrush != null)
            {
                if (!fillBrush.IsFrozen && fillBrush.CanFreeze)
                    fillBrush.Freeze();
                shape.Fill = fillBrush;
                fillBrush = null;
            }
        }
    }
}