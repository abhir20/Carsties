using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Model)
                .ToListAsync();

            return _mapper.Map<List<AuctionDTO>>(auctions);
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            return _mapper.Map<AuctionDTO>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO createAuctionDTO)
        {
            var auction = _mapper.Map<Auction>(createAuctionDTO);
            //TODO: add current user as seller

            auction.Seller = "test";
            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not save changes to DB");
            }

            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDTO>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
        {
            var auction = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            //TODO: check seller = username

            auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
            auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
            auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
            auction.Item.Mileage = updateAuctionDTO.Mileage;

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest("Could not save changes to DB");


        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null)
            {
                return NotFound();
            }

            //TODO: check seller = username

            _context.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to DB");

            return Ok();
        }

    }
}