using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheBoys.Helpers;
using TheBoys.Services;

namespace TheBoys.Controllers
{
    /// <summary>
    /// The FileController class provides API endpoints for uploading and deleting files.
    /// </summary>
    [Route("api/file")]
    [Authorize]
    public class FileController : BaseApiController
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Initializes a new instance of the FileController class.
        /// </summary>
        /// <param name="fileService">fileService value to set.</param>
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Saves file
        /// </summary>
        /// <param name="entityName">Name of the target entity</param>
        /// <param name="fieldName">Name of the target field</param>
        /// <param name="files">List of files to be saved</param>
        /// <returns>Returns list of file path of saved files.</returns>
        [HttpPost]
        [Route("{entityName}/field/{fieldName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SaveFiles(string entityName, string fieldName, List<IFormFile> files)
        {
            var response = await _fileService.SaveFiles(TenantId, entityName, fieldName, files);
            return Ok(response);
        }

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="fileNames">List of file path to be deleted</param>
        /// <returns>Returns list of boolean status of deleted files.</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteFiles([FromForm] List<string> fileNames)
        {
            var response = await _fileService.DeleteFiles(fileNames);
            return Ok(response);
        }
    }
}