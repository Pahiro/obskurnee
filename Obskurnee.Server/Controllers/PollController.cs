﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Obskurnee.Models;
using Obskurnee.Services;
using Obskurnee.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Design;

namespace Obskurnee.Controllers;

[Authorize]
[ApiController]
[Route("api/polls")]
public class PollController(
    ILogger<PollController> logger,
    PollService polls,
    RoundManagerService roundManager,
    UserServiceBase users) : Controller
{
    private readonly ILogger<PollController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PollService _polls = polls ?? throw new ArgumentNullException(nameof(polls));
    private readonly RoundManagerService _roundManager = roundManager ?? throw new ArgumentNullException(nameof(roundManager));
    private readonly UserServiceBase _users = users ?? throw new ArgumentNullException(nameof(users));

    [HttpGet]
    public async Task<IEnumerable<Poll>> GetPolls() => await _polls.GetAll();

    [HttpGet]
    [Route("{pollId:int}")]
    public async Task<PollInfo> GetPoll(int pollId) => await _polls.GetPollInfo(pollId, User.GetUserId());

    [HttpPost]
    [Route("{pollId:int}/vote")]
    [Authorize(Policy = "CanUpdate")]
    public async Task<RoundUpdateResults> CastVote(int pollId, VotePayload votePayload)
    {
        var vote = new Vote(User.GetUserId()) 
            {
            PostIds = votePayload.PostIds,
            PollId = pollId
        };
        var poll = await _polls.CastPollVote(vote);
        _logger.LogInformation("User {userId} voted in poll {pollId}", User.GetUserId(), pollId);
        if (poll.Results.AlreadyVoted.Count == _users.GetAllActiveUserCount())
        {
            return await _roundManager.ClosePoll(pollId, User.GetUserId());
        }
        return new RoundUpdateResults { Poll = poll };
    }

    public class VotePayload
    {
        public int[] PostIds { get; set; }
    }

    [HttpDelete("{pollId:int}")]
    [Authorize(Policy = "CanUpdate")]
    public async Task<IActionResult> DeletePoll(int pollId)
    {
        var success = await _polls.DeletePoll(pollId);
        if (!success)
        {
            return NotFound();
        }

        _logger.LogInformation("Poll {pollId} deleted by user {userId}", pollId, User.GetUserId());
        return NoContent(); // 204 No Content for a successful delete
    }

}
