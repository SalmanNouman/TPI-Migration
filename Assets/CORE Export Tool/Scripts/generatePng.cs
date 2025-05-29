using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class generatePng : MonoBehaviour

{

    [DllImport("__Internal")]

    private static extern void PngDownloader(string content, string filename);

    public static void DownloadPng(byte[] byteArray, string fileName)
    {
        Regex reg = new Regex("[*'\",&#^@:;+]");
        fileName = reg.Replace(fileName, "_");
        PngDownloader(System.Convert.ToBase64String(byteArray), fileName);
    }

}
