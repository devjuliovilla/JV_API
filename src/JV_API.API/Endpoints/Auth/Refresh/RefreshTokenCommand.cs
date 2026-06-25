using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Refresh;

public record RefreshTokenCommand(string Token) : IRequest<RefreshTokenResponse>;
