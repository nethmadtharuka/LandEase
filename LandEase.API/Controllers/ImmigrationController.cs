using LandEase.API.Models;
using LandEase.Application.DTOs.Immigration;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImmigrationController : ControllerBase
{
    private readonly IImmigrationPredictorService _predictor;

    public ImmigrationController(IImmigrationPredictorService predictor)
        => _predictor = predictor;

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Analyze(
        [FromForm] ProfileSubmitDto dto,
        [FromForm] List<IFormFile>? documents)
    {
        try
        {
            // Convert uploaded files to byte arrays
            var uploadedDocs = new List<(byte[] fileBytes, string docType)>();

            if (documents != null)
            {
                foreach (var file in documents)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    // File must be named like: Passport_test.pdf
                    // Split on underscore to get doc type
                    var docType = file.FileName.Split('_')[0];
                    uploadedDocs.Add((ms.ToArray(), docType));
                }
            }

            var result = await _predictor.AnalyzeProfileAsync(dto, uploadedDocs.Any() ? uploadedDocs : null);
            return Ok(ApiResponse<ScoreResultDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("visa-types")]
    [AllowAnonymous]
    public IActionResult GetVisaTypes()
    {
        var types = _predictor.GetSupportedVisaTypes();
        return Ok(ApiResponse<List<string>>.Ok(types));
    }

    [HttpGet("countries")]
    [AllowAnonymous]
    public IActionResult GetCountries()
    {
        var countries = _predictor.GetSupportedCountries();
        return Ok(ApiResponse<List<string>>.Ok(countries));
    }
}