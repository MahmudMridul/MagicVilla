using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")] // Required when you have multiple versions
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        private APIResponse _apiResponse;

        public VillaNumberAPIController(ILogger<VillaAPIController> logger, IVillaNumberRepository dbVillaNumber, IVillaRepository dbVilla, IMapper mapper)
        {
            _logger = logger;
            _dbVillaNumber = dbVillaNumber;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _apiResponse = new APIResponse();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                _logger.LogInformation("GET all villas");
                IEnumerable<VillaNumber> villaNumbers = await _dbVillaNumber.GetAllAsync();
                _apiResponse.Result = _mapper.Map<IEnumerable<VillaNumberDTO>>(villaNumbers);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
            
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<APIResponse>> GetAllVillaNumbers()
        {
            try
            {
                _logger.LogInformation("GET all villas");
                IEnumerable<VillaNumber> villaNumbers = await _dbVillaNumber.GetAllAsync();
                _apiResponse.Result = _mapper.Map<IEnumerable<VillaNumberDTO>>(villaNumbers);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }

        }

        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Id is 0");
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }
                VillaNumber villaNumber = await _dbVillaNumber.GetAsync(v => v.VillaNo == id);
                if (villaNumber == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                if (createDTO == null) 
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse); 
                }

                bool numExists = await _dbVillaNumber.GetAsync(v => v.VillaNo == createDTO.VillaNo) != null;

                if (numExists)
                {
                    ModelState.AddModelError("DuplicateNumberError", "Villa number already exists");
                    _apiResponse.Result = ModelState;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                if(await _dbVilla.GetAsync(villa => villa.Id == createDTO.VillaId) == null)
                {
                    ModelState.AddModelError("InvalidVillaIDError", "Villa Id is invalid");
                    _apiResponse.Result = ModelState;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return _apiResponse;
                }

                VillaNumber model = _mapper.Map<VillaNumber>(createDTO);
                model.CreatedDate = DateTime.Now;
                await _dbVillaNumber.CreateAsync(model);
                _apiResponse.Result = model;
                _apiResponse.StatusCode = HttpStatusCode.Created;
                _apiResponse.IsSuccess = true;
                return CreatedAtRoute("GetVilla", new { id = model.VillaNo }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
            
        }

        //If return type is IActionResult, no need to specify the 
        //the return type explicitly
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                VillaNumber villa = await _dbVillaNumber.GetAsync(v => v.VillaNo == id);
                if (villa == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                await _dbVillaNumber.RemoveAsync(villa);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }
                if (await _dbVilla.GetAsync(villa => villa.Id == updateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("InvalidVillaIDError", "Villa Id is invalid");
                    _apiResponse.Result = ModelState;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return _apiResponse;
                }

                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);
                await _dbVillaNumber.UpdateAsync(model);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO) 
        {
            try
            {
                if (patchDTO == null || id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                VillaNumber villa = await _dbVillaNumber.GetAsync(v => id == v.VillaNo, false);
                if (villa == null) return NotFound(patchDTO);

                VillaNumberUpdateDTO updateDTO = _mapper.Map<VillaNumberUpdateDTO>(villa);
                patchDTO.ApplyTo(updateDTO, ModelState);
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);
                await _dbVillaNumber.UpdateAsync(model);
                if (!ModelState.IsValid)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
                return _apiResponse;
            }
        }
    }
}
