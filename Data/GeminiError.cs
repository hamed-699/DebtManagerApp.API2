namespace DebtManagerApp.Data
{
    /// <summary>
    /// Represents an error entry to be stored in the database, potentially from the Gemini API.
    /// This class defines the properties that will be stored in the local database.
    /// </summary>
    public class GeminiError
    {
        /// <summary>
        /// The Primary Key for the GeminiError entity.
        /// This is required by Entity Framework Core to uniquely identify each error record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The HTTP error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// The detailed error message from the API.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Message { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// The status of the error (e.g., "INVALID_ARGUMENT").
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Status { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
