using MediatR;
using Shared.DTOs.Auth;

namespace WebApi.Endpoints.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
