using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateComplexMaterialFromTexture
{
    [MenuItem("VAR Tools/Create Complex Material")]

    public static void CreateComplexDiffuseMaterial()
    {
        //populate this with the explicit expected shader. In the future we can expand this script to use other shaders, but it will currently only work for the In-House MRO Shaders
        string expectedShader = "Shader Graphs/MRO Universal Workflow Triple Mask";

        var selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        var cnt = selectedAsset.Length * 1.0f;
        var idx = 0f;
        List<Texture2D> tx2D = new List<Texture2D>();
        foreach (Object obj in selectedAsset)
        {
            idx++;
            EditorUtility.DisplayProgressBar("Create material", "Create material for: " + obj.name, idx / cnt);

            if (obj is Texture2D)
            {
                tx2D.Add(obj as Texture2D);
            }

        }
        CreateComplexMatFromTx(tx2D, Shader.Find(expectedShader));
        //CreateComplexMatFromTx(tx2D, Shader.Find("Universal Render Pipeline/Lit"));
        EditorUtility.ClearProgressBar();
    }
    static void CreateComplexMatFromTx(List<Texture2D> tx2D, Shader shader)
    {
        //Set these to match your project structure
        string materialSavingFilePath = "Assets/Art/Materials/";
        string materialExtension = "_Mat.mat";

        Texture2D colorMap = null;
        Texture2D CVMask = null;
        Texture2D MROMap = null;
        Texture2D normalMap = null;
        Texture2D emissiveMap = null;
        Texture2D emissiveMask = null;

        if (Directory.Exists(materialSavingFilePath))
        {
            Debug.Log("There's a folder where you think it is!");
        }
        else
        {
            Debug.Log("The specified folder does not exist. Exiting.");
            return;
        }

        foreach (Texture2D tex in tx2D)
        {
            string n = tex.name.ToLower();
            n = n.Substring(n.IndexOf("_"));
            if (n.Contains("_color") || n.Contains("_colour"))
            {
                colorMap = tex;
                Debug.Log("Color Map: " + colorMap.name);
            }
            else if (n.Contains("_cvm"))
            {
                MROMap = tex;
                Debug.Log("Colour Variation Mask: " + CVMask.name);
            }
            else if (n.Contains("_mro"))
            {
                MROMap = tex;
                Debug.Log("MRO Map: " + MROMap.name);
            }
            else if (n.Contains("_normal"))
            {
                normalMap = tex;
                Debug.Log("Normal Map: " + normalMap.name);
            }
            else if (n.Contains("_emis"))
            {
                emissiveMap = tex;
                Debug.Log("Emission Map: " + emissiveMap.name);
            }
            else if (n.Contains("_em"))
            {
                emissiveMask = tex;
                Debug.Log("Emission Mask: " + emissiveMask.name);
            }
        }

        if (colorMap == null)
        {
            Debug.Log("This function expects that at least one .png with the suffix '_color' or '_colour' exists, but none was found. Exiting.");
            return;
        }

        /*
        //This code retrieves the path of the selected files. Not being used but could be leveraged in the future
        var path = AssetDatabase.GetAssetPath(tx2D[0]);
        if (File.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }
        */

        var mat = new Material(shader);

        mat.SetTexture("_Color_Map", colorMap);
        mat.SetTexture("_Three_Channel_Tint_Mask", CVMask);
        mat.SetTexture("_Normal_Map", normalMap);
        mat.SetTexture("_MRO_Map", MROMap);
        mat.SetTexture("_Emissive_Map", emissiveMap);
        mat.SetTexture("_Emissive_Mask", emissiveMask);

        //The value 0 specifies an opaque workflow. For a transparent workflow set the value to 1 instead
        mat.SetFloat("_Surface", 0);
        //For assets textured in Substance we often want a default roughness intensity of 0.3. This can be changed 
        //via the inspector after generation, but 0.3 should get us a reasonable baseline for most instances. 
        mat.SetFloat("_Roughness_Intensity", 0.3f);
        //Set the specular preservation to ON. This means that anything that has an alpha of 0 will still have data representing its reflected light
        mat.SetFloat("_BlendModePreserveSpecular", 1);


        string rawName = FindRawName(colorMap.name);

        AssetDatabase.CreateAsset(mat, materialSavingFilePath + rawName + materialExtension);
    }

    //tokenizes a file name and drops the last token, which we expect to be an identifier for the type of file/texture
    static string FindRawName(string sourceName)
    {
        string rawName = null;

        string[] splitName = sourceName.Split("_");

        for (int i = 0; i < splitName.Length - 1; i++)
        {
            if (i == 0)
            {
                rawName = rawName + splitName[i];
            }
            else
            {
                rawName = rawName + "_" + splitName[i];
            }
        }
        return rawName;
    }
}
