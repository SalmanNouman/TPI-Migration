using System.Text.RegularExpressions;

namespace VARLab.DLX
{
    /// <summary>
    ///     This static class is used to manage the Points of Interest (POI) List. 
    /// </summary>
    public static class PoiList
    {
        #region Fields

        /// <summary>
        ///     Enum of the POI names.
        ///     To add or modify POIs, update this enum.
        /// </summary>
        public enum PoiName
        {
            None = 0,
            Reception = 1,
            Bathroom = 2,
            PiercingArea = 3,
            TattooArea = 4,
            Office = 5,
        }

        #endregion

        #region Methods

        /// <summary>
        ///     This method converts the POI name to a readable format.
        /// </summary>
        /// <param name="name">The name of the POI to format.</param>
        /// <returns>The formatted string of the POI name.</returns>
        public static string GetPoiName(string name)
        {
            // Add a space and '#' in front of numeric values (e.g., "Bathroom2" -> "Bathroom #2")
            name = Regex.Replace(name, @"\d+", " #$0");
            // Add a space before uppercase letters that are not at the beginning of the string (e.g., "PiercingArea" -> "Piercing Area")
            name = Regex.Replace(name, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            return name;
        }

        #endregion
    }
}