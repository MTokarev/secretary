using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Extensions;
using Secretary.Interfaces;
using Secretary.Models;

namespace Secretary.ApiEndpoints;

/// <summary>
/// Class that contains extension method to register endpoints
/// </summary>
public static class SecretsEndpoints
{
    public static void MapSecretEndpoints(this IEndpointRouteBuilder app)
    {
        // Gets secret by id
        app.MapGet("/secrets/{id:guid}",
                async ([FromRoute] Guid id, [FromHeader] string? accessPassword, ISecretService secretService) =>
                {
                    var secretFromDb = await secretService.GetSecretAsync(s => s.Id == id);

                    var validatedResult = await secretService.ValidateSecretAsync(secretFromDb, accessPassword);

                    if (validatedResult.ValidationResult == SecretValidationResult.SuccessfullyValidated)
                    {
                        await secretService.ProcessAccessedSecretAsync(secretFromDb);
                    }

                    return validatedResult.GetResult();
                })
            .Produces<SecretExtendedDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests)
            .WithName("GetSecret");

        // Gets all secrets for auth user in paginated manner
        app.MapGet("/secrets", async (
                [FromHeader] AuthDto authDto,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                ISecretService secretService,
                ITokenService tokenService,
                CancellationToken cancellationToken) =>
            {
                var isTokenValid = await tokenService.IsTokenValidAsync(authDto.Provider, authDto.Token, cancellationToken);

                if (!isTokenValid)
                {
                    return TypedResults.Unauthorized();
                }
                
                var userEmail = await tokenService.GetUserEmailFromTokenAsync(authDto.Provider, authDto.Token, cancellationToken);

                if (string.IsNullOrEmpty(userEmail))
                {
                    return TypedResults.Problem();
                }
                
                // Find all secrets associated with the requested Email
                var paginatedResponse = await secretService
                    .GetSecretsAsync(secret => string.Equals(
                        secret.SharedByEmail.ToLower(),
                        userEmail), page, pageSize);
                Results<Ok<PaginatedResponse<SecretDto>>, UnauthorizedHttpResult, ProblemHttpResult> result = TypedResults.Ok(paginatedResponse);

                return result;
            })
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces<PaginatedResponse<SecretDto>>(StatusCodes.Status200OK);

        app.MapPost("/secrets", async ([FromBody] SecretExtendedDto secretDto, ISecretService secretService) =>
        {
            if (secretDto.AvailableFromUtc > secretDto.AvailableUntilUtc)
            {
                return Results.BadRequest("Expiration date must be greater than 'available from' date. " +
                    $"Start date: '{secretDto.AvailableFromUtc}'. End date: '{secretDto.AvailableUntilUtc}'.");
            }
            var result = await secretService.CreateSecretAsync(secretDto);

            return Results.CreatedAtRoute("GetSecret", new { id = result.Id }, result);
        }).Produces<SecretExtendedDto>(StatusCodes.Status201Created);

        app.MapDelete("/secrets/{removalKeyId:guid}",
            async ([FromRoute] Guid removalKeyId, ISecretService secretService) =>
            {
                var secretToDelete = await secretService.RemoveSecretAsync(removalKeyId);

                if (secretToDelete is null)
                {
                    return Results.NotFound($"Secret with removal key id '{removalKeyId}' not found.");
                }

                return Results.NoContent();
            }).Produces(StatusCodes.Status204NoContent);
    }
}