using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Internal;
using DevExpress.Xpf.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Utils;
using System.Drawing;
using System.Drawing.Imaging;
using DevExpress.Xpf.Core.Native;
using System.IO;
using Image = System.Drawing.Image;
using System.Diagnostics;
using System.Windows.Markup;

namespace DevExpressWebcam.Control.WPF
{
    /// <summary>
    /// Interaction logic for DevExpressCam.xaml
    /// </summary>
    public partial class DevExpressCam : Window, IMediaVisualAidItem

    {
        public DevExpressCam()
        {
            InitializeComponent();
            DataType = DataTypeEnum.Bitmap;
        }

        //public override void ReadXml(System.Xml.XmlReader reader)
        //{
        //    ReadBaseXmlAttributes(reader);

        //    base.ReadXml(reader);
        //}

        //public override void WriteXml(System.Xml.XmlWriter writer)
        //{
        //    WriteBaseXmlAttribues(writer);

        //    base.WriteXml(writer);
        //}

        //public override void ReadBaseXmlAttributes(System.Xml.XmlReader reader)
        //{
        //    base.ReadBaseXmlAttributes(reader);

        //    Guid.TryParse(reader.GetAttribute("FileID"), out fileID);

        //    DataTypeEnum typeVar;
        //    if (!Enum.TryParse<DataTypeEnum>(reader.GetAttribute("DataType"), out typeVar))
        //        typeVar = DataTypeEnum.Bitmap;
        //    DataType = typeVar;

        //    int ver = 0;
        //    //int.TryParse(Version, out ver);

        //    if (ver >= 14)
        //    {
        //        this.PathRoot = reader.GetAttribute("PathRoot");
        //        this.OriginalFile = reader.GetAttribute("OriginalFile");
        //    }
        //}

        //public override void WriteBaseXmlAttribues(System.Xml.XmlWriter writer)
        //{
        //    base.WriteBaseXmlAttribues(writer);

        //    writer.WriteAttributeString("FileID", FileID.ToString());
        //    writer.WriteAttributeString("DataType", DataType.ToString());
        //    writer.WriteAttributeString("PathRoot", PathRoot);
        //    writer.WriteAttributeString("OriginalFile", OriginalFile);
        //}

        public ImageSource ImageSource
        {
            get
            {
                return img.Source;
            }
            set
            {
                img.Source = value;
            }
        }

        public DataTypeEnum DataType
        {
            get;
            set;
        }

        private byte[] mediaData = null;

        public byte[] MediaData
        {
            get
            {
                return mediaData;
            }
            set
            {
                mediaData = value;
                if (mediaData != null && mediaData.Any())
                {
                    System.Windows.Controls.Image image = null;
                    switch (DataType)
                    {
                        case DataTypeEnum.XAML:
                            ResourceDictionary resourceDictionary = (ResourceDictionary)XamlReader.Parse(Encoding.Unicode.GetString(mediaData));
                            ContentControl c = new ContentControl();
                            c.Content = resourceDictionary[fileID.ToString()];
                            break;
                        default:
                            image = new System.Windows.Controls.Image()
                            {
                                Source = DevExCameraHelper.GetBitmapImageSourceFromByteArray(mediaData),
                            };
                            break;
                    }

                    if (image != null)
                    {
                        image.Stretch = Stretch.Uniform;
                        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                        ContentControl c = new ContentControl();
                        c.Content = image;
                    }

                    //this.IsHitTestVisible = false;
                    //DesignerItem di = this.VisualParent as DesignerItem;

                    //if (di != null)
                    //    di.IsHitTestVisible = true;
                }
            }
        }

        Guid fileID = SequentialGuid.NewGuid();

        public Guid FileID
        {
            get
            {
                return fileID;
            }
            set
            {
                fileID = value;
            }
        }

        string pathRoot;
        public string PathRoot
        {
            get
            {
                return pathRoot;
            }
            set
            {
                pathRoot = value;
            }
        }

        string originalFile;
        public string OriginalFile
        {
            get
            {
                return originalFile;
            }
            set
            {
                originalFile = value;
            }
        }

        public enum DataTypeEnum
        {
            Bitmap,
            XAML
        }
    }
}
