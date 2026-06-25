using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;
