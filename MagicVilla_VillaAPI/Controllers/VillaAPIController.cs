﻿using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            return Ok(VillaStore.villaList);   
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if(id == 0) return BadRequest();

            VillaDTO villa = VillaStore.villaList.FirstOrDefault(villa => villa.Id == id);

            if(villa == null) return NotFound();
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            if(villaDTO == null) return BadRequest(villaDTO);
            if(villaDTO.Id > 0) return StatusCode(StatusCodes.Status500InternalServerError);

            bool nameExists = VillaStore.villaList.FirstOrDefault(villa => villa.Name.ToLower() == villaDTO.Name.ToLower()) != null;

            if(nameExists)
            {
                ModelState.AddModelError("DuplicateNameError", "Villa name already exists");
                return BadRequest(ModelState);
            }

            villaDTO.Id = VillaStore.villaList.OrderByDescending(villa => villa.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDTO);

            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id },villaDTO);
        }

        //If return type is IActionResult, no need to specify the 
        //the return type explicitly
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0) return BadRequest();

            VillaDTO villa = VillaStore.villaList.FirstOrDefault(villa => villa.Id == id);

            if (villa == null) return NotFound();

            VillaStore.villaList.Remove(villa);
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if(villaDTO == null || id != villaDTO.Id) return BadRequest();

            VillaDTO villa = VillaStore.villaList.FirstOrDefault(villa => id == villa.Id);

            if (villa == null) return NotFound(villaDTO);

            villa.Name = villaDTO.Name;
            villa.Occupancy = villaDTO.Occupancy;
            villa.Sqft = villaDTO.Sqft;
            return NoContent();
        }

    }
}
