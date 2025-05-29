using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class GenerateZip : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void InitZip(string zipIdPtr);
    [DllImport("__Internal")]
    private static extern void AddFileToZip(string zipIdPtr, string fileNamePtr, string base64DataPtr);
    [DllImport("__Internal")]
    private static extern void DownloadZip(string zipIdPtr, string zipFileNamePtr);

    /// <summary>
    /// Function that calls the InitZip function in the zipExport.jslib file. It initializes the zip file.
    /// </summary>
    public static void CreateZip(string zipId)
    {
        InitZip(zipId);
    }
    /// <summary>
    /// Function that calls the AddFileToZip function in the zipExport.jslib file. It adds the file to the zip file based on the
    /// fileName and base64Data provided.
    /// </summary>
    /// <param name="fileName"> The name of the file to be added to the zip file </param>
    /// <param name="base64Data"> The base64 data of the file to be added to the zip file </param>
    public static void AddPhotoToZip(string zipId, string fileName, string base64Data)
    {
        AddFileToZip(zipId, fileName, base64Data);
    }
    /// <summary>
    /// Function that calls the ZipDownloader function in the zipExport.jslib file. It downloads the zip files based on the information
    /// provided in the filesJson object and the zipFileName.
    /// </summary>
    /// <param name="filesJson"> The json object consisting of the file data in a string format </param>
    /// <param name="zipFileName"> The filename specified for the zip file </param>
    public static void DownloadZipFile(string zipId, string zipFileName)
    {
        Regex reg = new Regex("[*'\",&#^@:;+]");
        zipFileName = reg.Replace(zipFileName, "_");
        DownloadZip(zipId, zipFileName);
    }
}