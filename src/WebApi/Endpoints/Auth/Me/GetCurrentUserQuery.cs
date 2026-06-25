using MediatR;
using Shared.DTOs.Auth;

namespace WebApi.Endpoints.Auth.Me;

public record GetCurrentUserQuery : IRequest<CurrentUserResponse>;
