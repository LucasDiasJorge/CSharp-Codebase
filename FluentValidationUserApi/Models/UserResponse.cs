using System;

namespace FluentValidationUserApi.Models
{
    /// <summary>
    /// Represents the data returned by the API after a user is created.
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserResponse"/> class.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="email">The email of the user.</param>
        /// <param name="age">The validated age of the user.</param>
        /// <param name="dateOfBirth">The validated date of birth of the user.</param>
        public UserResponse(string name, string email, int age, DateTime dateOfBirth)
        {
            Name = name;
            Email = email;
            Age = age;
            DateOfBirth = dateOfBirth;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the age of the user.
        /// </summary>
        public int Age { get; }

        /// <summary>
        /// Gets the date of birth of the user.
        /// </summary>
        public DateTime DateOfBirth { get; }
    }
}
