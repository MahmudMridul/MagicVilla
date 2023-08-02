using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        private APIResponse _apiResponse;

        public VillaAPIController(ILogger<VillaAPIController> logger, IVillaRepository dbVilla, IMapper mapper)
        {
            _logger = logger;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _apiResponse = new APIResponse();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            _logger.LogInformation("GET all villas");
            IEnumerable<Villa> villas = await _dbVilla.GetAllAsync();
            _apiResponse.Result = _mapper.Map<IEnumerable<VillaDTO>>(villas);
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);   
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            if (id == 0) 
            {
                _logger.LogError("Id is 0");
                return BadRequest(); 
            }

            Villa villa = await _dbVilla.GetAsync(villa => villa.Id == id);

            if(villa == null) return NotFound();
            _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody]VillaCreateDTO createDTO)
        {
            if(createDTO == null) return BadRequest(createDTO);

            bool nameExists = await _dbVilla.GetAsync(villa => villa.Name.ToLower() == createDTO.Name.ToLower()) != null;

            if(nameExists)
            {
                ModelState.AddModelError("DuplicateNameError", "Villa name already exists");
                return BadRequest(ModelState);
            }

            Villa model = _mapper.Map<Villa>(createDTO);
            await _dbVilla.CreateAsync(model);

            _apiResponse.Result = model;
            _apiResponse.StatusCode = HttpStatusCode.Created;

            return CreatedAtRoute("GetVilla", new { id = model.Id }, _apiResponse);
        }

        //If return type is IActionResult, no need to specify the 
        //the return type explicitly
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            if (id == 0) return BadRequest();

            Villa villa = await _dbVilla.GetAsync(villa => villa.Id == id);

            if (villa == null) return NotFound();

            await _dbVilla.RemoveAsync(villa);

            _apiResponse.StatusCode= HttpStatusCode.NoContent;
            _apiResponse.IsSuccess = true;

            return Ok(_apiResponse);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            if(updateDTO == null || id != updateDTO.Id) return BadRequest();

            Villa model = _mapper.Map<Villa>(updateDTO);
            await _dbVilla.UpdateAsync(model);

            _apiResponse.StatusCode = HttpStatusCode.NoContent;
            _apiResponse.IsSuccess = true;

            return Ok(_apiResponse);
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO) 
        {
            if (patchDTO == null || id == 0) return BadRequest();

            Villa villa = await _dbVilla.GetAsync(villa => id == villa.Id, false);

            if (villa == null) return NotFound(patchDTO);

            VillaUpdateDTO updateDTO = _mapper.Map<VillaUpdateDTO>(villa);

            patchDTO.ApplyTo(updateDTO, ModelState);

            Villa model = _mapper.Map<Villa>(updateDTO);

            await _dbVilla.UpdateAsync(model);

            if (!ModelState.IsValid) { return BadRequest(); }

            _apiResponse.StatusCode = HttpStatusCode.NoContent;
            _apiResponse.IsSuccess = true;

            return Ok(_apiResponse);
        }
    }
}
