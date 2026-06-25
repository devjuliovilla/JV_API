using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Me;

public record GetCurrentUserQuery : IRequest<CurrentUserResponse>;
