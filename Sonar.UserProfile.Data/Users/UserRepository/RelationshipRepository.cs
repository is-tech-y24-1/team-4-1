﻿using Microsoft.EntityFrameworkCore;
using Sonar.UserProfile.Core.Domain.Exceptions;
using Sonar.UserProfile.Core.Domain.Users;
using Sonar.UserProfile.Core.Domain.Users.Repositories;
using Sonar.UserProfile.Data.Users.ValueObjects;

namespace Sonar.UserProfile.Data.Users.UserRepository;

public class RelationshipRepository : IRelationshipRepository
{
    private readonly SonarContext _context;

    public RelationshipRepository(SonarContext context)
    {
        _context = context;
    }

    public async Task SendFriendshipRequestAsync(Guid userId, Guid friendId, CancellationToken cancellationToken)
    {
        var entityUser =
            await _context.Users.FirstOrDefaultAsync(it => it.Id == userId, cancellationToken);
        var entityFriend =
            await _context.Users.FirstOrDefaultAsync(it => it.Id == friendId, cancellationToken);

        if (entityUser is null)
        {
            throw new UserNotFoundException($"User with id = {userId} does not exists");
        }

        if (entityFriend is null)
        {
            throw new UserNotFoundException($"User with id = {friendId} does not exists");
        }

        _context.Relationships.Add(new RelationshipDbModel
        {
            UserId = userId,
            FriendId = friendId,
            RelationshipStatus = RelationshipStatus.Requested
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<User>> GetFriendsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetRelationshipListByIdAsync(id, RelationshipStatus.Friended, cancellationToken);
    }

    public Task<IReadOnlyList<User>> GetRequestedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetRelationshipListByIdAsync(id, RelationshipStatus.Requested, cancellationToken);
    }

    public Task<IReadOnlyList<User>> GetRejectedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetRelationshipListByIdAsync(id, RelationshipStatus.Rejected, cancellationToken);
    }

    public Task<bool> IsFriendsAsync(Guid userId, Guid friendId, CancellationToken cancellationToken)
    {
        return IsRelationshipAsync(userId, friendId, RelationshipStatus.Friended, cancellationToken);
    }

    public Task<bool> IsRequestedAsync(Guid userId, Guid friendId, CancellationToken cancellationToken)
    {
        return IsRelationshipAsync(userId, friendId, RelationshipStatus.Requested, cancellationToken);
    }

    public Task<bool> IsRejectedAsync(Guid userId, Guid friendId, CancellationToken cancellationToken)
    {
        return IsRelationshipAsync(userId, friendId, RelationshipStatus.Rejected, cancellationToken);
    }

    private async Task<IReadOnlyList<User>> GetRelationshipListByIdAsync(
        Guid id,
        RelationshipStatus relationshipStatus,
        CancellationToken cancellationToken)
    {
        var relationshipList = await _context.Relationships
            .Where(r => r.UserId == id && r.RelationshipStatus == relationshipStatus)
            .Select(r => new User
            {
                Id = r.FriendId,
                Email = _context.Users.FirstOrDefault(f => f.Id == r.FriendId).Email
            }).ToListAsync(cancellationToken);

        relationshipList.AddRange(await _context.Relationships
            .Where(r => r.FriendId == id && r.RelationshipStatus == relationshipStatus)
            .Select(r => new User
            {
                Id = r.UserId,
                Email = _context.Users.FirstOrDefault(f => f.Id == r.UserId).Email
            }).ToListAsync(cancellationToken));

        return relationshipList;
    }

    private Task<bool> IsRelationshipAsync(
        Guid userId,
        Guid friendId,
        RelationshipStatus relationshipStatus,
        CancellationToken cancellationToken)
    {
        return _context.Relationships.AnyAsync(r =>
            (r.UserId == userId && r.FriendId == friendId || r.UserId == friendId && r.FriendId == userId) &&
            r.RelationshipStatus == relationshipStatus, cancellationToken);
    }
}