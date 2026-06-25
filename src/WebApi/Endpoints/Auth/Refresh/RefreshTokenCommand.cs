using MediatR;
using Shared.DTOs.Auth;

namespace WebApi.Endpoints.Auth.Refresh;

public record RefreshTokenCommand(string Token) : IRequest<RefreshTokenResponse>;
