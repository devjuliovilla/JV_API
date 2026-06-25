using MediatR;
using Shared.DTOs.Auth;

namespace WebApi.Endpoints.Auth.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;
