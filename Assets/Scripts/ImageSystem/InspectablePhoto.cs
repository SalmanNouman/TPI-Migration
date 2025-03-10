namespace VARLab.DLX
{
    /// <summary>
    /// This is a class to represent a taken image/screen capture when the learner wants to take a photo of an object.
    /// It will contain any necessary information for the image itself and any methods tied to images.
    /// </summary>
    public class InspectablePhoto
    {
        // Properties
        public byte[] Data;
        public string Id;
        public string Location;
        public string Timestamp;

        /// <summary>
        /// Create a constructor of the Inspectable photo class with specified image details.
        /// </summary>
        /// <param name="data">Data of the captured image</param>
        /// <param name="id">Id associated with the photo</param>
        /// <param name="location">Location where the photo was taken</param>
        /// <param name="timestamp">The time when the photo was taken</param>
        public InspectablePhoto(byte[] data, string id, string location, string timestamp)
        {
            Data = data;
            Id = id;
            Location = location;
            Timestamp = timestamp;
        }

        public string ParseNameFromID(string id) 
        {
            string[] split = id.Split('_');
            if (split.Length == 1)
            {
                return split[0];
            }
            else
            {
                return split[1];
            }
        }
    }
}

