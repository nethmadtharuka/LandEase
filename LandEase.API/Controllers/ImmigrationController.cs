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
    public async Task<IActionResult> Analyze([FromBody] ProfileSubmitDto dto)
    {
        try
        {
            var result = await _predictor.AnalyzeProfileAsync(dto);
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
