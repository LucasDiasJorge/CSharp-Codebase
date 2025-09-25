using System;
using FluentValidationUserApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentValidationUserApi.Controllers
{
    /// <summary>
    /// Exposes endpoints for working with users and demonstrates FluentValidation rules.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Creates a new user after running FluentValidation rules.
        /// </summary>
        /// <param name="user">The user payload that will be validated and created.</param>
        /// <returns>A representation of the created user.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<UserResponse> Create([FromBody] User user)
        {
            UserResponse response = new UserResponse(user.Name, user.Email, user.Age, user.DateOfBirth);
            return CreatedAtAction(nameof(GetByEmail), new { email = response.Email }, response);
        }

        /// <summary>
        /// Returns a sample user. This endpoint exists to provide a <see cref="CreatedAtActionResult"/> target.
        /// </summary>
        /// <param name="email">An email used to compose a sample user.</param>
        /// <returns>A sample <see cref="UserResponse"/> instance.</returns>
        [HttpGet("{email}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        public ActionResult<UserResponse> GetByEmail(string email)
        {
            DateTime sampleDateOfBirth = DateTime.Today.AddYears(-30);
            UserResponse response = new UserResponse("Sample User", email, 30, sampleDateOfBirth);
            return Ok(response);
        }
    }
}
