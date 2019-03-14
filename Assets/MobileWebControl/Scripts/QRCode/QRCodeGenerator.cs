using System.Collections;
using System.Collections.Generic;
using MobileWebControl.Network;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace MobileWebControl.QRCode
{
    public class QRCodeGenerator : MonoBehaviour
    {
        public int qRCodeWidth = 256;
        public int qRCodeHeight = 256;

        public GameObject qRCodeArea;

        private void Start()
        {
            if (qRCodeArea)
            {
                if (qRCodeArea.GetComponentInChildren<Image>())
                {
                    qRCodeArea.GetComponentInChildren<Image>().material.mainTexture = GenerateQRCode();
                }
                if (qRCodeArea.GetComponentInChildren<Text>())
                {
                    qRCodeArea.GetComponentInChildren<Text>().text = MobileWebController.Instance.webServerAddress;
                }
            }
        }
        public Texture2D GenerateQRCode()
        {
            return GenerateQRCode(MobileWebController.Instance.webServerAddress);
        }

        public Texture2D GenerateQRCode(string address)
        {
            Debug.Log($"generating QR Code for {address}");
            var encoded = new Texture2D(qRCodeWidth, qRCodeHeight);
            var color32 = Encode(address, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
            return encoded;
        }

        private Color32[] Encode(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width
                }
            };
            return writer.Write(textForEncoding);
        }

    }
}