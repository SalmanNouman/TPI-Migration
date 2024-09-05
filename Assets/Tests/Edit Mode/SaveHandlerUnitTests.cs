using NUnit.Framework;
using UnityEngine;
using VARLab.DLX;

namespace Tests.EditMode
{

    /// <summary>
    ///     This class should test the <see cref="CustomSaveHandler"/>
    /// </summary>
    public class SaveHandlerUnitTests
    {
        private const string TestUsername = "TestUsername";

        // 
        private CustomSaveHandler saveHandler;

        /// <summary>
        ///     Validates that once the SaveHandler has received a username 
        ///     from a successful login, the 'Blob' (save file) name contains the username
        /// </summary>
        [Test]
        public void SaveHandler_HandleLogin_ShouldUpdateBlobName()
        {
            saveHandler = new GameObject().AddComponent<CustomSaveHandler>();

            // Arrange
            string nameExpected = TestUsername;

            // Act
            saveHandler.HandleLogin(TestUsername);

            // Assert
            Assert.That(saveHandler.Blob.Contains(nameExpected));
        }
    }
}
