using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Revoke;

public record RevokeTokenCommand(string Token) : IRequest<Unit>;
