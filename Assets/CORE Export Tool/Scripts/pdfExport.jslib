// Global variables to store the PDF document and stream
let doc;
let stream;

mergeInto(LibraryManager.library, {
    /* 
       Function: downloadPdf(String filename)
       This function is called from the C# code to download the PDF. It takes the filename as a parameter
       and creates a blob from the PDF stream. It then creates a URL from the blob and downloads the PDF file.
       Finally, it revokes the URL to free up memory.
       Parameters: (String) filename - the name of the file to download.
    */
    downloadPdf: function (filename) {
        const filenameStr = UTF8ToString(filename);
        doc.end();
        stream.on("finish", function () {
            const blob = stream.toBlob("application/pdf");
            const url = URL.createObjectURL(blob);
            var link = document.createElement("a");
            link.href = url;
            link.download = filenameStr;
            link.click();
            window.URL.revokeObjectURL(url);
        });
    },
    /*
       Function: createDocument()
       This function is called from the C# code to create a new PDF document. It creates a new PDF document
       with the size of 'LETTER' and a sets a flag to flush output to the pages. It then pipes the document
       to a blob stream.
       Parameters: None
    */
    createDocument: function () {
        // Create a new PDF document
        doc = new PDFDocument({ size: 'LETTER', bufferPages: true });
        stream = doc.pipe(blobStream());
    },

    // Funtion that adds a new page to the PDF document, same as a page break
    addPage: function () {
        doc.addPage();
    },

    /*
       Fuction: addContent(String content, String fontSize, String font, String alignment, String colour)
       This function is called from the C# code to add a new page to the PDF document. It adds content to the
       PDF document with the content and format specified.
       Parameters: (String) content - the content to add to the page.
                   (String) fontSize - the font size of the content.
                   (String) font - the font of the content.
                   (String) alignment - the alignment of the content.
                   (String) colour - the font colour of the content.
    */

    // Function to add content to the PDF document
    // Content (string) - text that will be added
    // ftSize (float) - the font size
    // fontName (string) - Name of the font
    // alignemnt (string) - position of the text (justify, center, left, right)
    // colour (string) - colour of the text, can use basic colours or HEX
    addContent: function (content, ftSize, fontName, alignment, colour) {

        const contentStr = UTF8ToString(content);// Remember to covert the content to a string
        const fontStr = UTF8ToString(fontName);
        const alignmentStr = UTF8ToString(alignment);
        const colourStr = UTF8ToString(colour);

        doc.fontSize(ftSize)
            .font(fontStr)
            .fillColor(colourStr)
            .text(contentStr, {
                align: alignmentStr
            })
            .moveDown();
    },
    /*
       Fcuntion: addHeader(String dateTime, String logoPlaceholderText)
       This function is called from the C# code to add date and time to the header of the PDF document. It also adds
       a logo to the header of the PDF document.
       Parameters: (String) dateTime - the date and time to add to the header.
                   (String) base64ImageData - the base54 data for the logo.
                   (String) font - the font for the date and time.
                   (String) fontColor - the font color for the date and time.
                   (int) fontSize - the font size for the date and time.
                   (String) horizontalMargin - the horizontal margin for the date and time.
    */
    addHeader: function (dateTime, base64ImageData, font, fontColor, fontSize, horizontalMargin) {
        const dateStr = UTF8ToString(dateTime);
        const imageStr = UTF8ToString(base64ImageData);
        const fontStr = UTF8ToString(font);
        const fontColorStr = UTF8ToString(fontColor);
        const logoVerticalOffset = 10;

        // Convert base64 string directly into a Data URL format
        const logoDataUrl = `data:image/png;base64,${imageStr}`;

        let pages = doc.bufferedPageRange();
        for (let i = pages.start; i < pages.count; i++) {
            doc.switchToPage(i);

            // Set the font configuration for the date and time
            doc.fontSize(fontSize)
                .font(fontStr)
                .fillColor(fontColorStr);

            // For each page, add the date/time on the left and the logo on the right
            let oldTopMargin = doc.page.margins.top;
            doc.page.margins.top = 0;

            // Date on the left side
            doc.text(dateStr, horizontalMargin, (oldTopMargin / 2), { align: 'left' });

            // Calculate position for the logo on the right side
            let pageWidth = doc.page.width;
            let logoWidth = 82.20; // Logo width in points 
            let logoHeight = 27.50; // Logo height in points 
            let logoX = pageWidth - logoWidth - horizontalMargin; // Right-aligned with a horizontal margin

            // Add the logo image to the header with a 10-point margin offset from the top
            doc.image(logoDataUrl, logoX, (oldTopMargin / 2 - logoVerticalOffset), { width: logoWidth, height: logoHeight });
            // Restore the top margin
            doc.page.margins.top = oldTopMargin;
        }
    },
    /*
       Function: addFooter(String footerText)
       This function is called from the C# code to add a footer to the PDF document. It adds the footer text to
       the bottom of each page of the PDF document along with the page number for each page.
       Parameters: (String) footerText - the text to add to the footer.
                   (String) leftFont - the font for the left side of the footer.
                   (int) leftFontSize - the font size for the left side of the footer.
                   (String) leftFontColor - the font color for the left side of the footer.
                   (String) rightFont - the font for the right side of the footer.
                   (int) rightFontSize - the font size for the right side of the footer.
                   (String) rightFontColor - the font color for the right side of the footer.
                   (int) horizontalMargin - the horizontal margin for the footer.
    */
    addFooter: function (footerText, leftFont, leftFontSize, leftFontColor, rightFont, rightFontSize, rightFontColor, horizontalMargin) {
        const footerStr = UTF8ToString(footerText);
        const leftFontStr = UTF8ToString(leftFont);
        const leftFontColorStr = UTF8ToString(leftFontColor);
        const rightFontStr = UTF8ToString(rightFont);
        const rightFontColorStr = UTF8ToString(rightFontColor);

        let pages = doc.bufferedPageRange();
        for (let i = pages.start; i < pages.count; i++) {
            doc.switchToPage(i);

            // Set the left side font configuration
            doc.fontSize(leftFontSize)
                .font(leftFontStr)
                .fillColor(leftFontColorStr);

            // For each page, add the page number and the footer text
            let oldBottomMargin = doc.page.margins.bottom;
            doc.page.margins.bottom = 0;
            doc.text(footerStr, horizontalMargin, (doc.page.height - oldBottomMargin / 2), { align: 'left' });

            // Set the right side text font configuration
            doc.fontSize(rightFontSize)
                .font(rightFontStr)
                .fillColor(rightFontColorStr);
            // Add the page number to the right side of the footer
            doc.text(`Page ${i + 1} of ${pages.count}`, 0, (doc.page.height - oldBottomMargin / 2), { align: 'right' });

            // Restore the bottom margin
            doc.page.margins.bottom = oldBottomMargin;
        }
    },

    // Function to draw a line
    // lineStyle (string) - style of line cap (butt, round,square)
    // startX, startY (int) - x,y position of the first point
    // endX, endY (int) - x,y position of the second point
    // lineColour (string) - colour of the line that will be drawn from point A to point B, can use basic colours or HEX
    addLine: function (lineStyle, startX, startY, endX, endY, lineColour) {

        const lineStyleStr = UTF8ToString(lineStyle);
        const lineColourStr = UTF8ToString(lineColour);

        doc.lineCap(lineStyleStr)
            .moveTo(startX, startY)
            .lineTo(endX, endY)
            .fillAndStroke(lineColourStr);
    },

    // Function to add content to the PDF document in a specified position in the page
    // Content (string) - text that will be added
    // ftSize (float) - the font size
    // fontName (string) - Name of the font
    // alignemnt (string) - position of the text (justify, center, left, right)
    // colour (string) - colour of the text, can use basic colours or HEX
    // posX, posY (int) - x,y position of the start point of the text
    // moveDown (float) - number of lines to move down after the content
    addContentWithXandYPosition: function (content, ftSize, fontName, alignment, colour, posX, posY, moveDown) {

        const contentStr = UTF8ToString(content);// Remember to covert the content to a string
        const fontStr = UTF8ToString(fontName);
        const alignmentStr = UTF8ToString(alignment);
        const colourStr = UTF8ToString(colour);

        doc.fontSize(ftSize)
            .font(fontStr)
            .fillColor(colourStr)
            .text(contentStr, posX, posY, {
                align: alignmentStr
            })
            .moveDown(moveDown)
    },

    // Function to add underlined text to the PDF document
    // Content (string) - text that will be added
    // ftSize (float) - the font size
    // fontName (string) - Name of the font
    // moveLineDown (float) - lines to move down after the underlined text
    addTextWithUnderline: function (content, fontName, ftSize, moveLineDown) {

        const contentStr = UTF8ToString(content);
        const fontStr = UTF8ToString(fontName);

        doc.font(fontStr)
            .fontSize(ftSize)
            .text(contentStr,
                { underline: doc.widthOfString(contentStr) })
            .moveDown(moveLineDown);
    },

    // Funtion to move lines down
    // linesDowm (float) - number of lines to move down
    moveLinesDown: function (linesDown) {

        doc.moveDown(linesDown);
    },

    // Function that allows content to have 2 different fonts and/or two different colours
    // content (string) - text that will be added
    // sliceCount (int) - number of characters in the first part
    // sliceMax (int) - total number of characters in content
    // fontOne, colourOne (string) - font and colour that will be applied to all characters up to the slice count
    // fontTwo, colourTwo (string) - font and colour that will be applied to the rest of the characters.
    // moveLineDown (float) - lines to move down after the underlined text
    // This can be used make the first part of the content bold
    addSlicedStringFontAndColour: function (content, sliceCount, sliceMax, fontOne, fontTwo, colourOne, colourTwo, moveLineDown) {

        const contentStr = UTF8ToString(content);
        const fontOneStr = UTF8ToString(fontOne);
        const fontTwoStr = UTF8ToString(fontTwo);
        const colourOneStr = UTF8ToString(colourOne);
        const colourTwoStr = UTF8ToString(colourTwo);

        doc.font(fontOneStr)
            .fillColor(colourOneStr)
            .text(contentStr.slice(0, sliceCount), {
                continued: true
            })
            .font(fontTwoStr)
            .fillColor(colourTwoStr)
            .text(contentStr.slice(sliceCount, sliceMax), {
                continued: false
            })
            .moveDown(moveLineDown);
    },

    // Function that add individual lines to a numbered list.
    // Set to work on Page size 'LETTER' with top and bottom margins set to 72
    // This function is called multiple times from the C# script
    addNumberedList: function (number, content) {

        const contentStr = UTF8ToString(content);
        const indentNumber = 80;
        const indentLog = 100;
        const pageLimit = 700;
        const newPageStartPosition = 85;
        let currentPosition = doc.y;
        let range = doc.bufferedPageRange();
        let count = range.count;

        if (doc.y <= pageLimit) {
            doc.text(`${number}.`, indentNumber, currentPosition);
            doc.text(contentStr, indentLog, currentPosition)
                .moveDown(0.8);
        } else {
            doc.addPage()
            currentPosition = newPageStartPosition;
            doc.text(`${number}.`, indentNumber, currentPosition);
            doc.text(contentStr, indentLog, currentPosition)
                .moveDown(0.8);
        }
    },

    // Sets the font style and size
    setFont: function (fontName, fontSize) {

        const fontNameStr = UTF8ToString(fontName);

        doc.font(fontNameStr)
            .fontSize(fontSize);
    },

    // Function to get the line height.
    getLineHeight: function () {
        return doc.currentLineHeight();
    }
}); 