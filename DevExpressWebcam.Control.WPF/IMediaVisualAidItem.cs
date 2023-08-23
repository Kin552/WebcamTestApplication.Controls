using System;

namespace DevExpressWebcam.Control.WPF
{
    internal interface IMediaVisualAidItem
    {
        Guid FileID { get; }

        byte[] MediaData { get; }
    }
}