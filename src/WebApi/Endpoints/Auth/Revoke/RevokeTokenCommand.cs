using MediatR;
using Shared.DTOs.Auth;

namespace WebApi.Endpoints.Auth.Revoke;

public record RevokeTokenCommand(string Token) : IRequest<Unit>;
