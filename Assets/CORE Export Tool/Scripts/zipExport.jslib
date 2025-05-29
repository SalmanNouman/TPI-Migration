mergeInto(LibraryManager.library, {
    /// <summary>
    /// Function that initializes a new JSZip instance and stores it in a global dictionary.
    /// </summary>
    /// <param name="zipIdPtr">Pointer to the string that represents the ID of the zip instance.</param>
    /// <returns>void</returns>
  InitZip: function (zipIdPtr) {
    var zipId = UTF8ToString(zipIdPtr);
    // Create a global dictionary to store JSZip instances
    if (!window.zipInstances) {
      window.zipInstances = {};
    }

    // Initialize a new JSZip instance
    window.zipInstances[zipId] = new JSZip();
  },
    /// <summary>
    /// Function that adds a file to a JSZip instance by using the zipId to retrieve the instance from the global dictionary.
    /// It also decodes the base64 data to binary before adding it to the zip.
    /// </summary>
    /// <param name="zipIdPtr">Pointer to the string that represents the ID of the zip instance.</param>
    /// <param name="fileNamePtr">Pointer to the string that represents the name of the file to be added to the zip.</param>
    /// <param name="base64DataPtr">Pointer to the string that represents the base64 data of the file to be added to the zip.</param>
    /// <returns>void</returns>
  AddFileToZip: function (zipIdPtr, fileNamePtr, base64DataPtr) {
    var zipId = UTF8ToString(zipIdPtr);
    var fileName = UTF8ToString(fileNamePtr);
    var base64Data = UTF8ToString(base64DataPtr);

    if (!window.zipInstances || !window.zipInstances[zipId]) {
      console.error("Zip instance for " + zipId + " is not initialized.");
      return;
    }

    // Decode base64 data to binary
    var binaryData = Uint8Array.from(atob(base64Data), c => c.charCodeAt(0));

    // Add the file to the zip
    window.zipInstances[zipId].file(fileName, binaryData);
  },
    /// <summary>
    /// Function that synchronously generates a zip file from a JSZip instance and triggers the download of the zip file.
    /// It also removes the JSZip instance from the global dictionary and performs the necessary cleanup.
    /// </summary>
    /// <param name="zipIdPtr">Pointer to the string that represents the ID of the zip instance.</param>
    /// <param name="zipFileNamePtr">Pointer to the string that represents the name of the zip file to be downloaded.</param>
  DownloadZip: function (zipIdPtr, zipFileNamePtr) {
    var zipId = UTF8ToString(zipIdPtr);
    var zipFileName = UTF8ToString(zipFileNamePtr);

    if (!window.zipInstances || !window.zipInstances[zipId]) {
      console.error("Zip instance for " + zipId + " is not initialized.");
      return;
    }

    // Generate the zip file synchronously
    var zipContent = window.zipInstances[zipId].generate({ type: "blob" });

    // Create a URL for the Blob
    var url = URL.createObjectURL(zipContent);

    // Create an anchor element and trigger the download
    var link = document.createElement('a');
    link.href = url;
    link.download = zipFileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    // Clean up
    delete window.zipInstances[zipId];
  }
});
