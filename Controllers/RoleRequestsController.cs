
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleRequestsController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public RoleRequestsController(EmployeeDbContext context)
        {
            _context = context;
        }

        // Employee submits a request for Admin access
        [HttpPost]
        public async Task<IActionResult> CreateRequest(
            [FromBody] CreateRoleRequestDto dto)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                return Unauthorized("User not found.");
            }

            var existingRequest = await _context.RoleRequests
                .AnyAsync(r => r.UserId == userId &&
                               r.Status == "Pending");

            if (existingRequest)
            {
                return BadRequest(
                    "You already have a pending role request.");
            }

            var request = new RoleRequest
            {
                UserId = userId,
                Reason = dto.Reason,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.RoleRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(CreateRequest),
                new { id = request.Id },
                new
                {
                    request.Id,
                    request.Status,
                    request.RequestedAt,
                    Message = "Role request submitted successfully."
                });
        }

        // Employee views their own role requests
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var requests = await _context.RoleRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Reason,
                    r.Status,
                    r.RequestedAt,
                    r.ReviewedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // Admin views all pending role requests
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _context.RoleRequests
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.Reason,
                    r.Status,
                    r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.RoleRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound("Role request not found.");
            }

            if (request.Status != "Pending")
            {
                return BadRequest(
                    "Only pending requests can be approved.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Role = "Admin";
            request.Status = "Approved";
            request.ReviewedAt = DateTime.UtcNow;

            var reviewerIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(reviewerIdValue, out var reviewerId))
            {
                request.ReviewedByUserId = reviewerId;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Role request approved successfully.",
                RequestId = request.Id,
                NewRole = user.Role,
                Status = request.Status
            });
        }

    }
}
