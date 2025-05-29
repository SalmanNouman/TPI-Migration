using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class generatePdf : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void createDocument();
    [DllImport("__Internal")]
    private static extern void addHeader(string dateTime, string base64ImageData, string font, string fontColor, int fontSize, int horizontalMargin);
    [DllImport("__Internal")]
    private static extern void addFooter(string footerText, string leftFont, int leftFontSize, string leftFontColor, string rightFont, int rightFontSize, string rightFontColor, int horizontalMargin);
    [DllImport("__Internal")]
    private static extern void downloadPdf(string filename);

    [DllImport("__Internal")]
    private static extern void addContent(string content, float fontSize, string font, string alignment, string colour);

    [DllImport("__Internal")]
    private static extern void addLine(string lineStyle, int startX, int startY, int endX, int endY, string lineColour);

    [DllImport("__Internal")]
    private static extern void addContentWithXandYPosition(string content, float ftSize, string fontName, string alignment, string colour, int posX, int posY, float moveDown);

    [DllImport("__Internal")]
    private static extern void addTextWithUnderline(string content, string fontName, float fontSize, float moveLineDown);

    [DllImport("__Internal")]
    private static extern void moveLinesDown(float linesDown);

    [DllImport("__Internal")]
    private static extern void addSlicedStringFontAndColour(string content, int sliceCount, int sliceMax, string fontOne, string fontTwo, string colourOne, string colourTwo, float moveLineDown);

    [DllImport("__Internal")]
    private static extern void addPage();

    [DllImport("__Internal")]
    private static extern void addNumberedList(int number, string content);

    [DllImport("__Internal")]
    private static extern int getLineHeight();

    [DllImport("__Internal")]
    private static extern void setFont(string fontName, float fontSize);

    /// <summary>
    /// Function called when a new PDF document needs to be created
    /// Page Size is set to Letter, but this could be changed to add a page size paramenter
    /// </summary>
    public static void CreatePDFDocument()
    {
        createDocument();
    }

    /// <summary>
    /// Funtion that adds a new page to the PDF document, same as a page break
    /// </summary>
    public static void AddPage()
    {
        addPage();
    }
    /// <summary>
    /// Function to add a header to the PDF document
    /// </summary>
    /// <param name="dateTime"> datetime text that will be added </param>
    /// <param name="base64ImageData"> Logo image data in base64 string format </param>
    /// <param name="font"> The font style for the datetime text </param>
    /// <param name="fontColor"> The font color for the datatime text </param>
    /// <param name="fontSize"> The font size for the datatime text </param>
    /// <param name="horizontalMargin"> The horizontal margin for both logo and datetime </param>
    public static void AddHeader(string dateTime, string base64ImageData, string font, string fontColor, int fontSize, int horizontalMargin)
    {
        addHeader(dateTime, base64ImageData, font, fontColor, fontSize, horizontalMargin);
    }

    /// <summary>
    /// Function to add text content to the PDF document
    /// </summary>
    /// <param name="content">text that will be added</param>
    /// <param name="ftSize">font size</param>
    /// <param name="fontName">Font name, can use Fonts.cs for list of available fonts</param>
    /// <param name="alignment">text alignment (left, right, center, justify)</param>
    /// <param name="colour">colour of the text name or HEX</param>
    public static void AddContent(string content, float ftSize, string fontName, string alignment, string colour)
    {
        addContent(content, ftSize, fontName, alignment, colour);
    }

    /// <summary>
    /// Draws a line from point A to point B
    /// </summary>
    /// <param name="lineStyle">style of line cap (butt, round,square)</param>
    /// <param name="startX">x position of the first point</param>
    /// <param name="startY">y position of the first point</param>
    /// <param name="endX">x position of the second point</param>
    /// <param name="endY">y position of the second point</param>
    /// <param name="lineColour">colour of the line name or HEX</param>
    public static void AddLine(string lineStyle, int startX, int startY, int endX, int endY, string lineColour = "black")
    {
        addLine(lineStyle, startX, startY, endX, endY, lineColour);
    }

    /// <summary>
    /// Function to add content to the PDF document in a specified position in the page
    /// </summary>
    /// <param name="content">text</param>
    /// <param name="fonttSize">font size</param>
    /// <param name="fontName">Font name, can use Fonts.cs for list of available fonts</param>
    /// <param name="alignment">text alignment (left, right, center, justify)</param>
    /// <param name="colour">colour of the text name or HEX</param>
    /// <param name="posX">x,y position of the start point of the text</param>
    /// <param name="posY">x,y position of the start point of the text</param>
    /// <param name="moveDown">number of lines to move down after the content</param>
    public static void AddContentWithXAndYPositions(string content, float fonttSize, string fontName, string alignment, string colour, int posX, int posY, float moveDown)
    {
        addContentWithXandYPosition(content, fonttSize, fontName, alignment, colour, posX, posY, moveDown);
    }

    /// <summary>
    /// Function to add underlined text to the PDF document
    /// </summary>
    /// <param name="content">text</param>
    /// <param name="fontName">Font name, can use Fonts.cs for list of available fonts</param>
    /// <param name="fontSize">font size</param>
    /// <param name="moveLineDown">number of lines to move down after the content</param>
    public static void AddTextWithUnderline(string content, string fontName, float fontSize, float moveLineDown)
    {
        addTextWithUnderline(content, fontName, fontSize, moveLineDown);
    }

    /// <summary>
    /// Move lines down
    /// </summary>
    /// <param name="linesDown">Number of lines to move down</param>
    public static void MoveLinesDown(float linesDown)
    {
        moveLinesDown(linesDown);
    }

    /// <summary>
    /// Function that allows content to have 2 different fonts and/or two different colours
    /// </summary>
    /// <param name="content">text</param>
    /// <param name="sliceCount">number of characters in the first part</param>
    /// <param name="sliceMax">total number of characters in content</param>
    /// <param name="fontOne">font that will be applied to all characters up to the slice count</param>
    /// <param name="fontTwo">font that will be applied to the rest of the characters.</param>
    /// <param name="colourOne">colour that will be applied to all characters up to the slice count</param>
    /// <param name="colourTwo">colour that will be applied to the rest of the characters.</param>
    /// <param name="moveLineDown">number of lines to move down after the content</param>
    public static void AddSlicedStringFontAndColour(string content, int sliceCount, int sliceMax, string fontOne, string fontTwo, string colourOne, string colourTwo, float moveLineDown)

    {
        addSlicedStringFontAndColour(content, sliceCount, sliceMax, fontOne, fontTwo, colourOne, colourTwo, moveLineDown);
    }

    /// <summary>
    /// Function to add a footer to the PDF document
    /// </summary>
    /// <param name="footerText"> The string text for the footer left text </param>
    /// <param name="leftFont"> The font style for the footer text </param>
    /// <param name="leftFontSize"> The font size for the footer text </param>
    /// <param name="leftFontColor"> The font color for the footer text </param>
    /// <param name="rightFont"> The font style for the footer page number </param>
    /// <param name="rightFontSize">The font size for the footer page number </param>
    /// <param name="rightFontColor"> The font color for the footer page number </param>
    /// <param name="horizontalMargin"> The horizontal margin for the footer text and page number </param>
    public static void AddFooter(string footerText, string leftFont, int leftFontSize, string leftFontColor, string rightFont, int rightFontSize, string rightFontColor, int horizontalMargin)
    {
        addFooter(footerText, leftFont, leftFontSize, leftFontColor, rightFont, rightFontSize, rightFontColor, horizontalMargin);
    }

    /// <summary>
    /// Triggers the PDF download using a filename.
    /// </summary>
    /// <param name="filename">The name of the file to be downloaded</param>
    public static void DownloadPDF(string filename)
    {
        downloadPdf(filename);
    }

    /// <summary>
    /// Creates a numbered list based from a list of strings.
    /// </summary>
    /// <param name="content">List with text to be added to the list</param>
    public static void AddNumberedListItem(List<string> content)
    {
        var i = 1;

        foreach (string log in content)
        {
            addNumberedList(i, log);
            i++;
        }
    }

    /// <summary>
    /// Gets the line height
    /// </summary>
    /// <returns>line height</returns>
    public static int GetLineHeight()
    {
        return getLineHeight();
    }

    /// <summary>
    /// Sets the font style and size
    /// </summary>
    /// <param name="fontName">font name</param>
    /// <param name="fontSize">font size</param>
    public static void SetFont(string fontName, float fontSize)
    {
        setFont(fontName, fontSize);
    }
}