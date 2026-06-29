using MediatR;
using Application.Features.Auth.Refresh;

namespace Application.Features.Auth.Refresh;

public record RefreshTokenCommand(string Token) : IRequest<RefreshTokenResponse>;
