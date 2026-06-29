using MediatR;
using Application.Features.Auth.Login;

namespace Application.Features.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
