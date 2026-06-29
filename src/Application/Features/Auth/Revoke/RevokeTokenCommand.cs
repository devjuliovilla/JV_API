using MediatR;
using Application.Features.Auth.Revoke;

namespace Application.Features.Auth.Revoke;

public record RevokeTokenCommand(string Token) : IRequest<Unit>;
