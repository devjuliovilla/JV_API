using MediatR;
using Application.Features.Auth.Me;

namespace Application.Features.Auth.Me;

public record GetCurrentUserQuery : IRequest<CurrentUserResponse>;
