namespace VARLab.DLX
{
    /// <summary>
    ///     Represents an inspection record for an inspectable object, 
    ///     storing its compliance status and whether a photo was taken.
    /// </summary>
    public class InspectionData
    {
        //Properties
        public InspectableObject Obj { get; set; }
        public bool IsCompliant { get; set; }
        public bool HasPhoto { get; set; }

        //Constructors
        public InspectionData() { }
        public InspectionData(InspectableObject obj, bool isCompliant, bool hasPhoto)
        {
            this.Obj = obj;
            this.IsCompliant = isCompliant;
            this.HasPhoto = hasPhoto;
        }
    }
}
