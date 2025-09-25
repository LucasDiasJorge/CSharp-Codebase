using System;

namespace FluentValidationUserApi.Models
{
    /// <summary>
    /// Represents the data required to register a user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email address used for communication.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the age of the user. The value must be consistent with <see cref="DateOfBirth"/>.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the date of birth of the user.
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// Properties are initialized to safe defaults to improve the developer experience.
        /// </summary>
        public User()
        {
            Name = string.Empty;
            Email = string.Empty;
            Age = 0;
            DateOfBirth = DateTime.MinValue;
        }
    }
}
