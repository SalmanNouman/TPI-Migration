
using UnityEngine;
using UnityEngine.UIElements;

public class PDFExportBuilder : MonoBehaviour
{
    public UIDocument PDFExportUI;

    private TextField headerText;
    private TextField contentText;
    private TextField footerText;

    private Button headerButton;
    private Button contentButton;
    private Button footerButton;
    private Button downloadButton;
    private Button createButton;

    private void Start()
    {
        headerText = PDFExportUI.rootVisualElement.Q<TextField>("HeaderTxt");
        contentText = PDFExportUI.rootVisualElement.Q<TextField>("ContentTxt");
        footerText = PDFExportUI.rootVisualElement.Q<TextField>("FooterTxt");

        headerButton = PDFExportUI.rootVisualElement.Q<Button>("HeaderBtn");
        contentButton = PDFExportUI.rootVisualElement.Q<Button>("ContentBtn");
        footerButton = PDFExportUI.rootVisualElement.Q<Button>("FooterBtn");
        downloadButton = PDFExportUI.rootVisualElement.Q<Button>("SubmitBtn");
        createButton = PDFExportUI.rootVisualElement.Q<Button>("CreateBtn");

        headerText.label = "Enter header text: ";
        headerText.labelElement.style.color = Color.white;
        headerText.labelElement.style.fontSize = 20f;
        contentText.label = "Enter content text:";
        contentText.labelElement.style.color = Color.white;
        contentText.labelElement.style.fontSize = 20f;
        footerText.label = "Enter footer text:";
        footerText.labelElement.style.color = Color.white;
        footerText.labelElement.style.fontSize = 20f;

        headerButton.clicked += HeaderButtonClicked;
        contentButton.clicked += ContentButtonClicked;
        footerButton.clicked += FooterButtonClicked;
        downloadButton.clicked += DownloadButtonClicked;
        createButton.clicked += CreateButtonClicked;
    }

    private void HeaderButtonClicked()
    {
        Debug.Log(headerText.value);
        // Load resource from the Resources folder
        Texture2D logoTexture = Resources.Load<Texture2D>("pdfLogo");
        byte[] logoData = logoTexture.EncodeToPNG();
        // Convert byte array to base64
        string base64LogoData = System.Convert.ToBase64String(logoData);
        generatePdf.AddHeader(headerText.value, base64LogoData, "Times-Roman", "black", 10, 50);
        headerText.value = "";

    }

    private void FooterButtonClicked()
    {
        Debug.Log(footerText.value);
        generatePdf.AddFooter(footerText.value, "Times-Roman", 9, "black", "Times-Roman", 10, "black", 50);
        footerText.value = "";
    }

    private void CreateButtonClicked()
    {
        generatePdf.CreatePDFDocument();
    }

    private void ContentButtonClicked()
    {
        Debug.Log(contentText.value);
        generatePdf.AddContent(contentText.value, 10, "Times-Roman", "left", "black");
        contentText.value = "";
    }

    private void DownloadButtonClicked()
    {
        generatePdf.DownloadPDF("PDFKit_UI.pdf");
    }
}
