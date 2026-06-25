using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
